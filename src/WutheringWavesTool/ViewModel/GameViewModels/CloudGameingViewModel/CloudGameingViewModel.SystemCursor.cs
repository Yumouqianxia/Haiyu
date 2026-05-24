using System;
using System.Collections.Generic;
using System.Text;

namespace Haiyu.ViewModel.GameViewModels;

partial class CloudGameingViewModel
{
    #region Win32

    private const int CURSOR_SHOWING = 0x00000001;
    private const int HotKeyIdToggleCursor = 0x5141;
    private const uint WindowMessageSubclassId = 0x5141;
    private const uint MOD_ALT = 0x0001;
    private const uint SPI_SETCURSORS = 0x0057;
    private const uint WM_HOTKEY = 0x0312;
    private const uint WM_SETCURSOR = 0x0020;
    private const uint VK_Q = 0x51;
    private delegate bool EnumWindowsProc(IntPtr windowHandle, IntPtr lParam);
    private static readonly uint[] SystemCursorIds =
    [
        32512, // OCR_NORMAL
            32513, // OCR_IBEAM
            32514, // OCR_WAIT
            32515, // OCR_CROSS
            32516, // OCR_UP
            32642, // OCR_SIZENWSE
            32643, // OCR_SIZENESW
            32644, // OCR_SIZEWE
            32645, // OCR_SIZENS
            32646, // OCR_SIZEALL
            32648, // OCR_NO
            32649, // OCR_HAND
            32650, // OCR_APPSTARTING
            32651, // OCR_HELP
            32671, // OCR_PIN
            32672, // OCR_PERSON
        ];


    private delegate IntPtr SUBCLASSPROC(
        IntPtr windowHandle,
        uint message,
        IntPtr wParam,
        IntPtr lParam,
        UIntPtr subclassId,
        UIntPtr referenceData
    );

    [StructLayout(LayoutKind.Sequential)]
    private struct CURSORINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorInfo(out CURSORINFO pci);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetSystemCursor(IntPtr hcur, uint id);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreateCursor(
        IntPtr hInst,
        int xHotSpot,
        int yHotSpot,
        int nWidth,
        int nHeight,
        byte[] pvANDPlane,
        byte[] pvXORPlane
    );

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(
        uint uiAction,
        uint uiParam,
        IntPtr pvParam,
        uint fWinIni
    );

    [DllImport("user32.dll")]
    private static extern IntPtr SetCursor(IntPtr hCursor);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(
        IntPtr hWnd,
        StringBuilder lpClassName,
        int nMaxCount
    );

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumChildWindows(
        IntPtr hWndParent,
        EnumWindowsProc lpEnumFunc,
        IntPtr lParam
    );

    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowSubclass(
        IntPtr hWnd,
        SUBCLASSPROC pfnSubclass,
        UIntPtr uIdSubclass,
        UIntPtr dwRefData
    );

    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveWindowSubclass(
        IntPtr hWnd,
        SUBCLASSPROC pfnSubclass,
        UIntPtr uIdSubclass
    );

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern IntPtr DefSubclassProc(
        IntPtr hWnd,
        uint uMsg,
        IntPtr wParam,
        IntPtr lParam
    );

    [DllImport("user32.dll")]
    private static extern int ShowCursor(bool bShow);

    private static bool IsSystemCursorVisible()
    {
        var cursorInfo = new CURSORINFO
        {
            cbSize = Marshal.SizeOf<CURSORINFO>()
        };

        return GetCursorInfo(out cursorInfo)
            && (cursorInfo.flags & CURSOR_SHOWING) == CURSOR_SHOWING;
    }
    #endregion

    private bool _cursorHidden;
    private bool _webViewCursorSubclassInstalled;
    private nint _webViewChildHandle;

    private bool _isClosingRequested;
    private bool _systemCursorSchemeOverridden;
    private DispatcherTimer _cursorTimer;
    private IntPtr _windowHandle;
    private bool _windowMessageSubclassInstalled;
    private bool _cursorHotKeyRegistered;
    private SUBCLASSPROC _windowMessageSubclassProc;
    private SUBCLASSPROC _webViewCursorSubclassProc;

    private void HideSystemCursor()
    {
        if (_cursorHidden)
        {
            return;
        }

        _cursorHidden = true;

        TryInstallWebViewCursorSubclass();
        OverrideSystemCursorsWithTransparent();
        EnsureSystemCursorHidden();

        if (_cursorTimer is null)
        {
            _cursorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _cursorTimer.Tick += (_, _) =>
            {
                if (_cursorHidden && !_webViewCursorSubclassInstalled)
                {
                    TryInstallWebViewCursorSubclass();
                }

                if (_cursorHidden && IsSystemCursorVisible())
                {
                    EnsureSystemCursorHidden();
                }
            };
        }

        _cursorTimer.Start();
    }

    private static void EnsureSystemCursorHidden()
    {
        while (ShowCursor(false) >= 0) { }
    }

    private void RestoreSystemCursors()
    {
        if (!_systemCursorSchemeOverridden)
        {
            return;
        }

        _ = SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, 0);
        _systemCursorSchemeOverridden = false;
    }

    private void OverrideSystemCursorsWithTransparent()
    {
        if (_systemCursorSchemeOverridden)
        {
            return;
        }

        foreach (var cursorId in SystemCursorIds)
        {
            var transparentCursor = CreateTransparentCursorHandle();
            if (transparentCursor != IntPtr.Zero)
            {
                _ = SetSystemCursor(transparentCursor, cursorId);
            }
        }

        _systemCursorSchemeOverridden = true;
    }


    private static IntPtr CreateTransparentCursorHandle()
    {
        byte[] andMask = [0xFF, 0xFF, 0xFF, 0xFF];
        byte[] xorMask = [0x00, 0x00, 0x00, 0x00];
        return CreateCursor(IntPtr.Zero, 0, 0, 1, 1, andMask, xorMask);
    }

    private void ShowSystemCursor()
    {
        if (!_cursorHidden)
        {
            return;
        }

        _cursorHidden = false;
        _cursorTimer?.Stop();

        RestoreSystemCursors();
        while (ShowCursor(true) < 0) { }
    }



    private void TryInstallWebViewCursorSubclass()
    {
        if (_webViewCursorSubclassInstalled)
        {
            return;
        }

        if (WindowHandle == IntPtr.Zero)
        {
            WindowHandle = Window.GetWindowHandle();
            if (WindowHandle == IntPtr.Zero)
            {
                return;
            }
        }

        _webViewChildHandle = FindWebViewChildWindow(WindowHandle);
        if (_webViewChildHandle == IntPtr.Zero)
        {
            return;
        }

        _webViewCursorSubclassProc ??= WebViewCursorSubclassProc;
        _webViewCursorSubclassInstalled = SetWindowSubclass(
            _webViewChildHandle,
            _webViewCursorSubclassProc,
            UIntPtr.Zero,
            UIntPtr.Zero
        );
    }

    private IntPtr WebViewCursorSubclassProc(
            IntPtr windowHandle,
            uint message,
            IntPtr wParam,
            IntPtr lParam,
            UIntPtr subclassId,
            UIntPtr referenceData
        )
    {
        if (_cursorHidden && message == WM_SETCURSOR)
        {
            SetCursor(IntPtr.Zero);
            return new IntPtr(1);
        }

        return DefSubclassProc(windowHandle, message, wParam, lParam);
    }

    private IntPtr FindWebViewChildWindow(IntPtr parentWindowHandle)
    {
        IntPtr result = IntPtr.Zero;

        EnumChildWindows(
            parentWindowHandle,
            (childHandle, _) =>
            {
                if (IsWebViewWindowClass(childHandle))
                {
                    result = childHandle;
                    return false;
                }

                var descendant = FindWebViewChildWindow(childHandle);
                if (descendant != IntPtr.Zero)
                {
                    result = descendant;
                    return false;
                }

                return true;
            },
            IntPtr.Zero
        );

        return result;
    }

    private static bool IsWebViewWindowClass(IntPtr windowHandle)
    {
        var classNameBuilder = new StringBuilder(256);
        _ = GetClassName(windowHandle, classNameBuilder, classNameBuilder.Capacity);
        var className = classNameBuilder.ToString();

        return className.StartsWith("Chrome_WidgetWin_", StringComparison.Ordinal)
            || className.Contains("WebView", StringComparison.OrdinalIgnoreCase);
    }


    private void ToggleSystemCursor()
    {
        if (_cursorHidden)
        {
            ShowSystemCursor();
            return;
        }

        HideSystemCursor();
    }

    private IntPtr WindowMessageSubclassProc(
            IntPtr windowHandle,
            uint message,
            IntPtr wParam,
            IntPtr lParam,
            UIntPtr subclassId,
            UIntPtr referenceData
        )
    {
        if (message == WM_HOTKEY && wParam == new IntPtr(HotKeyIdToggleCursor))
        {
            ToggleSystemCursor();
            return new IntPtr(1);
        }

        return DefSubclassProc(windowHandle, message, wParam, lParam);
    }

    private void EnsureWindowMessageSubclass()
    {
        if (_windowHandle == IntPtr.Zero)
        {
            _windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this.Window);
            if (_windowHandle == IntPtr.Zero)
            {
                return;
            }
        }

        if (!_windowMessageSubclassInstalled)
        {
            _windowMessageSubclassProc ??= WindowMessageSubclassProc;
            _windowMessageSubclassInstalled = SetWindowSubclass(
                _windowHandle,
                _windowMessageSubclassProc,
                new UIntPtr(WindowMessageSubclassId),
                UIntPtr.Zero
            );
        }

        if (!_cursorHotKeyRegistered)
        {
            _cursorHotKeyRegistered = RegisterHotKey(
                _windowHandle,
                HotKeyIdToggleCursor,
                MOD_ALT,
                VK_Q
            );
        }
    }

    private void RemoveWindowMessageSubclass()
    {
        if (_cursorHotKeyRegistered && _windowHandle != IntPtr.Zero)
        {
            UnregisterHotKey(_windowHandle, HotKeyIdToggleCursor);
            _cursorHotKeyRegistered = false;
        }

        if (_windowMessageSubclassInstalled && _windowHandle != IntPtr.Zero)
        {
            RemoveWindowSubclass(
                _windowHandle,
                _windowMessageSubclassProc,
                new UIntPtr(WindowMessageSubclassId)
            );
            _windowMessageSubclassInstalled = false;
        }
    }
}

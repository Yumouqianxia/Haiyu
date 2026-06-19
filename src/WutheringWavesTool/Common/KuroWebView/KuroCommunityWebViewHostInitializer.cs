using System.Text.Encodings.Web;
using System.Text.Json;

namespace Haiyu.Common.KuroWebView;

public sealed class KuroCommunityWebViewHostInitializer
{
    private const string AndroidAppUserAgent = "Mozilla/5.0 (Linux; Android 13; 23049RAD8C Build/TQ3A.230805.001; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/124.0.0.0 Mobile Safari/537.36 KuroGameBox/2.2.2 KR Android";
    private static readonly Uri KuroMcBoxOrigin = new("https://web-static.kurobbs.com/");

    private string? _documentScriptId;
    private WebSessionContext _currentSession;
    private bool _eventsHooked;

    public async Task InitializeAsync(WebView2 webView, WebSessionContext session)
    {
        _currentSession = session;

        var environment = await CoreWebView2Environment.CreateAsync();
        await webView.EnsureCoreWebView2Async(environment);
        webView.CoreWebView2.Settings.UserAgent = AndroidAppUserAgent;
        webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
        webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
        webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
        webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
        HookEvents(webView);
        await InstallSessionScriptAsync(webView, session);
        ApplyCookieSession(webView, session);
    }

    public async Task ApplySessionAsync(WebView2 webView, WebSessionContext session)
    {
        _currentSession = session;

        if (webView.CoreWebView2 is null)
        {
            throw new InvalidOperationException("WebView2 尚未初始化。");
        }

        ApplyCookieSession(webView, session);
        await webView.CoreWebView2.ExecuteScriptAsync(BuildApplySessionScript(session));
    }

    private async Task InstallSessionScriptAsync(WebView2 webView, WebSessionContext session)
    {
        if (webView.CoreWebView2 is null)
        {
            throw new InvalidOperationException("WebView2 尚未初始化。");
        }

        if (!string.IsNullOrWhiteSpace(_documentScriptId))
        {
            return;
        }

        _documentScriptId = await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(BuildBootstrapScript(session));
    }

    private void HookEvents(WebView2 webView)
    {
        if (_eventsHooked || webView.CoreWebView2 is null)
        {
            return;
        }

        webView.CoreWebView2.NavigationCompleted += async (_, _) =>
        {
            await webView.CoreWebView2.ExecuteScriptAsync(BuildApplySessionScript(_currentSession));
        };

        _eventsHooked = true;
    }

    private static string BuildBootstrapScript(WebSessionContext session)
    {
        var json = JsonSerializer.Serialize(
            CreateBootstrapPayload(session),
            KuroSessionContext.Default.KuroBootstrapPayload);

        return $$"""
            (() => {
                const bootstrapSession = {{json}};
                const asJson = (value) => JSON.stringify(value ?? {});
                const ok = (payload) => asJson(payload);

                const writeValue = (storage, key, value) => {
                    if (!storage || value === undefined || value === null) {
                        return;
                    }

                    try {
                        storage.setItem(key, typeof value === 'string' ? value : JSON.stringify(value));
                    } catch {
                    }
                };

                function getHostAuth() {
                    const fallback = {
                        token: bootstrapSession.token ?? '',
                        did: bootstrapSession.did ?? '',
                        userId: bootstrapSession.userId ?? '',
                        serverId: bootstrapSession.serverId ?? '',
                        roleId: bootstrapSession.roleId ?? '',
                        serverName: bootstrapSession.serverName ?? '',
                        roleName: bootstrapSession.roleName ?? '',
                        requestIp: bootstrapSession.requestIp ?? '',
                        userName: bootstrapSession.userName ?? '',
                        headUrl: bootstrapSession.headUrl ?? '',
                        appVersion: bootstrapSession.appVersion ?? '3.0.2',
                        channelId: bootstrapSession.channelId ?? '2',
                        enterSource: bootstrapSession.enterSource ?? '12',
                        ua: bootstrapSession.ua ?? 'KuroGameBox',
                        os: bootstrapSession.os ?? 'Android',
                        gameId: Number(bootstrapSession.gameId ?? 3) || 3
                    };

                    if (window.__HOST_AUTH__) {
                        return {
                            token: window.__HOST_AUTH__.token ?? fallback.token,
                            did: window.__HOST_AUTH__.did ?? fallback.did,
                            userId: window.__HOST_AUTH__.userId ?? fallback.userId,
                            serverId: window.__HOST_AUTH__.serverId ?? fallback.serverId,
                            roleId: window.__HOST_AUTH__.roleId ?? fallback.roleId,
                            serverName: window.__HOST_AUTH__.serverName ?? fallback.serverName,
                            roleName: window.__HOST_AUTH__.roleName ?? fallback.roleName,
                            requestIp: window.__HOST_AUTH__.requestIp ?? fallback.requestIp,
                            userName: window.__HOST_AUTH__.userName ?? fallback.userName,
                            headUrl: window.__HOST_AUTH__.headUrl ?? fallback.headUrl,
                            appVersion: window.__HOST_AUTH__.appVersion ?? fallback.appVersion,
                            channelId: window.__HOST_AUTH__.channelId ?? fallback.channelId,
                            enterSource: window.__HOST_AUTH__.enterSource ?? fallback.enterSource,
                            ua: window.__HOST_AUTH__.ua ?? fallback.ua,
                            os: window.__HOST_AUTH__.os ?? fallback.os,
                            gameId: Number(window.__HOST_AUTH__.gameId ?? fallback.gameId) || fallback.gameId
                        };
                    }

                    try {
                        return {
                            token: window.localStorage.getItem('token') ?? fallback.token,
                            did: window.localStorage.getItem('did') ?? fallback.did,
                            userId: window.localStorage.getItem('userId') ?? fallback.userId,
                            serverId: window.localStorage.getItem('mc_serverId') ?? fallback.serverId,
                            roleId: window.localStorage.getItem('mc_roleId') ?? fallback.roleId,
                            serverName: fallback.serverName,
                            roleName: window.localStorage.getItem('mc_roleName') ?? fallback.roleName,
                            requestIp: window.localStorage.getItem('REQUEST_IP') ?? fallback.requestIp,
                            userName: fallback.userName,
                            headUrl: fallback.headUrl,
                            appVersion: fallback.appVersion,
                            channelId: fallback.channelId,
                            enterSource: fallback.enterSource,
                            ua: fallback.ua,
                            os: fallback.os,
                            gameId: fallback.gameId
                        };
                    } catch {
                        return fallback;
                    }
                }

                function applySession() {
                    const values = getHostAuth();

                    const userInfo = {
                        appVersion: values.appVersion,
                        os: values.os,
                        headUrl: values.headUrl,
                        userName: values.userName,
                        ua: values.ua,
                        userId: values.userId,
                        did: values.did,
                        channelId: values.channelId,
                        enterSource: values.enterSource,
                        token: values.token
                    };

                    const roleInfo = {
                        userId: values.userId,
                        gameId: values.gameId,
                        serverId: values.serverId,
                        serverName: values.serverName,
                        roleId: values.roleId,
                        roleName: values.roleName
                    };

                    const compactRoleInfo = {
                        gameId: String(values.gameId),
                        roleId: values.roleId,
                        roleName: values.roleName,
                        serverName: values.serverName,
                        userId: values.userId,
                        serverId: values.serverId,
                        token: values.token
                    };

                    for (const storage of [window.localStorage, window.sessionStorage]) {
                        writeValue(storage, 'token', values.token);
                        writeValue(storage, 'did', values.did);
                        writeValue(storage, 'userId', values.userId);
                        writeValue(storage, 'initUserInfo', userInfo);
                        writeValue(storage, 'mc-growth-simulator-user-info', userInfo);
                        writeValue(storage, 'mc-growth-simulator-role-info', roleInfo);
                        writeValue(storage, 'mcResMonReport_APP_USER_INFO', userInfo);
                        writeValue(storage, 'mcResMonReport_ROLE_INFO', roleInfo);
                        writeValue(storage, 'mcResMonReport_GUIDE_ETSRC', values.enterSource);
                        writeValue(storage, 'mc_userInfo', compactRoleInfo);
                        writeValue(storage, 'mc_serverId', values.serverId);
                        writeValue(storage, 'mc_roleId', values.roleId);
                        writeValue(storage, 'mc_roleName', values.roleName);

                        if (values.requestIp) {
                            writeValue(storage, 'REQUEST_IP', values.requestIp);
                        }
                    }

                    window.__HOST_AUTH__ = values;
                    window.__KURO_HOST_ENV__ = {
                        platform: 'android',
                        appName: values.ua
                    };
                }

                function createResponse(handlerName, data) {
                    const values = getHostAuth();
                    const appUserInfo = {
                        token: values.token,
                        did: values.did,
                        userId: values.userId,
                        appVersion: values.appVersion,
                        os: values.os,
                        headUrl: values.headUrl,
                        userName: values.userName,
                        ua: values.ua,
                        channelId: values.channelId,
                        enterSource: values.enterSource
                    };

                    switch (handlerName) {
                        case 'getUserInfo':
                            return ok(appUserInfo);
                        case 'refreshToken':
                        case 'refreshTokenV2':
                            return ok({ code: 0, ...appUserInfo });
                        case 'getSystemStatus':
                            return ok({ darkMode: false, theme: 'light' });
                        case 'setToolInfo':
                        case 'toSkip':
                        case 'appShare':
                        case 'finishPage':
                        case 'useSystemSetting':
                            return ok({ result: true });
                        default:
                            return ok({ result: true, echo: data ?? null, handlerName });
                    }
                }

                const bridge = {
                    init() {
                    },
                    callHandler(handlerName, data, callback) {
                        applySession();
                        const response = createResponse(handlerName, data);
                        if (typeof callback === 'function') {
                            setTimeout(() => callback(response), 0);
                        }
                    },
                    registerHandler(handlerName, callback) {
                        this._handlers = this._handlers || {};
                        this._handlers[handlerName] = callback;
                    }
                };

                window.WebViewJavascriptBridge = bridge;
                window.jsBridge = bridge;
                window.WVJBCallbacks = window.WVJBCallbacks || [];
                applySession();

                setTimeout(() => {
                    document.dispatchEvent(new Event('WebViewJavascriptBridgeReady'));
                }, 0);
            })();
            """;
    }

    private static string BuildApplySessionScript(WebSessionContext session)
    {
        var payload = CreateBootstrapPayload(session);
        var json = JavaScriptEncoder.Default.Encode(
            JsonSerializer.Serialize(payload, KuroSessionContext.Default.KuroBootstrapPayload));

        return $$"""
            (() => {
                const bootstrapSession = JSON.parse('{{json}}');

                const writeValue = (storage, key, value) => {
                    if (!storage || value === undefined || value === null) {
                        return;
                    }

                    try {
                        storage.setItem(key, typeof value === 'string' ? value : JSON.stringify(value));
                    } catch {
                    }
                };

                const values = {
                    token: bootstrapSession.token ?? '',
                    did: bootstrapSession.did ?? '',
                    userId: bootstrapSession.userId ?? '',
                    serverId: bootstrapSession.serverId ?? '',
                    roleId: bootstrapSession.roleId ?? '',
                    serverName: bootstrapSession.serverName ?? '',
                    roleName: bootstrapSession.roleName ?? '',
                    requestIp: bootstrapSession.requestIp ?? '',
                    userName: bootstrapSession.userName ?? '',
                    headUrl: bootstrapSession.headUrl ?? '',
                    appVersion: bootstrapSession.appVersion ?? '3.0.2',
                    channelId: bootstrapSession.channelId ?? '2',
                    enterSource: bootstrapSession.enterSource ?? '12',
                    ua: bootstrapSession.ua ?? 'KuroGameBox',
                    os: bootstrapSession.os ?? 'Android',
                    gameId: Number(bootstrapSession.gameId ?? 3) || 3
                };

                const userInfo = {
                    appVersion: values.appVersion,
                    os: values.os,
                    headUrl: values.headUrl,
                    userName: values.userName,
                    ua: values.ua,
                    userId: values.userId,
                    did: values.did,
                    channelId: values.channelId,
                    enterSource: values.enterSource,
                    token: values.token
                };

                const roleInfo = {
                    userId: values.userId,
                    gameId: values.gameId,
                    serverId: values.serverId,
                    serverName: values.serverName,
                    roleId: values.roleId,
                    roleName: values.roleName
                };

                const compactRoleInfo = {
                    gameId: String(values.gameId),
                    roleId: values.roleId,
                    roleName: values.roleName,
                    serverName: values.serverName,
                    userId: values.userId,
                    serverId: values.serverId,
                    token: values.token
                };

                for (const storage of [window.localStorage, window.sessionStorage]) {
                    writeValue(storage, 'token', values.token);
                    writeValue(storage, 'did', values.did);
                    writeValue(storage, 'userId', values.userId);
                    writeValue(storage, 'initUserInfo', userInfo);
                    writeValue(storage, 'mc-growth-simulator-user-info', userInfo);
                    writeValue(storage, 'mc-growth-simulator-role-info', roleInfo);
                    writeValue(storage, 'mcResMonReport_APP_USER_INFO', userInfo);
                    writeValue(storage, 'mcResMonReport_ROLE_INFO', roleInfo);
                    writeValue(storage, 'mcResMonReport_GUIDE_ETSRC', values.enterSource);
                    writeValue(storage, 'mc_userInfo', compactRoleInfo);
                    writeValue(storage, 'mc_serverId', values.serverId);
                    writeValue(storage, 'mc_roleId', values.roleId);
                    writeValue(storage, 'mc_roleName', values.roleName);

                    if (values.requestIp) {
                        writeValue(storage, 'REQUEST_IP', values.requestIp);
                    }
                }

                window.__HOST_AUTH__ = values;
                window.__KURO_HOST_ENV__ = {
                    platform: 'android',
                    appName: values.ua
                };

                if (!window.jsBridge && window.WebViewJavascriptBridge) {
                    window.jsBridge = window.WebViewJavascriptBridge;
                }
            })();
            """;
    }

    private static KuroBootstrapPayload CreateBootstrapPayload(WebSessionContext session)
    {
        return new KuroBootstrapPayload
        {
            Token = session.Token,
            Did = session.Did,
            UserId = session.UserId,
            ServerId = session.ServerId,
            RoleId = session.RoleId,
            ServerName = session.ServerName,
            RoleName = session.RoleName,
            RequestIp = session.RequestIp,
            UserName = session.UserName,
            HeadUrl = session.HeadUrl,
            AppVersion = session.AppVersion,
            ChannelId = session.ChannelId,
            EnterSource = session.EnterSource,
            UserAgentName = session.UserAgentName,
            Os = session.Os,
            GameId = session.GameId
        };
    }

    private static void ApplyCookieSession(WebView2 webView, WebSessionContext session)
    {
        if (webView.CoreWebView2 is null)
        {
            return;
        }

        var cookieManager = webView.CoreWebView2.CookieManager;
        SetCookie(cookieManager, "token", session.Token);
        SetCookie(cookieManager, "did", session.Did);
        SetCookie(cookieManager, "userId", session.UserId);
    }

    private static void SetCookie(CoreWebView2CookieManager cookieManager, string name, string value)
    {
        var cookie = cookieManager.CreateCookie(name, value ?? string.Empty, KuroMcBoxOrigin.Host, "/");
        cookie.IsHttpOnly = false;
        cookie.IsSecure = true;
        cookie.SameSite = CoreWebView2CookieSameSiteKind.None;
        cookie.Expires = DateTime.Now.AddDays(7).Ticks;
        cookieManager.AddOrUpdateCookie(cookie);
    }
}

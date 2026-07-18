using Haiyu.Models;
using Waves.Core.Settings;

namespace Haiyu.Services;

public sealed class WavesChannelService : IWavesChannelService
{
    private const string ThirdPartyRelativePath = @"Client\Binaries\Win64\ThirdParty\KrPcSdk_Mainland";
    private const string LauncherDownloadConfigName = "launcherDownloadConfig.json";
    private const string LocalGameResourcesName = "LocalGameResources.json";
    private const string LauncherDownloadName = "launcherDownload";
    private const string RailGameIdentifyRelativePath = @"rail_files\rail_game_identify.json";
    private const string RailGameBuildInfoRelativePath = @"rail_files\rail_game_build_info.dat";
    private const string RailKeyFileListRelativePath = @"rail_files\RailKeyFileList.dat";

    private static readonly string[] CommonRequiredRelativePaths =
    [
        ThirdPartyRelativePath,
        LauncherDownloadConfigName,
        LocalGameResourcesName,
        LauncherDownloadName,
    ];

    private static readonly string[] WeGameRequiredRelativePaths =
    [
        RailGameIdentifyRelativePath,
        RailGameBuildInfoRelativePath,
    ];

    private static readonly string[] WeGameOptionalRelativePaths =
    [
        RailKeyFileListRelativePath,
    ];

    public string GetDefaultBackupRoot()
    {
        return Path.Combine(AppSettings.BassFolder, "ChannelBackups", "Waves");
    }

    public string GetBackupPath(WavesChannel channel)
    {
        return Path.Combine(GetDefaultBackupRoot(), channel.ToString());
    }

    public async Task<WavesChannelStatus> GetStatusAsync(
        string gameFolder,
        WavesChannel selectedChannel,
        CancellationToken token = default
    )
    {
        var currentChannel = await DetectChannelAsync(gameFolder, token);
        var activeComplete = IsPackageComplete(gameFolder, currentChannel);
        var backupComplete = IsPackageComplete(GetBackupPath(selectedChannel), selectedChannel);
        var currentText = GetChannelDisplay(currentChannel);
        var selectedText = GetChannelDisplay(selectedChannel);
        var message =
            string.IsNullOrWhiteSpace(gameFolder)
                ? "未设置游戏目录"
                : $"当前：{currentText}；目标备份：{selectedText} {(backupComplete ? "可用" : "缺失")}";

        if (!activeComplete)
        {
            message += "；当前目录渠道文件不完整";
        }

        return new WavesChannelStatus
        {
            CurrentChannel = currentChannel,
            ActivePackageComplete = activeComplete,
            SelectedBackupExists = backupComplete,
            Message = message,
        };
    }

    public async Task<WavesChannel> DetectChannelAsync(string gameFolder, CancellationToken token = default)
    {
        var configPath = ResolveKrSdkConfigPath(gameFolder);
        if (!File.Exists(configPath))
        {
            return WavesChannel.Unknown;
        }

        try
        {
            var json = await File.ReadAllTextAsync(configPath, token);
            var node = JsonNode.Parse(json);
            var productId = ReadJsonValue(node, "KR_ProductId");
            var channelId = ReadJsonValue(node, "KR_ChannelId");
            var railId = ReadJsonValue(node, "KR_RailId");

            if (productId == "A1440" || channelId == "167" || railId == "2002137")
            {
                return WavesChannel.WeGame;
            }

            if (productId == "A1421")
            {
                return WavesChannel.Bilibili;
            }

            if (productId == "A1381" || channelId == "19")
            {
                return WavesChannel.Official;
            }
        }
        catch
        {
            return WavesChannel.Unknown;
        }

        return WavesChannel.Unknown;
    }

    private static string ResolveKrSdkConfigPath(string gameFolder)
    {
        var thirdPartyPath = Path.Combine(gameFolder ?? string.Empty, ThirdPartyRelativePath);
        var candidates = new[]
        {
            Path.Combine(thirdPartyPath, "KRSDKRes", "KRSDKConfig.json"),
            Path.Combine(thirdPartyPath, "KRSDKConfig.json"),
        };

        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }

    public async Task BackupAsync(
        string gameFolder,
        WavesChannel channel,
        bool overwrite,
        CancellationToken token = default
    )
    {
        ValidateChannel(channel);
        ValidateGameFolder(gameFolder);
        if (!IsPackageComplete(gameFolder, channel))
        {
            throw new InvalidOperationException("当前游戏目录渠道文件不完整，无法备份。");
        }

        var backupPath = GetBackupPath(channel);
        if (Directory.Exists(backupPath))
        {
            if (!overwrite)
            {
                throw new InvalidOperationException("目标渠道备份已存在。");
            }

            Directory.Delete(backupPath, true);
        }

        Directory.CreateDirectory(backupPath);
        await CopyPackageAsync(gameFolder, backupPath, channel, token);
    }

    public async Task RestoreAsync(string gameFolder, WavesChannel channel, CancellationToken token = default)
    {
        ValidateChannel(channel);
        ValidateGameFolder(gameFolder);

        var backupPath = GetBackupPath(channel);
        if (!IsPackageComplete(backupPath, channel))
        {
            throw new InvalidOperationException($"缺少 {GetChannelDisplay(channel)} 渠道备份，请先备份或用对应启动器修复后刷新备份。");
        }

        var currentChannel = await DetectChannelAsync(gameFolder, token);
        if (IsPackageComplete(gameFolder, currentChannel))
        {
            // Always capture the active channel before a switch. This also refreshes
            // channel metadata after an official launcher or WeGame repair/update.
            await BackupAsync(gameFolder, currentChannel, true, token);
        }

        if (currentChannel == channel)
        {
            return;
        }

        try
        {
            DeletePackage(gameFolder, channel);
            await CopyPackageAsync(backupPath, gameFolder, channel, token);
        }
        catch
        {
            if (currentChannel != WavesChannel.Unknown)
            {
                DeletePackage(gameFolder, currentChannel);
                var currentBackupPath = GetBackupPath(currentChannel);
                if (IsPackageComplete(currentBackupPath, currentChannel))
                {
                    await CopyPackageAsync(currentBackupPath, gameFolder, currentChannel, token);
                }
            }

            throw;
        }
    }

    private static string ReadJsonValue(JsonNode? node, string key)
    {
        return node?[key]?.ToString() ?? string.Empty;
    }

    private static bool IsPackageComplete(string folder, WavesChannel channel)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return false;
        }

        return GetRequiredRelativePaths(channel).All(relativePath =>
        {
            var fullPath = Path.Combine(folder, relativePath);
            return File.Exists(fullPath) || Directory.Exists(fullPath);
        });
    }

    private static async Task CopyPackageAsync(
        string sourceRoot,
        string targetRoot,
        WavesChannel channel,
        CancellationToken token
    )
    {
        foreach (var relativePath in GetRequiredRelativePaths(channel))
        {
            token.ThrowIfCancellationRequested();
            if (!CopyPackageItem(sourceRoot, targetRoot, relativePath, true))
            {
                throw new FileNotFoundException($"缺少渠道文件：{relativePath}", Path.Combine(sourceRoot, relativePath));
            }

            await Task.Yield();
        }

        foreach (var relativePath in GetOptionalRelativePaths(channel))
        {
            token.ThrowIfCancellationRequested();
            CopyPackageItem(sourceRoot, targetRoot, relativePath, false);
            await Task.Yield();
        }
    }

    private static bool CopyPackageItem(
        string sourceRoot,
        string targetRoot,
        string relativePath,
        bool required
    )
    {
        var source = Path.Combine(sourceRoot, relativePath);
        var target = Path.Combine(targetRoot, relativePath);

        if (Directory.Exists(source))
        {
            CopyDirectory(source, target);
            return true;
        }

        if (File.Exists(source))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(source, target, true);
            return true;
        }

        return !required;
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);
        foreach (var file in Directory.EnumerateFiles(sourceDirectory))
        {
            var targetFile = Path.Combine(targetDirectory, Path.GetFileName(file));
            File.Copy(file, targetFile, true);
        }

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory))
        {
            CopyDirectory(directory, Path.Combine(targetDirectory, Path.GetFileName(directory)));
        }
    }

    private static void DeletePackage(string gameFolder, WavesChannel channel)
    {
        foreach (var relativePath in GetRequiredRelativePaths(channel).Concat(GetOptionalRelativePaths(channel)))
        {
            var fullPath = Path.Combine(gameFolder, relativePath);
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
            else if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }

    private static IEnumerable<string> GetRequiredRelativePaths(WavesChannel channel)
    {
        return channel == WavesChannel.WeGame
            ? CommonRequiredRelativePaths.Concat(WeGameRequiredRelativePaths)
            : CommonRequiredRelativePaths;
    }

    private static IEnumerable<string> GetOptionalRelativePaths(WavesChannel channel)
    {
        return channel == WavesChannel.WeGame
            ? WeGameOptionalRelativePaths
            : [];
    }

    private static void ValidateChannel(WavesChannel channel)
    {
        if (channel == WavesChannel.Unknown)
        {
            throw new InvalidOperationException("请选择明确的渠道。");
        }
    }

    private static void ValidateGameFolder(string gameFolder)
    {
        if (string.IsNullOrWhiteSpace(gameFolder) || !Directory.Exists(gameFolder))
        {
            throw new DirectoryNotFoundException("游戏目录不存在。");
        }
    }

    private static string GetChannelDisplay(WavesChannel channel)
    {
        return channel switch
        {
            WavesChannel.Official => "官方",
            WavesChannel.Bilibili => "B服",
            WavesChannel.WeGame => "WeGame",
            _ => "未知",
        };
    }
}

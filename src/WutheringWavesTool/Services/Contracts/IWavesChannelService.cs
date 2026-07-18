using Haiyu.Models;

namespace Haiyu.Services.Contracts;

public interface IWavesChannelService
{
    string GetDefaultBackupRoot();

    string GetBackupPath(WavesChannel channel);

    Task<WavesChannelStatus> GetStatusAsync(string gameFolder, WavesChannel selectedChannel, CancellationToken token = default);

    Task<WavesChannel> DetectChannelAsync(string gameFolder, CancellationToken token = default);

    Task BackupAsync(string gameFolder, WavesChannel channel, bool overwrite, CancellationToken token = default);

    Task RestoreAsync(string gameFolder, WavesChannel channel, CancellationToken token = default);
}

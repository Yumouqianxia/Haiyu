using Haiyu.Plugin.Common.LegacyMessageBox;
using MemoryPack;
using Waves.Core.Contracts.CloudGame;
using Waves.Core.Models.CloudGame;
using Waves.Core.Services;

namespace Haiyu.ViewModel;

/// <summary>
/// 鸣潮抽卡分析
/// </summary>
public sealed partial class WavesAnalysisRecordViewModel : WindowViewModelBase
{
    public Window Window { get; set; }

    public readonly IKuroCloudGameContext CloudGameContext;
    public CloudGameLoginSession Session { get; set; }

    public WavesAnalysisRecordViewModel(
        [FromKeyedServices(nameof(KuroCloudGameContext))] IKuroCloudGameContext cloudGameContext
    )
    {
        this.CloudGameContext = cloudGameContext;
    }

    [RelayCommand]
    async Task Loaded()
    {
        try
        {
            await LoadDataAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task LoadDataAsync()
    {
        var path = Path.Combine(
            Waves.Core.Settings.AppSettings.RecordFolder,
            $"{this.Session.OrginData.Username}"
        );
        using (
            var fs = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                4096,
                true
            )
        )
        {
            var datas = await MemoryPackSerializer.DeserializeAsync<RecordCacheDetily>(
                fs,
                new MemoryPackSerializerOptions() { StringEncoding = StringEncoding.Utf8 }
            );
        }
    }

    internal void SetSessionAsync(CloudGameLoginSession session)
    {
        var result = this.CloudGameContext.WavesCloudSurivivalService.Cache.TryGet(
            session.OrginData.Username + session.OrginData.Sdkuserid
        );
        LegacyMessageBox.ShowError("账号异常……");
        this.Window.Close();
    }
}

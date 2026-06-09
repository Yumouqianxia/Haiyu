using Haiyu.Plugin.Common.LegacyMessageBox;
using Haiyu.Plugin.Contracts;
using Haiyu.Plugin.Models.Enums;
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
    public readonly IKuroCloudGameContext CloudGameContext;
    public CloudGameLoginSession Session { get; set; }
    public IWavesPlayerCardCacheServices Cache { get; }
    public FiveGroupModel FiveGroup { get; private set; }
    public List<CommunityRoleData> AllRole { get; private set; }
    public List<CommunityWeaponData> AllWeapon { get; private set; }
    private WavesAnalysisPlayerCard Cards { get; set; }
    public WavesAnalysisRecordViewModel(
        [FromKeyedServices(nameof(KuroCloudGameContext))] IKuroCloudGameContext cloudGameContext,
        IWavesPlayerCardCacheServices cache
    )
    {
        this.CloudGameContext = cloudGameContext;
        Cache = cache;
    }

    [RelayCommand]
    async Task Loaded()
    {
        try
        {
            await LoadDataAsync();
            await InitAnalysis();
            await AnalysisStarAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task InitAnalysis()
    {
        this.FiveGroup = await RecordHelper.GetFiveGroupAsync(this.CTS.Token)??new();
        this.AllRole = await RecordHelper.GetAllRoleAsync(this.CTS.Token)??new();
        this.AllWeapon = await RecordHelper.GetAllWeaponAsync(this.CTS.Token)??new();
        InitNavItems();

    }

    private async Task LoadDataAsync()
    {
        await MargeNewRecordPlayerAsync();
    }

    async Task MargeNewRecordPlayerAsync()
    {
        var newRecord =
            await this.CloudGameContext.WavesCloudSurivivalService.WavesCloudGameService.GetRecordAsync(
                this.Session,
                this.CTS.Token
            );
        if (newRecord == null || newRecord.Data == null)
        {
            return;
        }
        WavesAnalysisPlayerCard cards = new()
        {
            Items = new List<WavesAnalysisPlayerCardItem>(),
            LastUpdater = DateTime.Now,
            SessionId = this.Session.OrginData.Username + this.Session.OrginData.Sdkuserid,
        };
        foreach (var item in CardPoolTypeValues.All)
        {
            var resources =
                await this.CloudGameContext.WavesCloudSurivivalService.WavesCloudGameService.GetGameRecordResource(
                    Session,
                    newRecord.Data.RecordId,
                    newRecord.Data.PlayerId.ToString(),
                    (int) item
                );
            if (resources?.Data != null)
            {
                cards.Items.Add(
                    new WavesAnalysisPlayerCardItem()
                    {
                        PoolType = (int) item,
                        Resource = resources.Data.Select(
                            x => new RecordCardItemWrapper(x)
                        ),
                        
                    }
                );
            }
        }
        this.Cards  = await Cache.SaveAsync(cards);
    }

    internal void SetSessionAsync(CloudGameLoginSession session)
    {
        var result = this.CloudGameContext.WavesCloudSurivivalService.Cache.TryGet(
            session.OrginData.Username + session.OrginData.Sdkuserid
        );
        if (result == null)
        {
            LegacyMessageBox.ShowError("账号异常……");
            this.Window.Close();
        }
        this.Session = session;
    }
}

using Waves.Core.Contracts.CloudGame;
using Waves.Core.Models.CloudGame;
using Waves.Core.Services;

namespace Haiyu.ViewModel;

/// <summary>
/// 鸣潮抽卡分析
/// </summary>
public sealed partial class WavesAnalysisRecordViewModel:WindowViewModelBase
{
    public Window Window { get; set; }
    
    public readonly IKuroCloudGameContext CloudGameContext;

    public CloudGameLoginSession Session { get; set; }

    public WavesAnalysisRecordViewModel([FromKeyedServices(nameof(KuroCloudGameContext))]IKuroCloudGameContext cloudGameContext)
    {
        this.CloudGameContext = cloudGameContext;
    }


}

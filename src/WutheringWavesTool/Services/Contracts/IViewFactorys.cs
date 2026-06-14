using Waves.Api.Models.CloudGame;
using Waves.Core.Models.CloudGame;

namespace Haiyu.Services.Contracts;

public interface IViewFactorys
{
    public IAppContext<App> AppContext { get; }
    public GetGeetWindow CreateGeetWindow(GeetType type);

    public WindowModelBase ShowSignWindow(GameRoilDataItem role);

    public WindowModelBase ShowRolesDataWindow(ShowRoleData detily);

    public WindowModelBase ShowWavesDataCenter(GameRoilDataItem item);

    public TransparentWindow CreateTransperentWindow();

    public WindowModelBase ShowAdminDevice();

    public WindowModelBase ShowAnalysisRecordV2(CloudGameLoginSession selectLogin);
}

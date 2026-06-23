using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.AppLifecycle;
using Windows.UI.StartScreen;

namespace Haiyu.Services.Contracts;

public interface IAppActivation
{
    public Task ExecLaunchActivatedEventArgs(AppActivationArguments e);

    public Task<JumpListItem?> CreateJumpListsAndInitCoreAsync(IGameContextV2 context);
}

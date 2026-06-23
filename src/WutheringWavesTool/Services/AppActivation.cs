using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;
using Windows.UI.StartScreen;
using WinRT;

namespace Haiyu.Services
{
    public sealed partial class AppActivation : IAppActivation
    {
        public async Task<JumpListItem?> CreateJumpListsAndInitCoreAsync(IGameContextV2 context)
        {
            var status = await context.GetGameContextStatusAsync();
            var displayName = context.DisplayName;
            if (status.IsGameExists && status.IsLauncher)
            {
                var jumpItem = JumpListItem.CreateWithArguments($"{context.ContextName}/startGame", "startGame");
                jumpItem.GroupName = "快捷启动";
                jumpItem.Description = $"启动{displayName}";
                jumpItem.DisplayName = $"启动{displayName}";
                return jumpItem;
            }
            return null;
        }
        public async Task ExecLaunchActivatedEventArgs(AppActivationArguments e)
        {
            if (e.Kind == Microsoft.Windows.AppLifecycle.ExtendedActivationKind.File) { }
            if (e.Kind == ExtendedActivationKind.Launch)
            {
                string[] argumentsArray = Environment.GetCommandLineArgs();
                if (e.Data is ILaunchActivatedEventArgs launchArgs)
                {
                    var argument = launchArgs.Arguments.Split(" ");
                    var launcheArgument = argument.Last().Split("/");
                    //快速启动游戏
                    if (launcheArgument.Length == 2 && launcheArgument[1] == "startGame")
                    {
                        var context = Instance.Host.Services.GetRequiredKeyedService<IGameContextV2>(launcheArgument[0]);
                        await context.StartGameAsync();
                    }
                }
            }
        }
    }
}

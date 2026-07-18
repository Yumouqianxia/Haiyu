using System;
using System.Collections.Generic;
using System.Text;
using Waves.Core.Models.Enums;

namespace Haiyu.Models.GameConfig;

public class GameSettingDialogConfig
{
    public string CoreName { get; set; }

    public GameSettingDialogConfig(string coreName)
    {
        CoreName = coreName;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Waves.Core.Models.Enums;

namespace Haiyu.Models;

public class CloudQualityUpdateModel
{
    public int Fps { get; set; }

    public CloudQualityType Type { get; set; }

    public bool NetworkShow { get; set; }

    public bool QaulityEnable { get; set; }
}

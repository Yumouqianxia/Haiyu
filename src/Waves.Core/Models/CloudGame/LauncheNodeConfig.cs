using System;
using System.Collections.Generic;
using System.Text;
using Waves.Api.Models.CloudGame;

namespace Waves.Core.Models.CloudGame;

public class LauncheNodeConfig
{
    public IEnumerable<CloudGameNode> Nodes { get; set; }

    public CloudGameNode SelectNode { get; set; }
}
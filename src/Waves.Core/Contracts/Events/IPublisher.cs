using System;
using System.Collections.Generic;
using System.Text;

namespace Waves.Core.Contracts.Events;

public interface IPublisher
{
    public void Unsubscribe(Guid id);
}

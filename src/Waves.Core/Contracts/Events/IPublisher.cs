namespace Waves.Core.Contracts.Events;

public interface IPublisher
{
    public void Unsubscribe(Guid id);
}
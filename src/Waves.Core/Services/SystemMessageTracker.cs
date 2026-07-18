using System.Collections.ObjectModel;

namespace Waves.Core.Services
{
    public class SystemMessageTracker : TrackerBase<SystemMessageTracker, SystemMessagerModel>
    {
        public ObservableCollection<SystemMessagerModel> Messages { get; } = new();

        public async override ValueTask HandleEventAsync(SystemMessagerModel args)
        {
            if (args == null || string.IsNullOrWhiteSpace(args.Message))
                return;

            Messages.Add(args);
            _isDirty = true;

            if (Messages.Count > 500)
                Messages.RemoveAt(0);
        }

        public  override Task OnVirualDispose()
        {
            Messages.Clear();
            _isDirty = false;
            return Task.CompletedTask;
        }

        public override void Invoke() => onTrackerHandle?.Invoke(this);
    }
}

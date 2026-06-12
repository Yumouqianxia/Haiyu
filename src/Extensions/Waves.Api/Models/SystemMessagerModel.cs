using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Waves.Api.Models.Messanger;

namespace Waves.Api.Models;

public class SystemMessagerModel
{
    public DateTime Time
    {
        get => field;
        set
        {
            field = value;
            this.TimeString = value.ToString("HH:mm:ss");
        }
    }

    public string TimeString { get; set; }
    public string Message { get; set; }

    /// <summary>
    /// 延迟关闭，0为不关闭
    /// </summary>
    public double Delay { get; set; }

    public IRelayCommand RemoveCommand =>
        new RelayCommand(() => WeakReferenceMessenger.Default.Send<SystemMessageClose>(new(this)));
}

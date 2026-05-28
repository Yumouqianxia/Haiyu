using Waves.Api.Models.CloudGame;
using Waves.Core.Models.CloudGame;

namespace Haiyu.Pages.Dialogs
{
    public sealed partial class CloudSelectNodeDialog : ContentDialog,IResultDialog<LauncheNodeConfig>
    {
        public CloudSelectNodeDialog()
        {
            InitializeComponent();
            this.ViewModel = Instance.Host.Services.GetRequiredService<CloudSelectNodeViewModel>();
            
        }

        public CloudSelectNodeViewModel ViewModel { get; }

        public LauncheNodeConfig? GetResult()
        {
            return new()
            {
                Nodes = ViewModel.Nodes,
                SelectNode = ViewModel.SelectNode
            };
        }

        public void SetData(object data)
        {
            if(data is string strValue)
            {
                this.ViewModel.Id = strValue;
            }
        }
    }
}

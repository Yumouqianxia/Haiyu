using Waves.Api.Models.CloudGame;

namespace Haiyu.Pages.Dialogs
{
    public sealed partial class CloudSelectNodeDialog : ContentDialog,IResultDialog<CloudGameNode>
    {
        public CloudSelectNodeDialog()
        {
            InitializeComponent();
            this.ViewModel = Instance.Host.Services.GetRequiredService<CloudSelectNodeViewModel>();
            
        }

        public CloudSelectNodeViewModel ViewModel { get; }

        public CloudGameNode? GetResult()
        {
            return this.ViewModel.SelectNode;
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

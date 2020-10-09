using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwnLink.ViewModel;
using OwnLink.View;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OwnLink.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterDetailMain : MasterDetailPage
    {
        public MasterDetailMainViewModel mdmvm { get; set; }
        public int flag;
        public MasterDetailMain()
        {
            InitializeComponent();
            mdmvm = new MasterDetailMainViewModel() {Navigation = this.Navigation };
            this.BindingContext = mdmvm;
            flag = 0;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            

            MessagingCenter.Subscribe<string, string>("Call", "CallState", (sender, arg) =>
            {
                showCall(arg.Trim());
            });

            mdmvm.startPage();

            if (flag == 0 )
            {
                Detail = new NavigationPage(new HistoryCallPage());
                IsPresented = true;
                NavigationPage.SetHasNavigationBar(this, false);
                NavigationPage.SetHasBackButton(this, false);
                flag = 1;
            }

            
        }        
        
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            mdmvm.endPage();
            MessagingCenter.Unsubscribe<string, string>("Call", "CallState");
        }

        private void historyCall_Clicked(object sender, EventArgs e)
        {
            Detail = new NavigationPage(new HistoryCallPage());
            IsPresented = false;
        }

        private void setting_Clicked(object sender, EventArgs e)
        {
            Detail = new NavigationPage(new SettingPage());
            IsPresented = false;
        }

        private void showCall(string content)
        {
            if (content == "Incoming")
            {
                Detail = new NavigationPage(new CallPage());
                IsPresented = false;
            }
            if (content == "End")
            {
                Detail = new NavigationPage(new HistoryCallPage());
                IsPresented = false;
            }
        }
    }
}
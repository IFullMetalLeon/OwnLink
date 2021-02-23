using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwnLink.ViewModel;
using OwnLink.View;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;

namespace OwnLink.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterDetailMain : MasterDetailPage
    {
        public MasterDetailMainViewModel mdmvm { get; set; }
        public int flag;
        public int isCallShow;
        public MasterDetailMain()
        {
            InitializeComponent();
            mdmvm = new MasterDetailMainViewModel() {Navigation = this.Navigation };
            this.BindingContext = mdmvm;
            flag = 0;
            isCallShow = 0;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            

            MessagingCenter.Subscribe<string, string>("Call", "CallState", (sender, arg) =>
            {
                showCall(arg.Trim());
            });

            mdmvm.startPage();
            isCallShow = 0;
            if (flag == 0 )
            {
                Detail = new NavigationPage(new HistoryCallPage()) { BarBackgroundColor = Color.FromHex("#FFFFFF"), BarTextColor = Color.FromHex("#000000") };
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

        private void activeCall_Clicked(object sender, EventArgs e)
        {           
            if (mdmvm.Core.CallsNb > 0)
            {
                Detail = new NavigationPage(new CallPage());
                IsPresented = false;
            }
        }

        private void historyCall_Clicked(object sender, EventArgs e)
        {
            Detail = new NavigationPage(new HistoryCallPage()) { BarBackgroundColor = Color.FromHex("#FFFFFF"), BarTextColor = Color.FromHex("#000000") };
            IsPresented = false;
        }

        private void setting_Clicked(object sender, EventArgs e)
        {
            Detail = new NavigationPage(new SettingPage()) { BarBackgroundColor = Color.FromHex("#FFFFFF"), BarTextColor = Color.FromHex("#000000") };
            IsPresented = false;
        }

        private void showCall(string content)
        {
            if (content == "Incoming")
            {
                if (isCallShow == 0)
                {
                    isCallShow = 1;
                    Detail = new NavigationPage(new CallPage());
                    IsPresented = false;
                }
            }
            if (content == "End")
            {
                mdmvm.Core.TerminateAllCalls();
                isCallShow = 0;
                Detail = new NavigationPage(new HistoryCallPage()) { BarBackgroundColor = Color.FromHex("#FFFFFF"), BarTextColor = Color.FromHex("#000000") };
                IsPresented = false;
            }
        }
    }
}
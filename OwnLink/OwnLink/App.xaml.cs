using Linphone;
using OwnLink.View;
using Plugin.Settings;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using OwnLink.ViewModel;
using Plugin.DeviceInfo;

namespace OwnLink
{
    public partial class App : Application
    {
        public static string ConfigFilePath { get; set; }
        public static string FactoryFilePath { get; set; }

        public int flag;
        public LinphoneManager Manager { get; set; }
        public int flagSleep { get; set; }
        public Core Core
        {
            get
            {
                return Manager.Core;
            }
        }

        public App(IntPtr context)
        {
            InitializeComponent();
            
            string _login = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");
            string deviceId = CrossDeviceInfo.Current.Id;
            string deviceInfo = CrossDeviceInfo.Current.Manufacturer + " " + CrossDeviceInfo.Current.Model + " " + CrossDeviceInfo.Current.Platform + " " + CrossDeviceInfo.Current.Version;
            try
            {
                Manager = new LinphoneManager();
                Manager.Init(ConfigFilePath, FactoryFilePath, context);
            }
            catch(Exception ex)
            {
                HttpControler.ErrorLogSend(_login, deviceInfo, deviceId, "Core init fail. App.xaml.cs "+ex.Message);
            }
            //MainPage = new NavigationPage(new TestPage());
            if (_login == "")
            {
                MainPage = new NavigationPage(new LoginPage()) { BarBackgroundColor = Color.FromHex("#FFFFFF"), BarTextColor = Color.FromHex("#000000") };
                //((NavigationPage)Xamarin.Forms.Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#FF4C3F");


            }
            else
            {
                MainPage = new NavigationPage(new MasterDetailMain()) { BarBackgroundColor = Color.FromHex("#FFFFFF"), BarTextColor = Color.FromHex("#000000") };
                //((NavigationPage)Xamarin.Forms.Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#FF4C3F");
            }
            
            flagSleep = 0;
        }

        public StackLayout getLayoutView()
        {
            return MainPage.FindByName<StackLayout>("stack_layout");
        }

        protected override void OnStart()
        {
            Manager.Start();
            flagSleep = 0;
        }

        protected override void OnSleep()
        {
            Core.EnterBackground();
            flagSleep = 1;
        }

        protected override void OnResume()
        {
            Core.EnsureRegistered();
            flagSleep = 0;
        }       
    }
}

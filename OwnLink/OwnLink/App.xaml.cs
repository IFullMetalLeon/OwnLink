using Linphone;
using OwnLink.View;
using Plugin.Settings;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using OwnLink.ViewModel;

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

            Manager = new LinphoneManager();
            Manager.Init(ConfigFilePath, FactoryFilePath, context);

            string _login = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");
            if (_login == "")
            {
                MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                MainPage = new NavigationPage(new MasterDetailMain());
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

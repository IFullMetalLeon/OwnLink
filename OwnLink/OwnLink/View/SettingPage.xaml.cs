using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwnLink.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OwnLink.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPage : ContentPage
    {
        public SettingPageViewModel spvm { get; set; }
        public SettingPage()
        {
            InitializeComponent();
            spvm = new SettingPageViewModel();
            this.BindingContext = spvm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            spvm.startPage();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            spvm.endPage();
        }
    }
}
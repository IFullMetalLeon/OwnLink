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
    public partial class LoginPage : ContentPage
    {
        public LoginPageViewModel lpvm { get; set; }
        public LoginPage()
        {
            InitializeComponent();
            lpvm = new LoginPageViewModel() { Navigation = this.Navigation };
            this.BindingContext = lpvm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            lpvm.startPage();           
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            lpvm.endPage();
        }

        private void codeSmsEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            lpvm.checkCode();
        }

        private void countryList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            lpvm.SelectCountry();
        }

        private void counrtySearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            lpvm.SearchCountry();
        }

        private void phoneEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            lpvm.checkPhone();
        }
    }
}
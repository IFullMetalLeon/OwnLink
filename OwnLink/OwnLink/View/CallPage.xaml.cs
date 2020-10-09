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
    public partial class CallPage : ContentPage
    {
        public CallPageViewModel cpvm { get; set; }
        public CallPage()
        {
            InitializeComponent();
            cpvm = new CallPageViewModel() {Navigation = this.Navigation };
            this.BindingContext = cpvm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            cpvm.startPage();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            cpvm.endPage();
        }
    }
}
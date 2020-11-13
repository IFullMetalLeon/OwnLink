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
    public partial class TestPage : ContentPage
    {
        TestPageViewModel tpvm { get; set; }
        public TestPage()
        {
            InitializeComponent();
            tpvm = new TestPageViewModel();
            this.BindingContext = tpvm;
        }
    }
}
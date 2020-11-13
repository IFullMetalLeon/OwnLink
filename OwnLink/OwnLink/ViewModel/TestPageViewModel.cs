using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Acr.UserDialogs;

namespace OwnLink.ViewModel
{
    class TestPageViewModel : INotifyPropertyChanged
    {

        public ICommand SwipeButton { get; set; }
        public TestPageViewModel()
        {
            SwipeButton = new Command(swipeBut);
        }

        public void swipeBut()
        {
            UserDialogs.Instance.Alert("UP");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}

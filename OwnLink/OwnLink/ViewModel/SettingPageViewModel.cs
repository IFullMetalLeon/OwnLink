using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Xamarin.Forms;
using Acr.UserDialogs;
using OwnLink.View;
using Plugin.Settings;
using System.Collections.ObjectModel;
using OwnLink.Model;
using Newtonsoft.Json;
using static OwnLink.Model.JsonClass;
using Plugin.DeviceInfo;
using Android.Telephony;
using Android.Content;
using System.Windows.Input;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;

namespace OwnLink.ViewModel
{
    public class SettingPageViewModel: INotifyPropertyChanged
    {
        public string _uri { get; set; }
        public ICommand openUri { get; set; }
        public ICommand filePick { get; set; }
        public ICommand sendMsg { get; set; }

        public string fileName;
        public string _userEmail { get; set; }
        public string _userMessage { get; set; }

        public SettingPageViewModel()
        {
            _uri = "https://yandex.ru";
            openUri = new Command(showUri);
            filePick = new Command(fileOpen);
            sendMsg = new Command(sendMessage);
        }

        public void startPage()
        {
            MessagingCenter.Subscribe<string, string>("HttpControler", "Feedback", (sender, arg) =>
            {
                feedbackResult(arg.Trim());
            });
            MessagingCenter.Subscribe<string, string>("HttpControler", "Error", (sender, arg) =>
            {
                showError(arg.Trim());
            });
        }


        public void endPage()
        {
            MessagingCenter.Unsubscribe<string, string>("HttpControler", "Feedback");
            MessagingCenter.Unsubscribe<string, string>("HttpControler", "Error");
        }

        public void showUri()
        {
            UserDialogs.Instance.Alert(_uri);
        }

        public async void fileOpen()
        {
            FileData fileData = await CrossFilePicker.Current.PickFile();
            if (fileData == null)
                return; // user canceled file picking

            fileName = fileData.FilePath;
            //string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);
        }

        public void sendMessage()
        {
            string deviceId = CrossDeviceInfo.Current.Id;
            HttpControler.feedbackSend(UserEmail, UserMessage, deviceId, fileName);
        }

        public void feedbackResult(string content)
        {
            UserDialogs.Instance.Alert(content);
        }

        public void showError(string error)
        {
            UserDialogs.Instance.Alert(error);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public string UserEmail
        {
            get
            {
                return _userEmail;
            }
            set
            {
                if (_userEmail != value)
                {
                    _userEmail = value;
                    OnPropertyChanged("UserEmail");
                }
            }
        }

        public string UserMessage
        {
            get
            {
                return _userMessage;
            }
            set
            {
                if (_userMessage != value)
                {
                    _userMessage = value;
                    OnPropertyChanged("UserMessage");
                }
            }
        }
    }
}

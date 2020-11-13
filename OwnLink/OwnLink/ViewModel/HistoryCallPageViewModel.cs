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

namespace OwnLink.ViewModel
{
    public class HistoryCallPageViewModel: INotifyPropertyChanged
    {
        public ObservableCollection<HistoryCallView> historyCalls { get; set; }
        public HistoryCallPageViewModel()
        {
            historyCalls = new ObservableCollection<HistoryCallView>();
        }

        public void startPage()
        {
            MessagingCenter.Subscribe<string, string>("HttpControler", "History", (sender, arg) =>
            {
                historyResult(arg.Trim());
            });
            MessagingCenter.Subscribe<string, string>("HttpControler", "Error", (sender, arg) =>
            {
                showError(arg.Trim());
            });
            string deviceId = CrossDeviceInfo.Current.Id;
            string phone = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");
            HttpControler.history(phone, deviceId);
        }


        public void endPage()
        {
            MessagingCenter.Unsubscribe<string, string>("HttpControler", "History");
            MessagingCenter.Unsubscribe<string, string>("HttpControler", "Error");
        }

        public void historyResult(string content)
        {
            historyCalls.Clear();
            if (content.Length > 2)
            {
                List<HistoryCallView> q = new List<HistoryCallView>();
                historyRootObject tmp = JsonConvert.DeserializeObject<historyRootObject>(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                foreach (historyJsonItem cur in tmp.history)
                {
                    q.Add(new HistoryCallView
                    {
                        Id = cur.id,
                        Duration = cur.billsec,
                        DateShow = cur.start,
                        Caller = cur.src,
                        CallStatus = cur.disposition
                    });
                }
                q.Reverse();
                foreach (HistoryCallView cur in q)
                    historyCalls.Add(cur);
            }
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
    }
}

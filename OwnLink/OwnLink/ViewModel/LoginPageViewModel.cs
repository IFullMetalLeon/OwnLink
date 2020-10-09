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

namespace OwnLink.ViewModel
{
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        ObservableCollection<string> CountryList { get; set; }
        public string _phone { get; set; }
        public string _codeSms { get; set; }
        public bool _isCodeSmsVisible { get; set; }
        public string _iCallText { get; set; }
        public bool _iCallEnb { get; set; }
        public int _countdown;
        public int callFlag;

        public ICallJournal callJournal;
        public INavigation Navigation { get; set; }
        public Command ICallSend { get; set; }
        public Command SendCode { get; set; }

        public LoginPageViewModel()
        {
            ICallSend = new Command(sendCall);
            SendCode = new Command(sendCode);
            ICallText = "Запросить Интернет-звонок";
            ICallEnb = true;
            Phone = "+7";
            IsCodeSmsVisible = false;
            callJournal = DependencyService.Get<ICallJournal>();
            

        }

        public void startPage()
        {
            //Phone = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");            
            
            MessagingCenter.Subscribe<string, string>("HttpControler", "Register", (sender, arg) =>
            {
                register(arg.Trim());
            });
            MessagingCenter.Subscribe<string, string>("broadcast", "incomingNumber", (sender, arg) =>
            {
                phoneNumber(arg.Trim());
            });
            MessagingCenter.Subscribe<string, string>("HttpControler", "Error", (sender, arg) =>
            {
                showError(arg.Trim());
            });

            callFlag = 0;

            CountryList = new ObservableCollection<string>();
            CountryList.Add("+7  Россия");
            CountryList.Add("+380  Украина");
            CountryList.Add("+375  Беларусь");
            string lastPhone = callJournal.GetLastNumber();

        }

        public void endPage()
        {
            MessagingCenter.Unsubscribe<string, string>("HttpControler", "Register");
            MessagingCenter.Unsubscribe<string, string>("HttpControler", "Error");
        }


        public void phoneNumber(string _number)
        {            
            UserDialogs.Instance.Alert(_number);
            CodeSms = _number.Substring(_number.Length - 6, 6);
        }
        public void sendCall()
        {
            HttpControler.register(Phone, "","","");          
        }

        public void checkCode()
        {
            if (CodeSms.Length == 4)
                this.sendCode();
        }

        public void sendCode()
        {
            string deviceId = CrossDeviceInfo.Current.Id;
            string deviceInfo = CrossDeviceInfo.Current.Manufacturer + " " + CrossDeviceInfo.Current.Model + " " + CrossDeviceInfo.Current.Platform + " " + CrossDeviceInfo.Current.Version;
            HttpControler.register(Phone, CodeSms, deviceId, deviceInfo);
        }

        public void register(string content)
        {
            if (content.Length>2)
            {
                regJsonItem tmp = JsonConvert.DeserializeObject<regJsonItem>(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                if (tmp.status == "OK")
                {
                    if (tmp.login == null)
                    {
                        UserDialogs.Instance.Alert(tmp.message);
                        IsCodeSmsVisible = true;
                        Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
                        _countdown = 120;
                        ICallText = "Новый запрос через " + _countdown.ToString();
                        ICallEnb = false;
                    }
                    else
                    {
                        CrossSettings.Current.AddOrUpdateValue("sipPhoneLogin", tmp.login);
                        CrossSettings.Current.AddOrUpdateValue("sipPhonePass", tmp.password);
                        Navigation.PushAsync(new MasterDetailMain());
                    }
                }
                else
                    UserDialogs.Instance.Alert(tmp.message);
            }
        }

        public void showError(string error)
        {
            UserDialogs.Instance.Alert(error);
        }

        private bool OnTimerTick()
        {
            _countdown--;
            if (_countdown > 0)
            {
                ICallText = "Новый запрос через " + _countdown.ToString();

                if (callFlag == 0)
                {
                    string lastPhone = callJournal.GetLastNumber();
                    if (lastPhone == "111")
                        callFlag = 1;
                    else if (lastPhone != "123")
                    {
                        CodeSms = lastPhone.Substring(lastPhone.Length - 4, 4);
                        callFlag = 1;
                    }
                }
                return true;
            }
            else
            {
                ICallText = "Запросить Интернет-звонок";
                ICallEnb = true;
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public string Phone
        {
            get
            {
                return _phone;
            }
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    OnPropertyChanged("Phone");
                }
            }
        }

        public string CodeSms
        {
            get
            {
                return _codeSms;
            }
            set
            {
                if (_codeSms != value)
                {
                    _codeSms = value;
                    OnPropertyChanged("CodeSms");
                }
            }
        }

        public bool IsCodeSmsVisible
        {
            get
            {
                return _isCodeSmsVisible;
            }
            set
            {
                if (_isCodeSmsVisible != value)
                {
                    _isCodeSmsVisible = value;
                    OnPropertyChanged("IsCodeSmsVisible");
                }
            }
        }

        public string ICallText
        {
            get
            {
                return _iCallText;
            }
            set
            {
                if (_iCallText != value)
                {
                    _iCallText = value;
                    OnPropertyChanged("ICallText");
                }
            }
        }

        public bool ICallEnb
        {
            get
            {
                return _iCallEnb;
            }
            set
            {
                if (_iCallEnb != value)
                {
                    _iCallEnb = value;
                    OnPropertyChanged("ICallEnb");
                }
            }
        }
    }
}

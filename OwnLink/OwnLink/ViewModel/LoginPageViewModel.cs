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
        public class Countries
        {
            public string Name { get; set; }
            public string Pref { get; set; }
        }
        public ObservableCollection<Countries> CountryList { get; set; }
        public ObservableCollection<Countries> CounrtiesList { get; set; }
        public Countries CounrtySelected { get; set; }
        public string _phone { get; set; }
        public string _codeSms { get; set; }
        public string _countryName { get; set; }
        public string _countrySelectArrow { get; set; }
        public string _counrtySearchText { get; set; }
        public bool _isCodeSmsVisible { get; set; }
        public bool _isNumberVisible { get; set; }
        public bool _counrtyListVisible { get; set; }
        public string _iCallText { get; set; }
        public bool _iCallEnb { get; set; }
        public int _countdown;
        public int callFlag;
        public string cPref;

        public ICallJournal callJournal;
        public INavigation Navigation { get; set; }
        public Command ICallSend { get; set; }
        public Command SendCode { get; set; }
        public Command CountryTap { get; set; }

        public LoginPageViewModel()
        {
            ICallSend = new Command(sendCall);
            SendCode = new Command(sendCode);
            CountryTap = new Command(CountryListShow);
            ICallText = "Запросить Интернет-звонок";
            ICallEnb = true;
            Phone = "";
            cPref = "";
            IsCodeSmsVisible = false;
            callJournal = DependencyService.Get<ICallJournal>();

            CountryList = new ObservableCollection<Countries>();
            CounrtiesList = new ObservableCollection<Countries>();
            CounrtySelected = new Countries();

            CountryList.Add(new Countries { Name = "Россия", Pref = "+7" });
            CountryList.Add(new Countries { Name = "Белоруссия", Pref = "+375" });
            CountryList.Add(new Countries { Name = "Украина", Pref = "+380" });
            CountryList.Add(new Countries { Name = "Армения", Pref = "+374" });
            CountryList.Add(new Countries { Name = "Афганистан", Pref = "+93" });
            CountryList.Add(new Countries { Name = "Болгария", Pref = "+359" });
            CountryList.Add(new Countries { Name = "Хорватия", Pref = "+385" });
            CountryList.Add(new Countries { Name = "Грузия", Pref = "+995" });
            CountryList.Add(new Countries { Name = "Казахстан", Pref = "+7" });
            CountryList.Add(new Countries { Name = "Кыргызстан", Pref = "+996" });
            CountryList.Add(new Countries { Name = "Таджикистан", Pref = "+992" });
            CountryList.Add(new Countries { Name = "Узбекистан", Pref = "+998" });
            CountryList.Add(new Countries { Name = "Югославия", Pref = "+381" });

        }

        public void startPage()
        {
            Phone = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");            
            
            MessagingCenter.Subscribe<string, string>("HttpControler", "Register", (sender, arg) =>
            {
                register(arg.Trim());
            });
            MessagingCenter.Subscribe<string, string>("HttpControler", "Error", (sender, arg) =>
            {
                showError(arg.Trim());
            });

            callFlag = 0;

            CountryName = CrossSettings.Current.GetValueOrDefault("sipPhoneCountry", "");
            cPref = CrossSettings.Current.GetValueOrDefault("sipPhoneCountryPref", "");

            if (Phone == "")
                Phone = cPref;

            CounrtyListVisible = false;
            CountrySelectArrow = "&#xE70D;";

            if (CountryName == "")
            {
                IsNumberVisible = false;
                CountryName = "Выбор страны";
            }
            else
                IsNumberVisible = true;
            CounrtySearchText = "";
            SearchCountry();
        }

        public void endPage()
        {
            MessagingCenter.Unsubscribe<string, string>("HttpControler", "Register");
            MessagingCenter.Unsubscribe<string, string>("HttpControler", "Error");
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

        public void SearchCountry()
        {
            CounrtiesList.Clear();
            foreach (Countries cur in CountryList)
            {
                if (cur.Name.ToLower().Contains(CounrtySearchText.ToLower()))
                    CounrtiesList.Add(cur);
            }
        }

        public void CountryListShow()
        {
            if (!CounrtyListVisible)
            {
                CounrtyListVisible = true;
                CountrySelectArrow = "&#xE70E;";
            }
            else
            {
                CounrtyListVisible = false;
                CountrySelectArrow = "&#xE70D;";
            }
        }
        public void SelectCountry()
        {
            if (CounrtySelected != null)
            {
                CountryName = CounrtySelected.Name;
                CrossSettings.Current.AddOrUpdateValue("sipPhoneCountry", CountryName);
                CrossSettings.Current.AddOrUpdateValue("sipPhoneCountryPref", CounrtySelected.Pref);
                cPref = CounrtySelected.Pref;
                Phone = cPref;
                IsNumberVisible = true;
                CounrtyListVisible = false;
            }
        }

        public void checkPhone()
        {
            if (cPref == null)
                cPref = "";
            if (cPref.Length > 0)
            {
                if (Phone.Length < cPref.Length)
                {
                    Phone = cPref;
                }
                else
                {
                    string curPref = Phone.Substring(0, cPref.Length);
                    if (curPref != cPref)
                    {
                        Phone = cPref + Phone.Substring(cPref.Length);
                    }
                }
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

        public string CountryName
        {
            get
            {
                return _countryName;
            }
            set
            {
                if (_countryName != value)
                {
                    _countryName = value;
                    OnPropertyChanged("CountryName");
                }
            }
        }

        public string CountrySelectArrow
        {
            get
            {
                return _countrySelectArrow;
            }
            set
            {
                if (_countrySelectArrow != value)
                {
                    _countrySelectArrow = value;
                    OnPropertyChanged("CountrySelectArrow");
                }
            }
        }

        public string CounrtySearchText
        {
            get
            {
                return _counrtySearchText;
            }
            set
            {
                if (_counrtySearchText != value)
                {
                    _counrtySearchText = value;
                    OnPropertyChanged("CounrtySearchText");
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

        public bool IsNumberVisible
        {
            get
            {
                return _isNumberVisible;
            }
            set
            {
                if (_isNumberVisible != value)
                {
                    _isNumberVisible = value;
                    OnPropertyChanged("IsNumberVisible");
                }
            }
        }

        public bool CounrtyListVisible
        {
            get
            {
                return _counrtyListVisible;
            }
            set
            {
                if (_counrtyListVisible != value)
                {
                    _counrtyListVisible = value;
                    OnPropertyChanged("CounrtyListVisible");
                }
            }
        }
    }
}

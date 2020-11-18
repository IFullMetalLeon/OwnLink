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
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        public class Countries
        {
            public string Name { get; set; }
            public string Pref { get; set; }
            public string AlterName { get; set; }
            
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
        public ICommand ICallSend { get; set; }
        public ICommand SendCode { get; set; }
        public ICommand CountryTap { get; set; }

        public LoginPageViewModel()
        {
            ICallSend = new Command(sendCall);
            SendCode = new Command(sendCode);
            CountryTap = new Command(CountryListShow);
            ICallText = "Звонок-подтверждение";
            ICallEnb = true;
            Phone = "";
            cPref = "";
            IsCodeSmsVisible = false;
            callJournal = DependencyService.Get<ICallJournal>();

            CountryList = new ObservableCollection<Countries>();
            CounrtiesList = new ObservableCollection<Countries>();
            CounrtySelected = new Countries();

            

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

            InitCountry();

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
                if (cur.Name.ToLower().Contains(CounrtySearchText.ToLower()) || cur.AlterName.ToLower().Contains(CounrtySearchText.ToLower()))
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

        public void InitCountry()
        {
            CountryList.Add(new Countries { Name = "Афганистан", AlterName = "Afghanistan", Pref = "+93" });
            CountryList.Add(new Countries { Name = "Albania", AlterName = "Албания", Pref = "+355" });   
            CountryList.Add(new Countries { Name = "Algeria", AlterName = "Алжир", Pref = "+21" });     
            CountryList.Add(new Countries { Name = "American Samoa", AlterName = "Американское Самоа", Pref = "+684" });     
            CountryList.Add(new Countries { Name = "Andorra", AlterName = "Андорра", Pref = "+376" });
            CountryList.Add(new Countries { Name = "Angola", AlterName = "Ангола", Pref = "+244" });    
            CountryList.Add(new Countries { Name = "Anguilla", AlterName = "Anguilla", Pref = "+1-264" });    
            CountryList.Add(new Countries { Name = "Antigua and Barbuda", AlterName = "Антигуа и Барбуда", Pref = " +1-268" });    
            CountryList.Add(new Countries { Name = "Армения", AlterName = "Armenia", Pref = " +374" });  
            CountryList.Add(new Countries { Name = "Argentina", AlterName = "Аргентина", Pref = "+54" });     
            CountryList.Add(new Countries { Name = "Australia", AlterName = "Австралия", Pref = "+61" });     
            CountryList.Add(new Countries { Name = "Austria", AlterName = "Австрия", Pref = "+43" });   
            CountryList.Add(new Countries { Name = "Азербайджан", AlterName = "Azerbaijan", Pref = "+994" });   
            CountryList.Add(new Countries { Name = "Bahamas", AlterName = "Багамы", Pref = "+1-242" });    
            CountryList.Add(new Countries { Name = "Bahrain", AlterName = "Бахрейн", Pref = "+973" });   
            CountryList.Add(new Countries { Name = "Bangladesh", AlterName = "Бангладеш", Pref = "+880" });     
            CountryList.Add(new Countries { Name = "Barbados", AlterName = "Барбадос", Pref = "+1-246" });      
            CountryList.Add(new Countries { Name = "Белоруссия", AlterName = "Belarus", Pref = "+375" });    
            CountryList.Add(new Countries { Name = "Belgium", AlterName = "Бельгия", Pref = "+32" });   
            CountryList.Add(new Countries { Name = "Belize", AlterName = "Белиз", Pref = "+501" });     
            CountryList.Add(new Countries { Name = "Benin", AlterName = "Бенин", Pref = "+229" });     
            CountryList.Add(new Countries { Name = "Bermuda", AlterName = "Бермудские острова", Pref = "+1-441" });   
            CountryList.Add(new Countries { Name = "Bolivia", AlterName = "Боливия", Pref = "+591" });   
            CountryList.Add(new Countries { Name = "Bosnia and Herzegovina", AlterName = "Босния и Герцеговина", Pref = "+387" });     
            CountryList.Add(new Countries { Name = "Botswana", AlterName = "Ботсвана", Pref = "+267" });      
            CountryList.Add(new Countries { Name = "Brazil", AlterName = "Бразилия", Pref = "+55" });      
            CountryList.Add(new Countries { Name = "British Virgin Islands", AlterName = "British Virgin Islands", Pref = "+1-284" });    
            CountryList.Add(new Countries { Name = "Brunei Darusalaam", AlterName = "Бруней Даруэсалаам", Pref = "+673" });    
            CountryList.Add(new Countries { Name = "Болгария", AlterName = "Bulgaria", Pref = "+359" });      
            CountryList.Add(new Countries { Name = "Burkina Faso", AlterName = "Буркина Фасо", Pref = "+226" });       
            CountryList.Add(new Countries { Name = "Burundi", AlterName = "Бурунди", Pref = "+257" });   
            CountryList.Add(new Countries { Name = "Cambodia", AlterName = "Камбоджа", Pref = "+855" });      
            CountryList.Add(new Countries { Name = "Cameroon", AlterName = "Камерун", Pref = "+237" });   
            CountryList.Add(new Countries { Name = "Canada", AlterName = "Канада", Pref = "+1" });   
            CountryList.Add(new Countries { Name = "Cape Verde", AlterName = "Капе верде", Pref = "+238" });       
            CountryList.Add(new Countries { Name = "Cayman Islands", AlterName = "Каймановы острова", Pref = "+1-345" });        
            CountryList.Add(new Countries { Name = "Central African Republic", AlterName = " Центрально африканская республика", Pref = "+236" });    
            CountryList.Add(new Countries { Name = "Chad", AlterName = "Чад", Pref = "+235" });   
            CountryList.Add(new Countries { Name = "Chile", AlterName = "Чили", Pref = "+56" });      
            CountryList.Add(new Countries { Name = "Christmas Island", AlterName = "Рождественсткие острова", Pref = "+672" });    
            CountryList.Add(new Countries { Name = "Cocos Islands", AlterName = "Кокосовые острова", Pref = "+672" });     
            CountryList.Add(new Countries { Name = "Colombia", AlterName = "Колумбия", Pref = "+57" });      
            CountryList.Add(new Countries { Name = "Commonwealth of the Northern Mariana Islands", AlterName = "Содружество северных Марианских островов", Pref = "+1-670" });       
            CountryList.Add(new Countries { Name = "Comoros and Mayotte Island", AlterName = "Коморские и майотские(?) острова", Pref = "+269" });      
            CountryList.Add(new Countries { Name = "Congo", AlterName = "Конго", Pref = "+242" });     
            CountryList.Add(new Countries { Name = "Cook Islands", AlterName = "Острова Кука", Pref = "+682" });       
            CountryList.Add(new Countries { Name = "Costa Rica", AlterName = "Коста - Рика", Pref = "+506" });      
            CountryList.Add(new Countries { Name = "Croatia", AlterName = "Хорватия", Pref = "+385" });      
            CountryList.Add(new Countries { Name = "Cuba", AlterName = "Куба", Pref = "+53" });      
            CountryList.Add(new Countries { Name = "Cyprus", AlterName = "Кипр", Pref = "+357" });      
            CountryList.Add(new Countries { Name = "Czech Republic", AlterName = "Чешская республика", Pref = "+420" });       
            CountryList.Add(new Countries { Name = "Denmark", AlterName = "Дания", Pref = "+45" });     
            CountryList.Add(new Countries { Name = "Diego Garcia", AlterName = "Diego Garcia", Pref = "+246" });   
            CountryList.Add(new Countries { Name = "Djibouti", AlterName = "Джибути", Pref = "+253" });   
            CountryList.Add(new Countries { Name = "Dominica", AlterName = "Доминика", Pref = "+1-767" });      
            CountryList.Add(new Countries { Name = "Dominican Republic", AlterName = "Доминиканская республика", Pref = "+1-809" });         
            CountryList.Add(new Countries { Name = "East Timor", AlterName = "Восточный Тимор", Pref = "+62" });      
            CountryList.Add(new Countries { Name = "Ecuador", AlterName = "Эквадор", Pref = "+593" });   
            CountryList.Add(new Countries { Name = "Egypt", AlterName = "Египет", Pref = "+20" });    
            CountryList.Add(new Countries { Name = "El Salvador", AlterName = "Сальвадор", Pref = "+503" });     
            CountryList.Add(new Countries { Name = "Equatorial Guinea", AlterName = "Экваториальная Гвинея", Pref = "+240" });     
            CountryList.Add(new Countries { Name = "Estonia", AlterName = "Эстония", Pref = "+372" });   
            CountryList.Add(new Countries { Name = "Ethiopia", AlterName = "Эфиопия", Pref = "+251" });
            CountryList.Add(new Countries { Name = "Faeroe Islands", AlterName = "Faeroe Islands", Pref = "+298" });    
            CountryList.Add(new Countries { Name = "Falkland Islands", AlterName = "Фолклендские острова", Pref = "+500" });      
            CountryList.Add(new Countries { Name = "Fiji", AlterName = "Фиджи", Pref = "+679" });     
            CountryList.Add(new Countries { Name = "Finland", AlterName = "Финляндия", Pref = "+358" });     
            CountryList.Add(new Countries { Name = "France", AlterName = "Франция", Pref = "+33" });   
            CountryList.Add(new Countries { Name = "French Antilles", AlterName = "Французские Антиллы", Pref = "+590" });    
            CountryList.Add(new Countries { Name = "French Guiana", AlterName = "Французская Гвиана", Pref = "+594" });       
            CountryList.Add(new Countries { Name = "French Polynesia", AlterName = "Франзузская полинезия", Pref = "+689" });     
            CountryList.Add(new Countries { Name = "Gabon", AlterName = "Габон", Pref = "+241" });     
            CountryList.Add(new Countries { Name = "Gambia", AlterName = "Гамбия", Pref = "+220" });    
            CountryList.Add(new Countries { Name = "Грузия", AlterName = "Georgia", Pref = "+995" });    
            CountryList.Add(new Countries { Name = "Germany", AlterName = "Германия", Pref = "+49" });      
            CountryList.Add(new Countries { Name = "Ghana", AlterName = "Гана", Pref = "+233" });      
            CountryList.Add(new Countries { Name = "Gibraltar", AlterName = "Гибралтар", Pref = "+350" });     
            CountryList.Add(new Countries { Name = "Greece", AlterName = "Греция", Pref = "+30" });    
            CountryList.Add(new Countries { Name = "Greenland", AlterName = "Гренландия", Pref = "+299" });    
            CountryList.Add(new Countries { Name = "Grenada", AlterName = "Гренада", Pref = "+1-473" });   
            CountryList.Add(new Countries { Name = "Guam", AlterName = "Гуам", Pref = "+671" });     
            CountryList.Add(new Countries { Name = "Guatemala", AlterName = "Гватемала", Pref = "+502" });     
            CountryList.Add(new Countries { Name = "Guinea", AlterName = "Гвинея", Pref = "+224" });    
            CountryList.Add(new Countries { Name = "Guinea - Bissau", AlterName = "Гвинея Биссау", Pref = "+245" });        
            CountryList.Add(new Countries { Name = "Guyana", AlterName = "Гайана", Pref = "+592" });    
            CountryList.Add(new Countries { Name = "Haiti", AlterName = "Гаити", Pref = "+509" });     
            CountryList.Add(new Countries { Name = "Honduras", AlterName = "Гондурас", Pref = "+504" });      
            CountryList.Add(new Countries { Name = "Hong Kong", AlterName = "Гонгконг", Pref = "+852" });      
            CountryList.Add(new Countries { Name = "Hungary", AlterName = "Венгрия", Pref = "+36" });   
            CountryList.Add(new Countries { Name = "Iceland", AlterName = "Исландия", Pref = "+354" });      
            CountryList.Add(new Countries { Name = "India", AlterName = "Индия", Pref = "+91" });     
            CountryList.Add(new Countries { Name = "Indonesia", AlterName = "Индонезия", Pref = "+62" });     
            CountryList.Add(new Countries { Name = "Iran", AlterName = "Иран", Pref = "+98" });      
            CountryList.Add(new Countries { Name = "Iraq", AlterName = "Ирак", Pref = "+964" });      
            CountryList.Add(new Countries { Name = "Irish Republic", AlterName = "Ирландская республика", Pref = "+353" });       
            CountryList.Add(new Countries { Name = "Israel", AlterName = "Израиль", Pref = "+972" });   
            CountryList.Add(new Countries { Name = "Italy", AlterName = "Италия", Pref = "+39" });    
            CountryList.Add(new Countries { Name = "Ivory Coast", AlterName = "Берег слоновой кости", Pref = "+225" });    
            CountryList.Add(new Countries { Name = "Jamaica", AlterName = "Ямайка", Pref = "+1-876" });      
            CountryList.Add(new Countries { Name = "Japan", AlterName = "Япония", Pref = "+81" });    
            CountryList.Add(new Countries { Name = "Jordan", AlterName = "Иордания", Pref = "+962" });      
            CountryList.Add(new Countries { Name = "Казахстан", AlterName = "Kazakhstan", Pref = "+7" });     
            CountryList.Add(new Countries { Name = "Kenya", AlterName = "Кения", Pref = "+254" });     
            CountryList.Add(new Countries { Name = "Kiribati Republic", AlterName = "Кирибати", Pref = "+686" });      
            CountryList.Add(new Countries { Name = "Кыргызстан", AlterName = "Kirghizia", Pref = "+996" });    
            CountryList.Add(new Countries { Name = "Kuwait", AlterName = "Кувейт", Pref = "+965" });    
            CountryList.Add(new Countries { Name = "Laos", AlterName = "Лаос", Pref = "+856" });      
            CountryList.Add(new Countries { Name = "Latvia", AlterName = "Латвия", Pref = "+371" });    
            CountryList.Add(new Countries { Name = "Lebanon", AlterName = "Ливан", Pref = "+961" });     
            CountryList.Add(new Countries { Name = "Lesotho", AlterName = "Лессото", Pref = "+266" });   
            CountryList.Add(new Countries { Name = "Liberia", AlterName = "Либерия", Pref = "+231" });   
            CountryList.Add(new Countries { Name = "Libya", AlterName = "Ливия", Pref = "+21" });     
            CountryList.Add(new Countries { Name = "Liechtenstein", AlterName = "Лихтенштейн", Pref = "+41" });   
            CountryList.Add(new Countries { Name = "Lithuania", AlterName = "Литва", Pref = "+370" });     
            CountryList.Add(new Countries { Name = "Luxembourg", AlterName = "Люксембург", Pref = "+352" });    
            CountryList.Add(new Countries { Name = "Macao", AlterName = "Макао", Pref = "+853" });     
            CountryList.Add(new Countries { Name = "Macedonia", AlterName = "Македония", Pref = "+389" });     
            CountryList.Add(new Countries { Name = "Madagascar", AlterName = "Мадагаскар", Pref = "+261" });    
            CountryList.Add(new Countries { Name = "Malawi", AlterName = "Малави", Pref = "+265" });    
            CountryList.Add(new Countries { Name = "Malaysia", AlterName = "Малайзия", Pref = "+60" });      
            CountryList.Add(new Countries { Name = "Maldives", AlterName = "Мальдивы", Pref = "+960" });      
            CountryList.Add(new Countries { Name = "Mali", AlterName = "Мали", Pref = "+223" });      
            CountryList.Add(new Countries { Name = "Malta", AlterName = "Мальта", Pref = "+356" });    
            CountryList.Add(new Countries { Name = "Marshall Islands", AlterName = "Маршалловы острова", Pref = "+692" });    
            CountryList.Add(new Countries { Name = "Martinique", AlterName = "Мартиника", Pref = "+596" });     
            CountryList.Add(new Countries { Name = "Mauritania", AlterName = "Мавритания", Pref = "+222" });    
            CountryList.Add(new Countries { Name = "Mauritius", AlterName = "Маврикий", Pref = "+230" });      
            CountryList.Add(new Countries { Name = "Mexico", AlterName = "Мексика", Pref = "+52" });
            CountryList.Add(new Countries { Name = "Micronesia", AlterName = "Микронезия", Pref = "+691" });    
            CountryList.Add(new Countries { Name = "Monaco", AlterName = "Монако", Pref = "+377" });    
            CountryList.Add(new Countries { Name = "Монголия", AlterName = "Mongolia", Pref = "+976" });      
            CountryList.Add(new Countries { Name = "Montserrat", AlterName = "Montserrat", Pref = "+1-664" });  
            CountryList.Add(new Countries { Name = "Morocco", AlterName = "Мороко", Pref = "+212" });    
            CountryList.Add(new Countries { Name = "Mozambique", AlterName = "Мозамбик", Pref = "+258" });      
            CountryList.Add(new Countries { Name = "Myanmar", AlterName = "Myanmar", Pref = "+95" });  
            CountryList.Add(new Countries { Name = "Namibia", AlterName = "Намибия", Pref = "+264" });   
            CountryList.Add(new Countries { Name = "Nauru", AlterName = "Науру", Pref = "+674" });     
            CountryList.Add(new Countries { Name = "Nepal", AlterName = "Непал", Pref = "+977" });     
            CountryList.Add(new Countries { Name = "Netherlands", AlterName = "Нидерланды", Pref = "+31" });    
            CountryList.Add(new Countries { Name = "Netherlands Antilles", AlterName = "Нидерландские антиллы", Pref = "+599" });     
            CountryList.Add(new Countries { Name = "New Caledonia", AlterName = "Новая каледония", Pref = "+687" });      
            CountryList.Add(new Countries { Name = "New Zealand", AlterName = "Новая зеландия", Pref = "+64" });     
            CountryList.Add(new Countries { Name = "Nicaragua", AlterName = "Никарагуа", Pref = "+505" });     
            CountryList.Add(new Countries { Name = "Niger", AlterName = "Нигер", Pref = "+227" });     
            CountryList.Add(new Countries { Name = "Nigeria", AlterName = "Нигерия", Pref = "+234" });   
            CountryList.Add(new Countries { Name = "Niue", AlterName = "Niue", Pref = "+683" });  
            CountryList.Add(new Countries { Name = "Norfolk Island", AlterName = "Норфолкские острова", Pref = "+672" });    
            CountryList.Add(new Countries { Name = "North Korea", AlterName = "Северная Корея", Pref = "+850" });     
            CountryList.Add(new Countries { Name = "North Yemen", AlterName = "Северный Йемен", Pref = "+967" });     
            CountryList.Add(new Countries { Name = "Northern Mariana Islands", AlterName = "Северно Марианские острова", Pref = "+670" });    
            CountryList.Add(new Countries { Name = "Norway", AlterName = "Норвегия", Pref = "+47" });      
            CountryList.Add(new Countries { Name = "Oman", AlterName = "Оман", Pref = "+968" });      
            CountryList.Add(new Countries { Name = "Pakistan", AlterName = "Пакистан", Pref = "+92" });      
            CountryList.Add(new Countries { Name = "Panama", AlterName = "Панама", Pref = "+507" });    
            CountryList.Add(new Countries { Name = "Papua New Guinea", AlterName = "Папуа Новая Гвинея", Pref = "+675" });    
            CountryList.Add(new Countries { Name = "Paraguay", AlterName = "Парагвай", Pref = "+595" });      
            CountryList.Add(new Countries { Name = "Peru", AlterName = "Перу", Pref = "+51" });      
            CountryList.Add(new Countries { Name = "Philippines", AlterName = "Филипины", Pref = "+63" });      
            CountryList.Add(new Countries { Name = "Poland", AlterName = "Польша", Pref = "+48" });    
            CountryList.Add(new Countries { Name = "Portugal", AlterName = "Португалия", Pref = "+351" });    
            CountryList.Add(new Countries { Name = "Puerto Rico", AlterName = "Пуэрто Рико", Pref = "+1-787" });    
            CountryList.Add(new Countries { Name = "Qatar", AlterName = "Катар", Pref = "+974" });     
            CountryList.Add(new Countries { Name = "Republic of San Marino", AlterName = "Республика Сан Марино", Pref = "+ 378" });     
            CountryList.Add(new Countries { Name = "Reunion", AlterName = "Реоньон", Pref = "+262" });   
            CountryList.Add(new Countries { Name = "Romania", AlterName = "Румыния", Pref = "+40" });   
            CountryList.Add(new Countries { Name = "Россия", AlterName = "Russia", Pref = "+7" });    
            CountryList.Add(new Countries { Name = "Rwandese Republic", AlterName = "Республика Руанда", Pref = "+250" });        
            CountryList.Add(new Countries { Name = "Saint Helena and Ascension Island", AlterName = "Saint Helena and Ascension Island", Pref = "+247" });  
            CountryList.Add(new Countries { Name = "Saint Pierre et Miquelon", AlterName = "Saint Pierre et Miquelon", Pref = "+508" });      
            CountryList.Add(new Countries { Name = "San Marino", AlterName = "Сан Марино", Pref = "+39" });      
            CountryList.Add(new Countries { Name = "Sao Tome e Principe", AlterName = "Sao Tome e Principe", Pref = "+239" });   
            CountryList.Add(new Countries { Name = "Saudi Arabia", AlterName = "Саудовская Аравия", Pref = "+966" });     
            CountryList.Add(new Countries { Name = "Senegal", AlterName = "Сенегал", Pref = "+221" });   
            CountryList.Add(new Countries { Name = "Seychelles", AlterName = "Seychelles", Pref = "+248" });  
            CountryList.Add(new Countries { Name = "Sierra Leone", AlterName = "Сьерра Леоне", Pref = "+232" });      
            CountryList.Add(new Countries { Name = "Singapore", AlterName = "Сингапур", Pref = "+65" });      
            CountryList.Add(new Countries { Name = "Slovakia", AlterName = "Словакия", Pref = "+421" });      
            CountryList.Add(new Countries { Name = "Словения", AlterName = "Slovenia", Pref = "+386" });      
            CountryList.Add(new Countries { Name = "Solomon Islands", AlterName = "Соломоновы острова", Pref = "+677" });     
            CountryList.Add(new Countries { Name = "Somalia", AlterName = "Сомали", Pref = "+252" });    
            CountryList.Add(new Countries { Name = "South Africa", AlterName = "ЮАР", Pref = "+27" });   
            CountryList.Add(new Countries { Name = "South Korea", AlterName = "Южная Корея", Pref = "+82" });    
            CountryList.Add(new Countries { Name = "South Yemen", AlterName = "Южный Йемен", Pref = "+969" });    
            CountryList.Add(new Countries { Name = "Spain", AlterName = "Испания", Pref = "+34" });   
            CountryList.Add(new Countries { Name = "Sri Lanka", AlterName = "Шри Ланка", Pref = "+94" });        
            CountryList.Add(new Countries { Name = "St.Kitts and Nevis", AlterName = "St.Kitts and Nevis", Pref = "+1-869" });  
            CountryList.Add(new Countries { Name = "St.Lucia", AlterName = "St.Lucia", Pref = "+1-758" });  
            CountryList.Add(new Countries { Name = "St.Vincent and the Grenadines", AlterName = "St.Vincent and the Grenadines", Pref = "+1-784" });         
            CountryList.Add(new Countries { Name = "Sudan", AlterName = "Судан", Pref = "+249" });     
            CountryList.Add(new Countries { Name = "Suriname", AlterName = "Суринам", Pref = "+597" });   
            CountryList.Add(new Countries { Name = "Svalbard and Jan Mayen Islands", AlterName = "Svalbard and Jan Mayen Islands", Pref = "+47" });  
            CountryList.Add(new Countries { Name = "Swaziland", AlterName = "Свазиленд", Pref = "+268" });     
            CountryList.Add(new Countries { Name = "Sweden", AlterName = "Швеция", Pref = "+46" });    
            CountryList.Add(new Countries { Name = "Switzerland", AlterName = "Швейцария", Pref = "+41" });     
            CountryList.Add(new Countries { Name = "Syria", AlterName = "Сирия", Pref = "+963" });
            CountryList.Add(new Countries { Name = "Таджикистан", AlterName = "Tadjikistan", Pref = "+992" }); 
            CountryList.Add(new Countries { Name = "Taiwan", AlterName = "Тайвань", Pref = "+886" });   
            CountryList.Add(new Countries { Name = "Tanzania", AlterName = "Танзания", Pref = "+255" });      
            CountryList.Add(new Countries { Name = "Thailand", AlterName = "Тайланд", Pref = "+66" });
            CountryList.Add(new Countries { Name = "Togolese Republic", AlterName = "Республика Тоголезе", Pref = "+228" });   
            CountryList.Add(new Countries { Name = "Tokelau", AlterName = "Tokelau", Pref = "+690" });  
            CountryList.Add(new Countries { Name = "Tonga", AlterName = "Тонго", Pref = "+676" });     
            CountryList.Add(new Countries { Name = "Trinidad and Tobago", AlterName = "Тринидад и Тобаго", Pref = "+1-868" });     
            CountryList.Add(new Countries { Name = "Tunisia", AlterName = "Тунис", Pref = "+21" });     
            CountryList.Add(new Countries { Name = "Turkey", AlterName = "Турция", Pref = "+90" });    
            CountryList.Add(new Countries { Name = "Туркменистан", AlterName = "Turkmenistan", Pref = "+993" });      
            CountryList.Add(new Countries { Name = "Turks & Caicos Islands", AlterName = "Turks & Caicos Islands", Pref = "+1-649" });  
            CountryList.Add(new Countries { Name = "Tuvalu", AlterName = "Tuvalu", Pref = "+688" });  
            CountryList.Add(new Countries { Name = "US Virgin Islands", AlterName = "US Virgin Islands", Pref = "+1-340" });
            CountryList.Add(new Countries { Name = "Uganda", AlterName = "Уганда", Pref = "+256" });    
            CountryList.Add(new Countries { Name = "Украина", AlterName = "Ukraine", Pref = "+380" });   
            CountryList.Add(new Countries { Name = "United Arab Emirates", AlterName = " О.А.Э.", Pref = "+971" });   
            CountryList.Add(new Countries { Name = "United Kingdom", AlterName = "Великобритания", Pref = "+44" });     
            CountryList.Add(new Countries { Name = "Uruguay", AlterName = "Уругвай", Pref = "+598" });   
            CountryList.Add(new Countries { Name = "USA", AlterName = "США", Pref = "+1" });   
            CountryList.Add(new Countries { Name = "Узбекистан", AlterName = "Uzbekistan", Pref = "+998" });    
            CountryList.Add(new Countries { Name = "Vanuatu", AlterName = "Vanuatu", Pref = "+678" });  
            CountryList.Add(new Countries { Name = "Vatican City", AlterName = "Ватикан", Pref = "+39" });   
            CountryList.Add(new Countries { Name = "Venezuela", AlterName = "Венесуэла", Pref = "+58" });     
            CountryList.Add(new Countries { Name = "Vietnam", AlterName = "Вьетнам", Pref = "+84" });   
            CountryList.Add(new Countries { Name = "Wallis and Futuna Islands", AlterName = "Wallis and Futuna Islands", Pref = "+681" });     
            CountryList.Add(new Countries { Name = "Western Sahara", AlterName = "Западная Сахара", Pref = "+21" });     
            CountryList.Add(new Countries { Name = "Western Samoa", AlterName = "Западное Самоа", Pref = "+685" });       
            CountryList.Add(new Countries { Name = "Yugoslavia", AlterName = "Югославия", Pref = "+381" });     
            CountryList.Add(new Countries { Name = "Zaire", AlterName = "Заир", Pref = "+243" });      
            CountryList.Add(new Countries { Name = "Zambia", AlterName = "Замбия", Pref = "+260" });    
            CountryList.Add(new Countries { Name = "Zimbabwe", AlterName = "Зимбабве", Pref = "+263" });      
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

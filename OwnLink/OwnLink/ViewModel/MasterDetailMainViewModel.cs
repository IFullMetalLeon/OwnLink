using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Acr.UserDialogs;
using Plugin.Settings;
using OwnLink.Model;
using Xamarin.Forms;
using OwnLink.View;
using System.Net;
using OwnLink.ViewModel;
using System.Threading.Tasks;
using Linphone;
using Plugin.DeviceInfo;
using Xamarin.Essentials;

namespace OwnLink.ViewModel
{
    public class MasterDetailMainViewModel : INotifyPropertyChanged
    {

        public Core Core
        {
            get
            {
                return ((App)App.Current).Core;
            }
        }

        private int FlagSleep
        {
            get
            {
                return ((App)App.Current).flagSleep;
            }
        }

        public INotificationManager notificationManager;
        public IFCMService fCMService;
        public IOpenSettings openSettings;

        public string _phone { get; set; }
        public string _pass { get; set; }
        public string _regStatus { get; set; }
        public string _versionNumber { get; set; }
        public string _regStatusIcon { get; set; }
        public INavigation Navigation { get; set; }
        public Command ChangePhone { get; set; }
        public Command Reconnect { get; set; }

        public int isNotifySend;


        public MasterDetailMainViewModel()
        {
            ChangePhone = new Command(changePhone);
            Reconnect = new Command(reconnect);

            notificationManager = DependencyService.Get<INotificationManager>();
            fCMService = DependencyService.Get<IFCMService>();
            openSettings = DependencyService.Get<IOpenSettings>();

            Core.Listener.OnRegistrationStateChanged += OnRegistration;
            Core.Listener.OnCallStateChanged += OnCall;
            Core.Listener.OnLogCollectionUploadStateChanged += OnLogCollectionUpload;

            isNotifySend = 0;
        }

        public void startPage()
        {
            MessagingCenter.Subscribe<string, string>("HttpControler", "GetServerVersion", (sender, arg) => {
                checkVersion(arg.Trim());
            });
            HttpControler.GetServerVersion();
            string s = CrossDeviceInfo.Current.DeviceName;
            string deviceId = CrossDeviceInfo.Current.Id;
            s = CrossDeviceInfo.Current.Model;
            s = CrossDeviceInfo.Current.Platform.ToString();

            Phone = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");
            _pass = CrossSettings.Current.GetValueOrDefault("sipPhonePass", "");

            VersionNumber = CrossDeviceInfo.Current.AppVersion;

            string curFCMToken = fCMService.GetToken();

            HttpControler.FCMTokenSend(Phone, curFCMToken, deviceId);

            //Phone = "leon2";
            //_pass = "BYMyt3rL8T9wfBdY";


            RegStatus = "Оффлайн";
            RegStatusIcon = "StatusOffline.png";
            if (Core.DefaultProxyConfig?.State == RegistrationState.Ok)
            {
                Core.EnsureRegistered();
                RegStatus = "Онлайн";
                RegStatusIcon = "StatusOnline.png";
            }
            else
                reconnect();


            //Core.NatPolicy.StunServer = "stun.l.google.com:19302";
            //Core.NatPolicy.StunEnabled = true;
            //Core.NatPolicy.TlsTurnTransportEnabled = true;
            //Core.NatPolicy.TurnEnabled = true;



            IEnumerable<PayloadType> q = Core.AudioPayloadTypes;
            List<PayloadType> cur = new List<PayloadType>();           
            int a = 0;
            foreach(PayloadType c in q)
            {
                a++;
                if (c.MimeType == "PCMU" || c.MimeType == "PCMA" || c.MimeType == "G729")
                {

                    if (c.MimeType == "G729")
                        c.RecvFmtp = "annexb=no";
                    cur.Add(c);
                }
                else
                {
                    c.Number = -1;
                    cur.Add(c);
                }
            }

            Core.AudioPayloadTypes = cur;

             q = Core.AudioPayloadTypes;

             foreach (PayloadType c in q)
             {
                 a++;
             }

            Core.Ring = null;
            Core.Ringback = null;
            Core.RingerDevice = null;
            Core.RemoteRingbackTone = null;
            Core.RingDuringIncomingEarlyMedia = false;
            //Core.PushNotificationEnabled = false;

            isNotifySend = 0;            

            if (Core.CallsNb > 0)
            {
                isNotifySend = 1;
                MessagingCenter.Send<string, string>("Call", "CallState", "Incoming");
            }

            string isNotificationPermission = CrossSettings.Current.GetValueOrDefault("sipNotifyPer", "0");
            if (isNotificationPermission == "0")
            {
                checkNotifyPermission();
            }
        }


        public void endPage()
        {
            MessagingCenter.Unsubscribe<string, string>("HttpControler", "GetServerVersion");
            //Core.Stop();
        }

        public void changePhone()
        {
            Navigation.PushAsync(new LoginPage());
        }


        public async void checkVersion(string versionNum)
        {
            string curVer = CrossDeviceInfo.Current.AppVersion;
            if (curVer!=versionNum)
            {
                var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                {
                    Title = "Обновление",
                    Message = "Доступна новая версия приложения",
                    OkText = "Скачать",
                    CancelText = "Отмена"
                });
                if (result)
                {
                    await Launcher.OpenAsync("https://ic.pismo-fsin.ru/upgrade/OwnLink.fsin.apk");
                }               
                    
            }
        }

        public async void checkNotifyPermission()
        {
            var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
            {
                Title = "Дополнительные разрешения",
                Message = "Для корректной работы приложения нужно перейти в Настройки и включить для приложения Автозапуск и разрешить Уведомления.",
                OkText = "Настройки",
                CancelText = "Отмена"
            });
            if (result)
            {
                openSettings.GoToSettings();
                CrossSettings.Current.AddOrUpdateValue("sipNotifyPer", "1");
            }
        }

        public void reconnect()
        {
            try
            {
                Core.ClearAllAuthInfo();
                Core.ClearProxyConfig();
                RegStatus = "Оффлайн";
                RegStatusIcon = "StatusOffline.png";

                var authInfo = Factory.Instance.CreateAuthInfo(Phone, null, _pass, null, null, "ic.pismo-fsin.ru");
                var transports = Core.Transports;
                transports.UdpPort = 0;
                transports.TlsPort = 5061;
                transports.TcpPort = 0;
                Core.Transports = transports;
                Core.AddAuthInfo(authInfo);

                var proxyConfig = Core.CreateProxyConfig();
                var identity = Factory.Instance.CreateAddress("sip:" + Phone + "@ic.pismo-fsin.ru");
                identity.Username = Phone;
                identity.Domain = "ic.pismo-fsin.ru";
                identity.Transport = Linphone.TransportType.Tls;
                proxyConfig.Edit();
                proxyConfig.IdentityAddress = identity;
                proxyConfig.ServerAddr = "ic.pismo-fsin.ru";
                proxyConfig.Route = "ic.pismo-fsin.ru";
                proxyConfig.RegisterEnabled = true;
                proxyConfig.Done();
                Core.AddProxyConfig(proxyConfig);
                Core.DefaultProxyConfig = proxyConfig;

                Core.RefreshRegisters();
            }
            catch(Exception ex)
            {
                string _login = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");
                string deviceId = CrossDeviceInfo.Current.Id;
                string deviceInfo = CrossDeviceInfo.Current.Manufacturer + " " + CrossDeviceInfo.Current.Model + " " + CrossDeviceInfo.Current.Platform + " " + CrossDeviceInfo.Current.Version;
                HttpControler.ErrorLogSend(_login, deviceInfo, deviceId, "Core init error. MainPage. reconnect " + ex.Message);
            }
            
           // notificationManager.ScheduleNotification("Своя Связь", "тест");

        }

        private void OnRegistration(Core lc, ProxyConfig config, RegistrationState state, string message)
        {
            if (state == RegistrationState.Ok)
            {
                RegStatus = "Онлайн";
                RegStatusIcon = "StatusOnline.png";
            }
            if (state == RegistrationState.Progress)
            {
                RegStatus = "Подключение";
                RegStatusIcon = "StatusPending.png";
            }
        }


        private void OnCall(Core lc, Call lcall, CallState state, string message)
        {
            try
            {
               // Core.StopRinging();
                if (Core.CallsNb > 0)
                {

                    if (Core.CurrentCall.State == CallState.IncomingReceived)
                    {
                        if (FlagSleep==0)
                            MessagingCenter.Send<string, string>("Call", "CallState", "Incoming");
                        else
                        {
                            //if (isNotifySend == 0)
                            //{
                                string userName = Core.CurrentCall.RemoteAddress.DisplayName;
                                notificationManager.ScheduleNotification("Своя Связь", "Входящий звонок " + userName);
                                isNotifySend = 1;
                            //}
                        }
                    }
                    else if (Core.CurrentCall.State == CallState.StreamsRunning)
                    {

                    }
                }
            }
            catch(Exception ex)
            {
                //UserDialogs.Instance.Alert(ex.Message);
                string _login = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");
                string deviceId = CrossDeviceInfo.Current.Id;
                string deviceInfo = CrossDeviceInfo.Current.Manufacturer + " " + CrossDeviceInfo.Current.Model + " " + CrossDeviceInfo.Current.Platform + " " + CrossDeviceInfo.Current.Version;
                HttpControler.ErrorLogSend(_login, deviceInfo, deviceId, "Call OnCall. MainPage. OnCall " + ex.Message);
                MessagingCenter.Send<string, string>("Call", "CallState", "End");
            }
        }
      

        private void OnLogCollectionUpload(Core lc, CoreLogCollectionUploadState state, string info)
        {
            //UserDialogs.Instance.Alert("info");
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

        public string RegStatus
        {
            get
            {
                return _regStatus;
            }
            set
            {
                if (_regStatus != value)
                {
                    _regStatus = value;
                    OnPropertyChanged("RegStatus");
                }
            }
        }

        public string VersionNumber
        {
            get
            {
                return _versionNumber;
            }
            set
            {
                if (_versionNumber != value)
                {
                    _versionNumber = value;
                    OnPropertyChanged("VersionNumber");
                }
            }
        }

        public string RegStatusIcon
        {
            get
            {
                return _regStatusIcon;
            }
            set
            {
                if (_regStatusIcon != value)
                {
                    _regStatusIcon = value;
                    OnPropertyChanged("RegStatusIcon");
                }
            }
        }

    }
}

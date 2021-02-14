using Acr.UserDialogs;
using Linphone;
using Plugin.DeviceInfo;
using Plugin.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace OwnLink.ViewModel
{
    public class CallPageViewModel : INotifyPropertyChanged
    {

        private Core Core
        {
            get
            {
                return ((App)App.Current).Core;
            }
        }

        public string _name { get; set; }
        public int _duration { get; set; }
        public bool _isAccept { get; set; }
        public bool _isPaused { get; set; }
        public bool _isIncoming { get; set; }

        public bool _isSpeakerPhoneOn { get; set; }
        public bool _isMicOn { get; set; }

        public bool firstOpen { get; set; }

        public ICommand AcceptCall { get; set; }
        public ICommand CancelCall { get; set; }
        public ICommand MuteCall { get; set; }
        public ICommand ResumeCall { get; set; }
        public ICommand SpeakerCall { get; set; }
        public INavigation Navigation { get; set; }

        public Call curCall;

        public IPlaySoundService playSoundService;
        public ISpeakerPhone speakerPhone;

        public CallPageViewModel()
        {

            AcceptCall = new Command(accCall);
            CancelCall = new Command(cancelCall);
            MuteCall = new Command(muteCall);
            SpeakerCall = new Command(speakerCall);
            ResumeCall = new Command(resumeCall);

            playSoundService = DependencyService.Get<IPlaySoundService>();
            speakerPhone = DependencyService.Get<ISpeakerPhone>();
            firstOpen = true;
            
        }

        public void startPage()
        {
            curCall = Core.CurrentCall;
            
            Name = "";

            if (curCall != null)
            {
                Name = curCall.RemoteAddress.DisplayName;
            }
            else
            {
                string _login = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");
                string deviceId = CrossDeviceInfo.Current.Id;
                string deviceInfo = CrossDeviceInfo.Current.Manufacturer + " " + CrossDeviceInfo.Current.Model + " " + CrossDeviceInfo.Current.Platform + " " + CrossDeviceInfo.Current.Version;
                HttpControler.ErrorLogSend(_login,deviceInfo,deviceId,"Current call dont exist. CallPage. StartPage");
                MessagingCenter.Send<string, string>("Call", "CallState", "End");
            }

            if (!IsAccept)
                Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
            
            if (firstOpen)
            {
                Duration = 0;
                IsAccept = false;
                IsIncoming = true;
                IsSpeakerPhoneOn = false;
                IsMicOn = true;
                firstOpen = false;
                IsPaused = false;
            }

           // playSoundService.InitSystemSound();
           // playSoundService.PlaySystemSound();

        }


        public void endPage()
        {
            
        }

        public void accCall()
        {
            try
            {
                if (curCall != null)
                {
                    // playSoundService.StopSystemSound();
                    curCall.Accept();

                    IsAccept = true;
                    IsIncoming = false;
                }
                else
                    MessagingCenter.Send<string, string>("Call", "CallState", "End");
            }
            catch(Exception ex)
            {
                string _login = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");
                string deviceId = CrossDeviceInfo.Current.Id;
                string deviceInfo = CrossDeviceInfo.Current.Manufacturer + " " + CrossDeviceInfo.Current.Model + " " + CrossDeviceInfo.Current.Platform + " " + CrossDeviceInfo.Current.Version;
                HttpControler.ErrorLogSend(_login, deviceInfo, deviceId, "Current call dont exist. CallPage. accCall "+ex.Message);
                MessagingCenter.Send<string, string>("Call", "CallState", "End");
            }
        }

        public void cancelCall()
        {
            try
            {
                Core.TerminateAllCalls();
                MessagingCenter.Send<string, string>("Call", "CallState", "End");
            }
            catch (Exception ex)
            {
                string _login = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");
                string deviceId = CrossDeviceInfo.Current.Id;
                string deviceInfo = CrossDeviceInfo.Current.Manufacturer + " " + CrossDeviceInfo.Current.Model + " " + CrossDeviceInfo.Current.Platform + " " + CrossDeviceInfo.Current.Version;
                HttpControler.ErrorLogSend(_login, deviceInfo, deviceId, "Current call dont exist. CallPage. cancelCall "+ex.Message);
                MessagingCenter.Send<string, string>("Call", "CallState", "End");
            }
        }

        public void muteCall()
        {
            try
            {
                if (curCall != null)
                {
                    curCall.MicrophoneMuted = !curCall.MicrophoneMuted;
                    IsMicOn = !IsMicOn;
                }
            }
            catch(Exception ex)
            {
                string _login = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");
                string deviceId = CrossDeviceInfo.Current.Id;
                string deviceInfo = CrossDeviceInfo.Current.Manufacturer + " " + CrossDeviceInfo.Current.Model + " " + CrossDeviceInfo.Current.Platform + " " + CrossDeviceInfo.Current.Version;
                HttpControler.ErrorLogSend(_login, deviceInfo, deviceId, "Current call dont exist. CallPage. muteCall "+ex.Message);
                MessagingCenter.Send<string, string>("Call", "CallState", "End");
            }
        }

        public void speakerCall()
        {
            if (IsSpeakerPhoneOn)
            {
                IsSpeakerPhoneOn = false;
                speakerPhone.SpeakerphoneOff();
            }
            else
            {
                IsSpeakerPhoneOn = true;
                speakerPhone.SpeakerphoneOn();
            }
        }


        public void resumeCall()
        {
            if (curCall.State==CallState.Paused)
                curCall.Resume();
        }
        private bool OnTimerTick()
        {
            try
            {
                if (curCall.State == CallState.Paused)
                    IsPaused = true;
                else
                    IsPaused = false;
                if (Core.CallsNb > 0)
                {
                    if (IsAccept)
                        Duration += 1;
                    return true;
                }
                else
                {
                    //playSoundService.StopSystemSound();
                    MessagingCenter.Send<string, string>("Call", "CallState", "End");
                    return false;
                }
                
                
            }
            catch(Exception ex)
            {
                //MessagingCenter.Send<string, string>("Call", "CallState", "End");
                string _login = CrossSettings.Current.GetValueOrDefault("sipPhoneLogin", "");
                string deviceId = CrossDeviceInfo.Current.Id;
                string deviceInfo = CrossDeviceInfo.Current.Manufacturer + " " + CrossDeviceInfo.Current.Model + " " + CrossDeviceInfo.Current.Platform + " " + CrossDeviceInfo.Current.Version;
                HttpControler.ErrorLogSend(_login, deviceInfo, deviceId, "Current call dont exist. CallPage. OnTimerTick " + ex.Message);
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }



        public int Duration
        {
            get
            {
                return _duration;
            }
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged("DurationFormat");
                }
            }
        }

        public string DurationFormat
        {
            get
            {
                string mm = "";
                string ss = "";
                if (Duration % 60 > 9)
                    ss += (Duration % 60).ToString();
                else
                    ss+="0"+ (Duration % 60).ToString();
                if (Duration / 60 > 9)
                    mm += (Duration / 60).ToString();
                else
                    mm += "0" + (Duration / 60).ToString();
                return mm + ":" + ss;
            }
        }

        public bool IsIncoming
        {
            get
            {
                return _isIncoming;
            }
            set
            {
                if(_isIncoming != value)
                {
                    _isIncoming = value;
                    OnPropertyChanged("IsIncoming");
                }
            }
        }

        public bool IsAccept
        {
            get
            {
                return _isAccept;
            }
            set
            {
                if (_isAccept != value)
                {
                    _isAccept = value;
                    OnPropertyChanged("IsAccept");
                }
            }
        }

        public bool IsMicOn
        {
            get
            {
                return _isMicOn;
            }
            set
            {
                if (_isMicOn != value)
                {
                    _isMicOn = value;
                    OnPropertyChanged("IsMicOn");
                    OnPropertyChanged("MicButtonColor");
                    OnPropertyChanged("MicLabelTextColor");
                }
            }
        }

        public bool IsPaused
        {
            get
            {
                return _isPaused;
            }
            set
            {
                if (_isPaused != value)
                {
                    _isPaused = value;
                    OnPropertyChanged("IsPaused");
                }
            }
        }

        public bool IsMicOff
        {
            get
            {
                return !_isMicOn;
            }
        }

        public bool IsSpeakerPhoneOn
        {
            get
            {
                return _isSpeakerPhoneOn;
            }
            set
            {
                if (_isSpeakerPhoneOn != value)
                {
                    _isSpeakerPhoneOn = value;
                    OnPropertyChanged("IsSpeakerPhoneOn");
                    OnPropertyChanged("SpeakerButtonColor");
                    OnPropertyChanged("SpeakerLabelTextColor");
                }
            }
        }

        public bool IsSpeakerPhoneOff
        {
            get
            {
                return !_isSpeakerPhoneOn;
            }
        }

        public string MicButtonColor
        {
            get
            {
                if (IsMicOn)
                    return "White";
                else
                    return "Gray";
            }
        }

        public string MicLabelTextColor
        {
            get
            {
                if (IsMicOn)
                    return "Black";
                else
                    return "White";
            }
        }

        public string SpeakerButtonColor
        {
            get
            {
                if (IsSpeakerPhoneOn)
                    return "White";
                else
                    return "Gray";
            }
        }

        public string SpeakerLabelTextColor
        {
            get
            {
                if (IsSpeakerPhoneOn)
                    return "Black";
                else
                    return "White";
            }
        }
    }
}

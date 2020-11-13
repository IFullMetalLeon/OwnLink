using Acr.UserDialogs;
using Linphone;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public bool _isIncoming { get; set; }

        public bool _isSpeakerPhoneOn { get; set; }
        public bool _isMicOn { get; set; }

        public ICommand AcceptCall { get; set; }
        public ICommand CancelCall { get; set; }
        public ICommand MuteCall { get; set; }
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

            playSoundService = DependencyService.Get<IPlaySoundService>();
            speakerPhone = DependencyService.Get<ISpeakerPhone>();

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
                MessagingCenter.Send<string, string>("Call", "CallState", "End");
            Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
            Duration = 0;
            IsAccept = false;
            IsIncoming = true;
            IsSpeakerPhoneOn = false;
            IsMicOn = true;

            playSoundService.InitSystemSound();
            playSoundService.PlaySystemSound();
        }


        public void endPage()
        {
            
        }

        public void accCall()
        {
            if (curCall != null)
            {
                playSoundService.StopSystemSound();
                curCall.Accept();

                IsAccept = true;
                IsIncoming = false;
                try
                {
                    PayloadType cur = curCall.Params.UsedAudioPayloadType;
                }
                catch(Exception ex) { }
            }
            else
                MessagingCenter.Send<string, string>("Call", "CallState", "End");
        }

        public void cancelCall()
        {
            Core.TerminateAllCalls();
            playSoundService.StopSystemSound();
            MessagingCenter.Send<string, string>("Call", "CallState", "End");
        }

        public void muteCall()
        {
            if (curCall != null)
            {
                curCall.MicrophoneMuted = !curCall.MicrophoneMuted;
                IsMicOn = !IsMicOn;
            }
            IsMicOn = !IsMicOn;
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
            IsSpeakerPhoneOn = !IsSpeakerPhoneOn;
        }

        private bool OnTimerTick()
        {
            if (Core.CallsNb > 0)
            {
                if (IsAccept)
                    Duration += 1;
                return true;
            }
            else
            {
                playSoundService.StopSystemSound();
                MessagingCenter.Send<string, string>("Call", "CallState", "End");
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

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
        public string _codec { get; set; }
        public int _duration { get; set; }
        public bool _isAccept { get; set; }
        public bool _isIncoming { get; set; }

        public bool isSpeakerPhoneOn;

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
            isSpeakerPhoneOn = false;

            Codec = "Неизвестный кодек";

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
                    //curCall.Params.
                    Codec = cur.MimeType;
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
            }
        }

        public void speakerCall()
        {
            if (isSpeakerPhoneOn)
            {
                isSpeakerPhoneOn = false;
                speakerPhone.SpeakerphoneOff();
            }
            else
            {
                isSpeakerPhoneOn = true;
                speakerPhone.SpeakerphoneOn();
            }
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

        public string Codec
        {
            get
            {
                return _codec;
            }
            set
            {
                if (_codec != value)
                {
                    _codec = value;
                    OnPropertyChanged("Codec");
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

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

[assembly: Xamarin.Forms.Dependency(typeof(OwnLink.Android.AndroidSpeakerPhone))]
namespace OwnLink.Android
{
    class AndroidSpeakerPhone : ISpeakerPhone
    {
        public void SpeakerphoneOff()
        {
            AudioManager am = (AudioManager)Application.Context.GetSystemService(Context.AudioService);
            am.Mode = Mode.InCall;
            am.SpeakerphoneOn = false;
            am.SetStreamVolume(Stream.VoiceCall, am.GetStreamMaxVolume(Stream.VoiceCall), VolumeNotificationFlags.ShowUi);

        }

        public void SpeakerphoneOn()
        {
            AudioManager am = (AudioManager)Application.Context.GetSystemService(Context.AudioService);
            am.Mode = Mode.InCall;
            am.SpeakerphoneOn = true;
            am.SetStreamVolume(Stream.VoiceCall, am.GetStreamMaxVolume(Stream.VoiceCall), VolumeNotificationFlags.ShowUi);

        }
    }
}
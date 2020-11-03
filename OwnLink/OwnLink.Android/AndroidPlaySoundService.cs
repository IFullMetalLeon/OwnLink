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

[assembly: Xamarin.Forms.Dependency(typeof(OwnLink.Android.AndroidPlaySoundService))]
namespace OwnLink.Android
{
    class AndroidPlaySoundService : IPlaySoundService
    {
        Ringtone rt { get; set; }
        public void InitSystemSound()
        {
            var uri = RingtoneManager.GetDefaultUri(RingtoneType.Ringtone);
            rt = RingtoneManager.GetRingtone(MainActivity.instance.ApplicationContext, uri);
        }

        public void PlaySystemSound()
        {
            if (!rt.IsPlaying)
                rt.Play();            
        }

        public void StopSystemSound()
        {            
            if (rt.IsPlaying)
                rt.Stop();
        }
    }
}
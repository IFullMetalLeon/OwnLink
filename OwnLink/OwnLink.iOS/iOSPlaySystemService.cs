using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioToolbox;
using Foundation;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(OwnLink.iOS.iOSPlaySystemService))]
namespace OwnLink.iOS
{
    class iOSPlaySystemService : IPlaySoundService
    {
        public void InitSystemSound()
        {
            throw new NotImplementedException();
        }

        public void PlaySystemSound()
        {
            var sound = new SystemSound(1000);
            sound.PlaySystemSound();
        }

        public void StopSystemSound()
        {
            var sound = new SystemSound(1000);
            sound.Close();
        }
    }
}
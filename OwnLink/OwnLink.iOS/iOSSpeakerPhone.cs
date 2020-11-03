using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioToolbox;
using AVFoundation;
using Foundation;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(OwnLink.iOS.iOSSpeakerPhone))]
namespace OwnLink.iOS
{
    class iOSSpeakerPhone : ISpeakerPhone
    {
        public void SpeakerphoneOff()
        {
            try
            {
                /**/
                NSError error = AVFoundation.AVAudioSession.SharedInstance().SetCategory(AVFoundation.AVAudioSessionCategory.PlayAndRecord, AVFoundation.AVAudioSessionCategoryOptions.AllowBluetooth);
                var audioSession = AVAudioSession.SharedInstance();
                if (error == null)
                {
                    if (AVFoundation.AVAudioSession.SharedInstance().SetMode(AVFoundation.AVAudioSession.ModeVoiceChat, out error))
                    {
                        if (AVFoundation.AVAudioSession.SharedInstance().OverrideOutputAudioPort(AVFoundation.AVAudioSessionPortOverride.None, out error))
                        {
                            error = AVFoundation.AVAudioSession.SharedInstance().SetActive(true);

                            if (error != null)
                            {
                                //  Logger.Log(new Exception(error?.LocalizedDescription ?? "Cannot set active"));
                            }
                        }
                        else
                        {
                            // Logger.Log(new Exception(error?.LocalizedDescription ?? "Cannot override output audio port"));
                        }
                    }
                    else
                    {
                        //Logger.Log(new Exception("Cannot set mode"));
                    }
                }
                else
                {
                    //Logger.Log(new Exception(error?.LocalizedDescription ?? "Cannot set category"));
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void SpeakerphoneOn()
        {
            try
            {
                /**/
                NSError error = AVFoundation.AVAudioSession.SharedInstance().SetCategory(AVFoundation.AVAudioSessionCategory.PlayAndRecord, AVFoundation.AVAudioSessionCategoryOptions.DefaultToSpeaker);

                if (error == null)
                {
                    if (AVFoundation.AVAudioSession.SharedInstance().SetMode(AVFoundation.AVAudioSession.ModeVoiceChat, out error))
                    {
                        if (AVFoundation.AVAudioSession.SharedInstance().OverrideOutputAudioPort(AVFoundation.AVAudioSessionPortOverride.Speaker, out error))
                        {
                            error = AVFoundation.AVAudioSession.SharedInstance().SetActive(true);

                            if (error != null)
                            {
                                //  Logger.Log(new Exception(error?.LocalizedDescription ?? "Cannot set active"));
                            }
                        }
                        else
                        {
                            // Logger.Log(new Exception(error?.LocalizedDescription ?? "Cannot override output audio port"));
                        }
                    }
                    else
                    {
                        //Logger.Log(new Exception("Cannot set mode"));
                    }
                }
                else
                {
                    //Logger.Log(new Exception(error?.LocalizedDescription ?? "Cannot set category"));
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
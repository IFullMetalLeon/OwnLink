﻿
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content.Res;
using System.IO;
using Xamarin.Forms.Platform.Android;
using Android;
using Android.Util;
using System.Collections.Generic;
using Xamarin.Forms;
using Android.Views;
using Android.Widget;
using System;
using Acr.UserDialogs;
using Plugin.Settings;
using Plugin.CurrentActivity;
using Android.Content;
using Android.Database;
using Android.Provider;
using Firebase.Iid;
using AndroidApp = Android.App.Application;
using Android.Media;

namespace OwnLink.Android
{
    [Activity(Label = "Своя Связь", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : FormsAppCompatActivity
    {
        int PERMISSIONS_REQUEST = 101;
        TextureView displayCamera;
        TextureView captureCamera;
        const string channelId = "OwnLinkBG";
        const string channelName = "OwnLinkBG";
        const string channelDescription = "The OwnLinkBG channel for notifications.";
        internal static readonly string CHANNEL_ID = "my_nмotification_channel";
        internal static readonly int NOTIFICATION_ID = 100;

        public static MainActivity instance { set; get; }
        protected override void OnCreate(Bundle bundle)
        {


            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            AssetManager assets = Assets;
            string path = FilesDir.AbsolutePath;
            string rc_path = path + "/default_rc";
            if (!File.Exists(rc_path))
            {
                using (StreamReader sr = new StreamReader(assets.Open("linphonerc_default")))
                {
                    string content = sr.ReadToEnd();
                    File.WriteAllText(rc_path, content);
                }
            }
            string factory_path = path + "/factory_rc";
            if (!File.Exists(factory_path))
            {
                using (StreamReader sr = new StreamReader(assets.Open("linphonerc_factory")))
                {
                    string content = sr.ReadToEnd();
                    File.WriteAllText(factory_path, content);
                }
            }

            UserDialogs.Init(this);           
            Forms.Init(this, bundle);
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);

            App.ConfigFilePath = rc_path;
            App.FactoryFilePath = factory_path;

            App app = new App(this.Handle);

            System.Diagnostics.Debug.WriteLine("DEVICE=" + Build.Device);
            System.Diagnostics.Debug.WriteLine("MODEL=" + Build.Model);
            System.Diagnostics.Debug.WriteLine("MANUFACTURER=" + Build.Manufacturer);
            System.Diagnostics.Debug.WriteLine("SDK=" + Build.VERSION.Sdk);

            LinearLayout fl = new LinearLayout(this);
            ViewGroup.LayoutParams lparams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            fl.LayoutParameters = lparams;

            displayCamera = new TextureView(this);
            ViewGroup.LayoutParams dparams = new ViewGroup.LayoutParams(640, 480);
            displayCamera.LayoutParameters = dparams;

            captureCamera = new TextureView(this);
            ViewGroup.LayoutParams cparams = new ViewGroup.LayoutParams(320, 240);
            captureCamera.LayoutParameters = cparams;

            fl.AddView(displayCamera);
            fl.AddView(captureCamera);
            //app.getLayoutView().Children.Add(fl);

            app.Core.NativeVideoWindowId = displayCamera.Handle;
            app.Core.NativePreviewWindowId = captureCamera.Handle;

            app.Core.VideoDisplayEnabled = true;
            app.Core.VideoCaptureEnabled = true;

            instance = this;

            CreateNotificationChannel();

            LoadApplication(app);
            //CreateNotificationFromIntent(Intent);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (Int32.Parse(Build.VERSION.Sdk) >= 23)
            {
                List<string> Permissions = new List<string>();
                if (CheckSelfPermission(Manifest.Permission.Camera) != Permission.Granted)
                {
                    Permissions.Add(Manifest.Permission.Camera);
                }
                if (CheckSelfPermission(Manifest.Permission.RecordAudio) != Permission.Granted)
                {
                    Permissions.Add(Manifest.Permission.RecordAudio);
                }
                if(CheckSelfPermission(Manifest.Permission.ReadCallLog) != Permission.Granted)
                {
                    Permissions.Add(Manifest.Permission.ReadCallLog);
                }
                if (Permissions.Count > 0)
                {
                    RequestPermissions(Permissions.ToArray(), PERMISSIONS_REQUEST);
                }
            }
            
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == PERMISSIONS_REQUEST)
            {
                int i = 0;
                foreach (string permission in permissions)
                {
                    Log.Info("LinphoneXamarin", "Permission " + permission + " : " + grantResults[i]);
                    i += 1;
                }
            }
        }

        void CreateNotificationChannel()
        {
            NotificationManager manager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Max)
                {
                    Description = channelDescription
                };
                //channel.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone), null);
                channel.SetSound(null, null);
                channel.LockscreenVisibility = NotificationVisibility.Public;
                channel.EnableVibration(true);

                manager.CreateNotificationChannel(channel);
            }

        }

        protected override void OnNewIntent(Intent intent)
        {
            CreateNotificationFromIntent(intent);
        }

        void CreateNotificationFromIntent(Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.Extras.GetString(AndroidNotificationManager.TitleKey);
                string message = intent.Extras.GetString(AndroidNotificationManager.MessageKey);
                DependencyService.Get<INotificationManager>().ReceiveNotification(title, message);
            }
        }

    }
}


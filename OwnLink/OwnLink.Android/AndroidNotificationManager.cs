using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V4.App;
using OwnLink;
using OwnLink.Android;
using System;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;

[assembly: Dependency(typeof(OwnLink.Android.AndroidNotificationManager))]
namespace OwnLink.Android
{
    public class AndroidNotificationManager : INotificationManager
    {
        const string channelId = "default";
        const string channelName = "Default";
        const string channelDescription = "The default channel for notifications.";
        const int pendingIntentId = 0;

        public const string TitleKey = "title";
        public const string MessageKey = "message";

        bool channelInitialized = false;
        int messageId = -1;
        NotificationManager manager;

        public event EventHandler NotificationReceived;

        public void Initialize()
        {
            CreateNotificationChannel();
        }

        public int ScheduleNotification(string title, string message)
        {
            if (!channelInitialized)
            {
                CreateNotificationChannel();
            }

            messageId++;

            Intent intent = new Intent(AndroidApp.Context, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);

            PendingIntent pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, pendingIntentId, intent, PendingIntentFlags.OneShot);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, channelId)
                //.SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetVisibility((int)NotificationVisibility.Public)
                .SetFullScreenIntent(pendingIntent,true)
                .SetPriority((int)NotificationPriority.Max)               
                .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, Resource.Drawable.icon_large))
                .SetSmallIcon(Resource.Drawable.notification_tile_bg)
                .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone))
                //.SetOngoing(true)
                .SetAutoCancel(true)
                .SetDefaults((int)NotificationDefaults.Sound);

            var notification = builder.Build();
            manager.Notify(messageId, notification);

            return messageId;
        }

        public void ReceiveNotification(string title, string message)
        {
            var args = new NotificationEventArgs()
            {
                Title = title,
                Message = message,
            };
            NotificationReceived?.Invoke(null, args);
        }

        void CreateNotificationChannel()
        {
            manager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Max)
                {
                    Description = channelDescription
                };
                channel.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone), null);
                channel.LockscreenVisibility = NotificationVisibility.Public;
                channel.EnableVibration(true);

                manager.CreateNotificationChannel(channel);
            }

            channelInitialized = true;
        }
    }
}
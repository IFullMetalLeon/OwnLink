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
using Firebase.Messaging;
using AndroidApp = Android.App.Application;
using System.Collections.Generic;

namespace OwnLink.Android
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "MyFirebaseMsgService";

        /**
         * Called when message is received.
         */

        // [START receive_message]
        public override void OnNewToken(string p0)
        {
            base.OnNewToken(p0);
            System.Diagnostics.Debug.WriteLine(TAG, p0);
        }
        public override void OnMessageReceived(RemoteMessage message)
        {
            // TODO(developer): Handle FCM messages here.
            // If the application is in the foreground handle both data and notification messages here.
            // Also if you intend on generating your own notifications as a result of a received FCM
            // message, here is where that should be initiated. See sendNotification method below.
            System.Diagnostics.Debug.WriteLine(TAG, "From: " + message.From);
            RemoteMessage.Notification tmp=message.GetNotification();
            if (tmp != null)
            {
                System.Diagnostics.Debug.WriteLine(TAG, "Notification Message Body: " + message.GetNotification().Body);
                var body = message.GetNotification().Body;
                ScheduleNotification(body, message.Data);
            }
            else
            {
                ScheduleNotification("123", message.Data);
            }
        }
        // [END receive_message]

        /**
         * Create and show a simple notification containing the received FCM message.
         */
       /* void SendNotification(string messageBody, IDictionary<string, string> data)
        {
            
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            foreach (var key in data.Keys)
            {
                intent.PutExtra(key, data[key]);
            }

            string title = "123";
            if (data.ContainsKey("type"))
                title = data["type"];
            string body = "123";
            if (data.ContainsKey("caller_name"))
                body = data["caller_name"];

            var pendingIntent = PendingIntent.GetActivity(this,
                                                          MainActivity.NOTIFICATION_ID,
                                                          intent,
                                                          PendingIntentFlags.OneShot);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                //.SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetVisibility((int)NotificationVisibility.Public)
                .SetFullScreenIntent(pendingIntent, true)
                .SetPriority((int)NotificationPriority.Max)
                .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, Resource.Drawable.icon_large))
                .SetSmallIcon(Resource.Drawable.notification_tile_bg)
                .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone))
                //.SetOngoing(true)
                .SetAutoCancel(true)
                .SetDefaults((int)NotificationDefaults.Sound);

            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                                      .SetSmallIcon(Resource.Drawable.logo)
                                      .SetContentTitle(title)
                                      .SetContentText(body)
                                      .SetAutoCancel(true)
                                      .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(MainActivity.NOTIFICATION_ID, builder.Build());
        }*/


        const string channelId = "OwnLinkFCM";
        const string channelName = "OwnLinkFCM";
        const string channelDescription = "The OwnLinkFCM channel for notifications.";
        const int pendingIntentId = 0;

        public const string TitleKey = "title";
        public const string MessageKey = "message";

        bool channelInitialized = false;
        int messageId = 100;
        NotificationManager manager;

        public event EventHandler NotificationReceived;

        public void Initialize()
        {
            CreateNotificationChannel();
        }

        public int ScheduleNotification(string messageBody, IDictionary<string, string> data)
        {
            if (!channelInitialized)
            {
                CreateNotificationChannel();
            }

            string title = "Входящий звонок";
            if (data.ContainsKey("type"))
                title = data["type"];
            string body = "";
            if (data.ContainsKey("caller_name"))
                body = data["caller_name"];

            messageId++;

            Intent intent = new Intent(AndroidApp.Context, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, body);

            PendingIntent pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, pendingIntentId, intent, PendingIntentFlags.OneShot);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, channelId)
                //.SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetVisibility((int)NotificationVisibility.Public)
                .SetFullScreenIntent(pendingIntent, true)
                .SetPriority((int)NotificationPriority.Max)
                .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, Resource.Drawable.icon_large))
                .SetSmallIcon(Resource.Drawable.notification_tile_bg)
                .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone))
                //.SetOngoing(true)
                .SetAutoCancel(true)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);

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
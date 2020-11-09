using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.App;
using Firebase.Messaging;

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
                SendNotification(body, message.Data);
            }
            else
            {
                SendNotification("123", message.Data);
            }
        }
        // [END receive_message]

        /**
         * Create and show a simple notification containing the received FCM message.
         */
        void SendNotification(string messageBody, IDictionary<string, string> data)
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

            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                                      .SetSmallIcon(Resource.Drawable.logo)
                                      .SetContentTitle(title)
                                      .SetContentText(body)
                                      .SetAutoCancel(true)
                                      .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(MainActivity.NOTIFICATION_ID, notificationBuilder.Build());
        }
    }

}
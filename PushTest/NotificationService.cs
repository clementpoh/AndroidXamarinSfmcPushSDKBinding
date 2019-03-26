using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Firebase.Iid;
using Android.Util;
using Firebase.Messaging;
using Android.OS;
using Com.Salesforce.Marketingcloud;
using Com.Salesforce.Marketingcloud.Messages.Push;
using System.Linq;

namespace PushTest
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class FirebaseIIDService : FirebaseInstanceIdService
    {
        public static string TAG = "FIrebaseIIDservice";
        public override void OnTokenRefresh()
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            Log.Debug(TAG, $"Refreshed token: " + refreshedToken);
        }
    }

    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MessagingService : FirebaseMessagingService
    {
        static readonly string TAG = "MessagingService";

        static readonly string CHANNEL_ID = "Messages";
        static readonly string CHANNEL_NAME = CHANNEL_ID;

        public override void HandleIntent(Intent intent)
        {
            try
            {
                Log.Debug(TAG, "Handling notification intent");
                if (intent.Extras != null)
                {
                    var builder = new RemoteMessage.Builder("MessagingService");

                    foreach (string key in intent.Extras.KeySet())
                    {
                        builder.AddData(key, intent.Extras.Get(key).ToString());
                        Log.Debug(TAG, $"{key} : {intent.Extras.Get(key).ToString()}");
                    }
                    OnMessageReceived(builder.Build());
                }
                else
                {
                    base.HandleIntent(intent);
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex.StackTrace);
                base.HandleIntent(intent);
            }
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            var notification = message.GetNotification();
            Log.Debug(TAG, $"From: {message.From}");
            Log.Debug(TAG, $"Data: {message.Data.ToString()}");
			Log.Debug(TAG, $"{ message.ToString() }");

            if (PushMessageManager.IsMarketingCloudPush(message.Data)) 
            {
                Log.Debug(TAG, "Marketing Cloud Message");
                MarketingCloudSdk.RequestSdk(new SdkReady {
                    OnReady = sdk => sdk.PushMessageManager.HandleMessage(message.Data)
                });

            }
            else
            {
                Log.Debug(TAG, "Normal Message");
                SendNotification(notification, message.Data);
            }
        }
            
        void SendNotification(RemoteMessage.Notification notification, IDictionary<string, string> data)
        {
            Log.Debug(TAG, $"SendNotification");
            try
            {
                var intent = new Intent(this, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                data.Select(item => intent.PutExtra(item.Key, item.Value));

                var notificationBuilder = new Notification.Builder(this, CHANNEL_ID)
                    .SetSmallIcon(Resource.Drawable.abc_ic_ab_back_material)
                    .SetContentTitle(notification.Title)
                    .SetContentText(notification.Body)
                    .SetAutoCancel(true)
                    .SetContentIntent(PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot));

                var notificationManager = NotificationManager.FromContext(this);

                if ((notificationManager != null) && (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O))
                {
                    var channel = new NotificationChannel(CHANNEL_ID, CHANNEL_NAME, NotificationImportance.Default);
                    notificationManager.CreateNotificationChannel(channel);
                    Log.Debug(TAG, $"Push notification channel {CHANNEL_ID} created");
                }

                notificationManager.Notify(0, notificationBuilder.Build());
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"SendNotification: {ex.Message}");
            }
        }
    }
}

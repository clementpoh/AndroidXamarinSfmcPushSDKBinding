using System;
using Android.App;
using Android.Arch.Lifecycle;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.Runtime;
using Android.Util;
using Com.Salesforce.Marketingcloud;
using Com.Salesforce.Marketingcloud.Notifications;
using Com.Salesforce.Marketingcloud.Registration;
using Com.Salesforce.Marketingcloud.Messages;
using Com.Salesforce.Marketingcloud.Messages.Geofence;

using NotificationManager = Com.Salesforce.Marketingcloud.Notifications.NotificationManager;
using Uri = Android.Net.Uri;
using Android.Content;

namespace PushTest
{
    [Application]
    public class PushApplication : Application,
        MarketingCloudSdk.IInitializationListener,
        IRegistrationManagerRegistrationEventListener
    {
        private static readonly string TAG = "PushApplication";

        private static readonly string APPLICATION_ID = "";
        private static readonly string ACCESS_TOKEN = "";
        private static readonly string SERVER_URL = "";
        private static readonly string MID = "";

        private static readonly string FIREBASE_SERVER_KEY = "";

        public PushApplication(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip) {}

        public override void OnCreate()
        {
            base.OnCreate();
            InitMarketingCloud();
        }

		private class LaunchIntentProvider : Java.Lang.Object, NotificationManager.INotificationLaunchIntentProvider
		 {
			public PendingIntent GetNotificationPendingIntent(Context context, NotificationMessage message)
			 {
				var intent = string.IsNullOrEmpty(message.Url())
					? new Intent(context, typeof(MainActivity))
					: new Intent(Intent.ActionView, Uri.Parse(message.Url()));

				var pendingIntent = PendingIntent.GetActivity(context, new Random().Next(), intent, PendingIntentFlags.UpdateCurrent);
                return NotificationManager.RedirectIntentForAnalytics(context, pendingIntent, message, true);
              }
		}

		private class ChannelProvider : Java.Lang.Object, NotificationManager.INotificationChannelIdProvider
		{
			public string GetNotificationChannelId(Context context, NotificationMessage message)
			{
				return NotificationManager.CreateDefaultNotificationChannel(context);
			}
		}

		private void InitMarketingCloud()
        {
            MarketingCloudSdk.LogLevel = BuildConfig.Debug ? MCLogListener.Debug : MCLogListener.Error;
            MarketingCloudSdk.SetLogListener(new MCLogListenerAndroidLogListener());

            MarketingCloudConfig.Builder builder = MarketingCloudConfig.InvokeBuilder()
                // Set Salesforce Marketing Cloud instance details
                .SetApplicationId(APPLICATION_ID)
                .SetAccessToken(ACCESS_TOKEN)
				.SetMarketingCloudServerUrl(SERVER_URL)
                .SetMid(MID)

                // Set the Firebase service key
                .SetSenderId(FIREBASE_SERVER_KEY)

                // Enable SDK Features
                .SetAnalyticsEnabled(true)
                .SetPiAnalyticsEnabled(true)
                .SetInboxEnabled(true)
                .SetGeofencingEnabled(true)

                // Disable beacon messaging for now
                .SetProximityEnabled(false)
                .SetNotificationCustomizationOptions(
					NotificationCustomizationOptions.Create(
						Resource.Drawable.abc_ic_ab_back_material,
						new LaunchIntentProvider(),
						new ChannelProvider()
					)
				);

            MarketingCloudSdk.Init(this, builder.Build(this), this);

        }

        public void Complete(InitializationStatus status)
        {
            Log.Debug(TAG, status.ToString());

            if (!status.IsUsable)
            {
                Log.Error(TAG, "Marketing Cloud SDK init failed.", status.UnrecoverableException());
            }
        }

        public void OnRegistrationReceived(Registration registration)
        {
            MarketingCloudSdk.Instance.AnalyticsManager.TrackPageView("data://RegistrationEvent", "Registration Event Completed");
            Log.Debug(TAG, registration.ToString());
            Log.Debug(TAG, string.Format("Last sent: {0}", DateTime.Now));
        }
    }
}

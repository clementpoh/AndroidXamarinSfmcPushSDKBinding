using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Widget;
using Com.Salesforce.Marketingcloud;
using Firebase.Iid;

namespace PushTest
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ActivityCompat.IOnRequestPermissionsResultCallback
    {
        private static readonly string TAG = "MainActivity";
        private static readonly int REQUEST_LOCATION = 100;
        
        TextView txtTest;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            txtTest = FindViewById<TextView>(Resource.Id.txtTest);

            UpdateSubscriberKey();

            ArePlayServicesAvailable();
            RequestLocationPermission();

            txtTest.Text += "\nFirebase Token: " + FirebaseInstanceId.Instance.Token + "\n";
			Log.Info(TAG, "Firebase Token: " + FirebaseInstanceId.Instance.Token + "\n");

			CheckLocationStatus();

            MarketingCloudSdk.RequestSdk(new SdkReady { OnReady = sdk => {
                txtTest.Text += "\nSDK State:\n" + sdk.SdkState.ToString(2) + "\n";
            }});

        }

        public void UpdateSubscriberKey()
        {
            MarketingCloudSdk.RequestSdk(new SdkReady {
                OnReady = sdk => sdk.RegistrationManager
                    .Edit()
                    .SetContactKey("1002")
                    .SetAttribute("FirstName", "Hello")
                    .SetAttribute("LastName", "World")
                    .Commit()
            });
        }

        public void CheckLocationStatus()
        {
            MarketingCloudSdk.RequestSdk(new SdkReady {
                OnReady = sdk => {
                    var status = sdk.InitializationStatus;
                    var availability = GoogleApiAvailability.Instance;

                    if (status.LocationsError())
                    {
                        Log.Error(TAG, string.Format("MarketingCloudSdk Locations error: {0}", status.PlayServicesStatus()) );
                        Log.Info(TAG, string.Format("Google Play Services Availability: {0}", availability.GetErrorString(status.PlayServicesStatus())));

                        if (availability.IsUserResolvableError(status.PlayServicesStatus()))
                        {
                            Log.Error(TAG, "User resolvable error");
                            availability.ShowErrorNotification(this, status.PlayServicesStatus());
                        }
                        else
                        {
                            Log.Error(TAG, "Unresolvable error ");
                            Log.Error(TAG, "Play Service status: " + status.PlayServicesStatus());
                            Log.Error(TAG, "Play Service message: " + status.PlayServicesMessage());
                        }
                    }
                    else
                    {
                        Log.Debug(TAG, "MarketingCloudSdk Geofencing Success");
                    }
                }
            });
        }

        public void EnableGeofencing()
        {
            MarketingCloudSdk.RequestSdk(new SdkReady {
                OnReady = sdk => sdk.RegionMessageManager.EnableGeofenceMessaging()
            });
        }

        public bool ArePlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                txtTest.Text += GoogleApiAvailability.Instance.IsUserResolvableError(resultCode)
                    ? GoogleApiAvailability.Instance.GetErrorString(resultCode)
                    : "\nGoogle Play is not supported\n";

                GoogleApiAvailability.Instance.MakeGooglePlayServicesAvailable(this);
                return false;
            }
            else
            {
                txtTest.Text += "\nGoogle Play Services are available\n";
                return true;
            }
        }

        public void RequestLocationPermission()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                Log.Debug(TAG, "Location permissions granted");
                EnableGeofencing();
            }
            else
            {
                Log.Debug(TAG, "Requesting location permissions");
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.AccessFineLocation }, REQUEST_LOCATION);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == REQUEST_LOCATION)
            {
                Log.Info(TAG, "Received response for Location permission request.");

                // Check if the only required permission has been granted
                if ((grantResults.Length == 1) && (grantResults[0] == Permission.Granted))
                {
                    Log.Debug(TAG, "Location permission was granted.");
                    EnableGeofencing();
                }
                else
                {
                    Log.Debug(TAG, "Location permission was NOT granted.");
                    ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.AccessFineLocation }, REQUEST_LOCATION);
                }
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }
    }
}

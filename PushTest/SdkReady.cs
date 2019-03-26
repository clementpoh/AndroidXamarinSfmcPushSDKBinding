using System;
using Com.Salesforce.Marketingcloud;
using static Com.Salesforce.Marketingcloud.MarketingCloudSdk;

namespace PushTest
{
    public class SdkReady : Java.Lang.Object, IWhenReadyListener
    {
        public Action<MarketingCloudSdk> OnReady { get; set; }
        public void Ready(MarketingCloudSdk sdk) => OnReady?.Invoke(sdk);
    }
}

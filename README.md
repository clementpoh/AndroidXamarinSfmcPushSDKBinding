## Description

Xamarin binding for the version 6.1.0 of the Salesforce Marketing Cloud Android
Mobile Push SDK and an accompanying app to demonstrate the basic features of
the SDK.

The AndroidPush project is the Xamarin binding, this can just be included as a
reference in another Xamarin project. It's important that the Google Play
Services Maps are included in this project for the SDK to initialise correctly
when Geofence messaging is enabled.

The PushTest project is the demonstration app; it’s a bare-bones app  with
minimal features to keep things simple. A lot of the code is related to getting
the SDK to initialise correctly with Location and Geofence messaging features
successfully.

The SDK is initialised in the OnCreate method of the App in PushApplication.cs.
The application ID, access token, MID, and marketing cloud server URL, are a
corresponding app are values you can get from your instance of Staging in
Marketing Cloud. The Firebase Server Key is obviously from Firebase.

The UpdateSubscriberKey method in MainActivity.cs demonstrates how to identify
a user to Salesforce Marketing Cloud once a user provides their personal
information or e.g. login to the app.

Finally, the app implements the Firebase Notification Service, so in the
OnMessageReceived method, it calls the SDK to distinguish between messages sent
by the Marketing Cloud and other Firebase Cloud Messages, and has the former
handled by the SDK.

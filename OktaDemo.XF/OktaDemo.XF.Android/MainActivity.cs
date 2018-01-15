using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using OktaDemo.XF.Droid.Implementations;
using OktaDemo.XF.Helpers;
using OpenId.AppAuth;
using Org.Json;

namespace OktaDemo.XF.Droid
{
    [Activity(Label = "OktaDemo.XF", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        internal static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            Instance = this;

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            if (intent != null && LoginProvider.Current != null)
            {
                LoginProvider.Current.NotifyOfCallback(intent);
            }
        }

        public static PendingIntent CreatePostAuthorizationIntent(Context context, AuthorizationRequest request, AuthorizationServiceDiscovery discoveryDoc, AuthState authState)
        {
            var intent = new Intent(context, typeof(MainActivity));
            intent.PutExtra(Constants.AuthStateKey, authState.JsonSerializeString());
            if (discoveryDoc != null)
            {
                intent.PutExtra(Constants.AuthServiceDiscoveryKey, discoveryDoc.DocJson.ToString());
            }

            return PendingIntent.GetActivity(context, request.GetHashCode(), intent, 0);
        }
    }
}


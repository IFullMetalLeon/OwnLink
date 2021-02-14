using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Net;
using Android.Support.V4.App;
using OwnLink;
using OwnLink.Android;
using Xamarin.Android;
using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using AndroidApp = Android.App.Application;
using Uri = Android.Net.Uri;

[assembly: Dependency(typeof(OwnLink.Android.AndroidOpenSettings))]
namespace OwnLink.Android
{
    class AndroidOpenSettings : IOpenSettings
    {
        public void GoToSettings()
        {
            Intent intent = new Intent(Settings.ActionApplicationDetailsSettings);
            string package_name = "OwnLink.fsin.com";
            var uri = Uri.FromParts("package", package_name, null);
            intent.SetData(uri);
            MainActivity.instance.StartActivity(intent);
            /*string manufacturer =  DeviceInfo.Manufacturer;

            Intent intent = new Intent();

            if ("Xiaomi".Equals(manufacturer))
            {
                //intent.SetComponent(new ComponentName("com.miui.securitycenter", "com.miui.permcenter.autostart.AutoStartManagementActivity"));
                intent.SetComponent(new ComponentName("com.miui.securitycenter", "com.miui.permcenter.permissions.AppPermissionsEditorActivity"));
            }
            else if ("oppo".Equals(manufacturer))
            {
                intent.SetComponent(new ComponentName("com.coloros.safecenter", "com.coloros.safecenter.permission.startup.StartupAppListActivity"));
            }

            MainActivity.instance.StartActivity(intent);*/
            //StartActivity(intent);
        }

    }
}
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V4.App;
using Firebase.Iid;
using OwnLink;
using OwnLink.Android;
using System;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;

[assembly: Dependency(typeof(OwnLink.Android.AndroidFCMService))]
namespace OwnLink.Android
{
    class AndroidFCMService : IFCMService
    {
        public string GetToken()
        {
            return FirebaseInstanceId.Instance.Token; ;
        }
    }
}
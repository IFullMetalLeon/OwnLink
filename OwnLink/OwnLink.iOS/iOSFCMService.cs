using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(OwnLink.iOS.iOSFCMService))]
namespace OwnLink.iOS
{
    class iOSFCMService : IFCMService
    {
        public string GetToken()
        {
            return "123";
        }
    }
}
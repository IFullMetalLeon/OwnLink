using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;

[assembly: Xamarin.Forms.Dependency(typeof(OwnLink.Android.CallJournal))]
namespace OwnLink.Android
{
    public class CallJournal : ICallJournal
    {
        public string GetLastNumber()
        {
            string rezPhone = "123";
            string querySorter = String.Format("{0} desc ", CallLog.Calls.Date);
            try
            {
                using (var phones = Application.Context.ContentResolver.Query(CallLog.Calls.ContentUri, null, null, null, querySorter))
                {
                    if (phones != null)
                    {
                        while (phones.MoveToNext())
                        {
                            try
                            {
                                string callNumber = phones.GetString(phones.GetColumnIndex(CallLog.Calls.Number));
                                string callDuration = phones.GetString(phones.GetColumnIndex(CallLog.Calls.Duration));
                                long callDate = phones.GetLong(phones.GetColumnIndex(CallLog.Calls.Date));
                                string callName = phones.GetString(phones.GetColumnIndex(CallLog.Calls.CachedName));

                                int callTypeInt = phones.GetInt(phones.GetColumnIndex(CallLog.Calls.Type));
                                string callType = Enum.GetName(typeof(CallType), callTypeInt);

                                DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                DateTime st = start.AddMilliseconds(callDate).ToLocalTime();

                                DateTime end = DateTime.Now;
                                TimeSpan rez = end - st;
                                if (rez.TotalSeconds <= 10)
                                    rezPhone = callNumber;
                                break;

                            }
                            catch (Exception ex)
                            {
                                break;
                            }
                        }
                        phones.Close();
                    }
                    
                }
            }
            catch(Exception ex) { }

            return rezPhone;
        }
    }
}
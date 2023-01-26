using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Xamarin.Forms;
using System.IO;
using System.Linq;

namespace OwnLink.ViewModel
{
    public static class HttpControler
    {

        public static string mainUrl = "https://ic.pismo-fsin.ru/";

        public static async void register(string _phone,string _code,string _deviceId,string _deviceInfo)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient http = new HttpClient(clientHandler);
            http.BaseAddress = new Uri(mainUrl + "register");
            var values = new Dictionary<string, string>();
            values.Add("phone", _phone);
            if (_code != "")
                values.Add("code", _code);
            if (_deviceId != "")
            {
                values.Add("device_id", _deviceId);
                values.Add("device_type", "android");
                values.Add("device_info", _deviceInfo);
            }

            var response = await http.PostAsync(http.BaseAddress, new FormUrlEncodedContent(values));

            string content = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
                MessagingCenter.Send<string, string>("HttpControler", "Register", content);
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<string, string>("HttpControler", "Error", ex.Message);
            }

        }

        public static async void feedbackSend(string _email,string _text,string _token,string _file)
        {

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient http = new HttpClient(clientHandler);
            http.BaseAddress = new Uri(mainUrl + "feedback");



            var multiForm = new MultipartFormDataContent();

            
            multiForm.Add(new StringContent(_email), "email");
            multiForm.Add(new StringContent(_text), "text");
            multiForm.Add(new StringContent(_token), "token");

            try
            {
                FileStream fs = File.OpenRead(_file);
                multiForm.Add(new StreamContent(fs), "file", Path.GetFileName(_file));
            }
            catch(Exception ex) { }



            var response = await http.PostAsync(http.BaseAddress, multiForm);

            string content = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
                MessagingCenter.Send<string, string>("HttpControler", "Feedback", content);
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<string, string>("HttpControler", "Error", ex.Message);
            }
        }

        public static async void history(string _phone, string _deviceId)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient http = new HttpClient(clientHandler);
            http.BaseAddress = new Uri(mainUrl + "history");
            var values = new Dictionary<string, string>();
            values.Add("phone", _phone);
            values.Add("token", _deviceId);

            var response = await http.PostAsync(http.BaseAddress, new FormUrlEncodedContent(values));

            string content = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
                MessagingCenter.Send<string, string>("HttpControler", "History", content);
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<string, string>("HttpControler", "Error", ex.Message);
            }

        }

        public static async void GetServerVersion()
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                HttpClient http = new HttpClient(clientHandler);
                http.Timeout = TimeSpan.FromMinutes(5);

                string addr = "http://ic.pismo-fsin.ru/upgrade/version";

                http.BaseAddress = new Uri(addr);
                http.DefaultRequestHeaders.Accept.Clear();

                var response = await http.GetAsync(http.BaseAddress);

                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                MessagingCenter.Send<string, string>("HttpControler", "GetServerVersion", content);
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<string, string>("HttpControler", "Error", ex.Message);
            }
        }

        public static async void FCMTokenSend(string _phone, string _fcmId,string _deviceId)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient http = new HttpClient(clientHandler);
            http.BaseAddress = new Uri(mainUrl + "firebase");
            var values = new Dictionary<string, string>();
            values.Add("phone", _phone);
            values.Add("token", _deviceId);
            values.Add("fcm_id", _fcmId);

            var response = await http.PostAsync(http.BaseAddress, new FormUrlEncodedContent(values));

            string content = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
                MessagingCenter.Send<string, string>("HttpControler", "FCMTokenSend", content);
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<string, string>("HttpControler", "Error", ex.Message);
            }
        }

        public static async void ErrorLogSend(string _phone, string _device_info, string _deviceId,string _text)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient http = new HttpClient(clientHandler);
            http.BaseAddress = new Uri(mainUrl + "crash_report");
            var values = new Dictionary<string, string>();
            values.Add("phone", _phone);
            values.Add("device_id", _deviceId);
            values.Add("device_info", _deviceId);
            values.Add("text", _text);

            var response = await http.PostAsync(http.BaseAddress, new FormUrlEncodedContent(values));

            string content = await response.Content.ReadAsStringAsync();
            
        }

    }
}

﻿using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;
using System.Net;
using System.Net.Sockets;
using Android.Graphics;
using System.Threading.Tasks;

namespace ITScheduleViewer
{
    [Activity(Label = "IT Schedule", MainLauncher = true, Theme = "@android:style/Theme.NoTitleBar")]
    public class MainActivity : Activity
    {
        static ProgressBar progressBar;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            // Set our view from the "main" layout resource
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.Main);

            var webView = FindViewById<WebView>(Resource.Id.webView);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            // Use subclassed WebViewClient to intercept hybrid native calls
            webView.SetWebViewClient(new HybridWebViewClient());
            progressBar.Visibility = ViewStates.Visible;
   
            var url = "http://121.96.88.16:5132/MedexWebApps/ITSched";

            if (Ping("10.23.234.9", 5132))
            {
                url = "http://10.23.234.9:5132/MedexWebApps/ITSched";
            }
            webView.LoadUrl(url);
        }
        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();
            Process.KillProcess(Process.MyPid());
        }
        private bool Ping(string host, int port)
        {
            try
            {
                var client = new TcpClient();
                var result = client.BeginConnect(host, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                if (!success)
                {
                    return false;
                }
                client.EndConnect(result);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private class HybridWebViewClient : WebViewClient
        {
            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                return false;
            }
            public async override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);
                var task = new Task(() => {
                    while (view.Progress != 0)
                    {
                        progressBar.Progress = view.Progress;
                    };
                });
                task.Start();
                await task;
            }
            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);
                progressBar.Visibility = ViewStates.Gone;
            }
        }
    }
}


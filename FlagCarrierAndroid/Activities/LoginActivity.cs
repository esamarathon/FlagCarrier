using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Views;
using Android.OS;
using Android.App;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;

using FlagCarrierBase;

namespace FlagCarrierAndroid.Activities
{
    [Activity(Label = "@string/login_title")]
    [IntentFilter(new[] { "android.nfc.action.NDEF_DISCOVERED" }, Categories = new[] { "android.intent.category.DEFAULT" }, DataMimeType = NdefHandler.FLAGCARRIER_MIME_TYPE)]
    public class LoginActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_login);
        }
    }
}
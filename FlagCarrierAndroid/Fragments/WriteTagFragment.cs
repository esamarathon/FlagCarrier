using System;
using System.Collections.Generic;
using System.Text;

using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using FlagCarrierAndroid.Helpers;

namespace FlagCarrierAndroid.Fragments
{
    public class WriteTagFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_write_tag, container, false);
        }
    }
}
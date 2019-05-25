using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Nfc;
using Android.Nfc.Tech;

using FlagCarrierBase;

namespace FlagCarrierAndroid.Helpers
{
    public static class TagHelper
    {
        public static void SetExtraSignDataFromTag(this NdefHandler handler, Tag tag)
        {
            if (tag == null)
            {
                handler.ClearExtraSignData();
                return;
            }

            byte[] uid = tag.GetId();
            string[] techs = tag.GetTechList();

            byte[] nuid = new byte[uid.Length + 1];
            Buffer.BlockCopy(uid, 0, nuid, 0, uid.Length);

            if (techs.Contains(Java.Lang.Class.FromType(typeof(MifareUltralight)).CanonicalName))
            {
                nuid[uid.Length] = 0xAA;
            }
            else if (techs.Contains(Java.Lang.Class.FromType(typeof(MifareClassic)).CanonicalName))
            {
                nuid[uid.Length] = 0xBB;
            }
            else
            {
                nuid = uid;
            }

            handler.SetExtraSignData(nuid);
        }
    }
}
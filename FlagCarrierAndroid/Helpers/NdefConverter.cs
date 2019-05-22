using NdefLibrary.Ndef;
using System.Collections.Generic;
using ANdefMessage = Android.Nfc.NdefMessage;
using ANdefRecord = Android.Nfc.NdefRecord;


namespace FlagCarrierAndroid.Helpers
{
    public static class NdefConverter
    {
        public static ANdefMessage ToAndroid(NdefMessage message)
        {
            if (message == null)
                return null;

            return new ANdefMessage(message.ToByteArray());
        }

        public static NdefMessage FromAndroid(ANdefMessage message)
        {
            if (message == null)
                return null;

            return NdefMessage.FromByteArray(message.ToByteArray());
        }
    }
}
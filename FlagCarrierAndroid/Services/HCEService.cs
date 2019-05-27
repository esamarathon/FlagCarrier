using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.OS;
using Android.Nfc.CardEmulators;
using Android.Runtime;
using Android.Widget;
using Android.Content;

using FlagCarrierBase;
using FlagCarrierAndroid.Activities;
using FlagCarrierAndroid.Helpers;

namespace FlagCarrierAndroid.Services
{
    [Service(Exported = true, Permission = Android.Manifest.Permission.BindNfcService)]
    [IntentFilter(new[] { ServiceInterface })]
    [MetaData(ServiceMetaData, Resource = "@xml/apduservice")]
    public class HCEService : HostApduService
    {
        private static readonly byte[] STATUS_SUCCESS = new byte[] { 0x90, 0x00 };
        private static readonly byte[] STATUS_FAILED = new byte[] { 0x6F, 0x00 };
        private static readonly byte[] CLA_NOT_SUPPORTED = new byte[] { 0x6E, 0x00 };
        private static readonly byte[] INS_NOT_SUPPORTED = new byte[] { 0x6D, 0x00 };
        private static readonly byte[] FILE_NOT_FOUND = new byte[] { 0x6A, 0x82 };
        private static readonly byte[] WRONG_PARAMETERS = new byte[] { 0x6B, 0x00 };

        private const byte DEFAULT_CLA = 0x00;

        private const byte SELECT_INS = 0xA4;
        private const byte UPDATEBINARY_INS = 0xD6;
        private const byte READBINARY_INS = 0xB0;

        private static readonly byte[] FLAGCARRIER_AID
                = new byte[] { 0xf0, 0x5a, 0x25, 0x58, 0x83, 0x6e, 0x09, 0x66, 0xae, 0xd5, 0x27, 0xce };

        private static Dictionary<string, string> dataToPublish = null;

        public static void Publish(Dictionary<string, string> data)
        {
            dataToPublish = data;
        }

        private readonly NdefHandler ndefHandler = new NdefHandler();

        private byte[] ndefData = null;
        private int highestReadEnd = 0;

        public override byte[] ProcessCommandApdu(byte[] apdu, Bundle extras)
        {
            if (apdu == null || apdu.Length < 5)
                return STATUS_FAILED;

            if (apdu[0] != DEFAULT_CLA)
                return CLA_NOT_SUPPORTED;

            switch (apdu[1])
            {
                case SELECT_INS:
                    return ProcessSelect(apdu);
                case UPDATEBINARY_INS:
                    return ProcessUpdate(apdu);
                case READBINARY_INS:
                    return ProcessRead(apdu);
                default:
                    return INS_NOT_SUPPORTED;
            }
        }

        private byte[] ProcessSelect(byte[] apdu)
        {
            if (apdu[2] != 0x04 || apdu[3] != 0x00)
                return STATUS_FAILED;

            if (apdu[4] != FLAGCARRIER_AID.Length)
                return FILE_NOT_FOUND;

            for (int i = 0; i < FLAGCARRIER_AID.Length; ++i)
                if (apdu[i + 5] != FLAGCARRIER_AID[i])
                    return FILE_NOT_FOUND;

            highestReadEnd = 0;

            return STATUS_SUCCESS;
        }

        private byte[] ProcessUpdate(byte[] apdu)
        {
            int address = ((apdu[2] & 0xFF) << 8) | (apdu[3] & 0xFF);
            if (address < 0 || address >= 1024)
                return WRONG_PARAMETERS;

            if (address != 0)
                return WRONG_PARAMETERS;

            int length = apdu[4] & 0xFF;
            if (apdu.Length < length + 5)
                return STATUS_FAILED;

            if (dataToPublish == null)
                return FILE_NOT_FOUND;

            byte[] challenge = new byte[length];
            Buffer.BlockCopy(apdu, 5, challenge, 0, length);

            try
            {
                ndefHandler.SetKeys(AppSettings.Global.PubKey, AppSettings.Global.PrivKey);
                ndefHandler.SetExtraSignData(challenge);
                ndefHandler.KeepEmptyFields = true;

                ndefData = ndefHandler.GenerateRawNdefMessage(dataToPublish);

                return STATUS_SUCCESS;
            }
            catch (Exception e)
            {
                ShowToast("Failed preparing HCE data: " + e.Message);
                return STATUS_FAILED;
            }
            finally
            {
                ndefHandler.ClearKeys();
                ndefHandler.ClearExtraSignData();
            }
        }

        private byte[] ProcessRead(byte[] apdu)
        {
            int offset = ((apdu[2] & 0xFF) << 8) | (apdu[3] & 0xFF);
            if (offset < 0 || offset >= 0x8000)
                return WRONG_PARAMETERS;

            int length = apdu[4] & 0xFF;
            if (length < 0 || length > 253)
                length = 253;

            if (ndefData == null)
                return FILE_NOT_FOUND;

            if (offset >= ndefData.Length)
                return STATUS_SUCCESS;

            int end = offset + length;
            if (end > ndefData.Length)
                end = ndefData.Length;

            length = end - offset;

            if (end > highestReadEnd)
                highestReadEnd = end;

            byte[] res = new byte[length + STATUS_SUCCESS.Length];
            Buffer.BlockCopy(ndefData, offset, res, 0, length);
            Buffer.BlockCopy(STATUS_SUCCESS, 0, res, length, STATUS_SUCCESS.Length);

            return res;
        }

        public override void OnDeactivated([GeneratedEnum] DeactivationReason reason)
        {
            if (dataToPublish != null)
            {
                if (ndefData == null || highestReadEnd < ndefData.Length)
                    ShowToast("Sending Flag Carrier HCE data was interrupted.");
                else
                    ShowToast("Transferred " + ndefData.Length + " bytes via Flag Carrier HCE Service.");
            }

            ndefData = null;
        }

        private void ShowToast(string txt)
        {
            Toast.MakeText(this, txt, ToastLength.Long).Show();
        }
    }
}

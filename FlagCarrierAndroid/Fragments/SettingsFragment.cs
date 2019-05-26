using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using FlagCarrierAndroid.Helpers;
using FlagCarrierBase;

namespace FlagCarrierAndroid.Fragments
{
    public class SettingsFragment : BaseFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        EditText targetUrlSettingInput = null;
        EditText posAvailInput = null;
        EditText deviceIdInput = null;
        EditText groupIdInput = null;
        Switch kioskModeSwitch = null;
        EditText pubKeyInput = null;
        EditText privKeyInput = null;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_settings, container, false);

            view.FindViewById<Button>(Resource.Id.genKeypairButton).Click += (s, e) => GenKeypairButton_Click();

            view.FindViewById<Button>(Resource.Id.applySettingsButton).Click += (s, e) => ApplySettingsButton_Click();
            view.FindViewById<Button>(Resource.Id.resetSettingsButton).Click += (s, e) => ResetSettingsButton_Click();

            targetUrlSettingInput = view.FindViewById<EditText>(Resource.Id.targetUrlSettingInput);
            posAvailInput = view.FindViewById<EditText>(Resource.Id.posAvailInput);
            deviceIdInput = view.FindViewById<EditText>(Resource.Id.deviceIdInput);
            groupIdInput = view.FindViewById<EditText>(Resource.Id.groupIdInput);
            kioskModeSwitch = view.FindViewById<Switch>(Resource.Id.kioskModeSwitch);
            pubKeyInput = view.FindViewById<EditText>(Resource.Id.pubKeyInput);
            privKeyInput = view.FindViewById<EditText>(Resource.Id.privKeyInput);

            ResetSettingsButton_Click();

            return view;
        }

        private void GenKeypairButton_Click()
        {
            KeyPair pair = CryptoHandler.GenKeyPair();

            pubKeyInput.Text = Convert.ToBase64String(pair.PublicKey);
            privKeyInput.Text = Convert.ToBase64String(pair.PrivateKey);
        }

        const string PrivKeyHiddenBlurb = "***hidden***";

        private void ResetSettingsButton_Click()
        {
            targetUrlSettingInput.Text = AppSettings.Global.TargetUrl;
            posAvailInput.Text = AppSettings.Global.Positions;
            deviceIdInput.Text = AppSettings.Global.DeviceId;
            groupIdInput.Text = AppSettings.Global.GroupId;
            kioskModeSwitch.Checked = AppSettings.Global.KioskMode;

            byte[] pubKey = AppSettings.Global.PubKey;
            if (pubKey != null)
                pubKeyInput.Text = Convert.ToBase64String(pubKey);
            else
                pubKeyInput.Text = "";

            byte[] privKey = AppSettings.Global.PrivKey;
            if (privKey != null)
                privKeyInput.Text = PrivKeyHiddenBlurb;
            else
                privKeyInput.Text = "";
        }

        private void ApplySettingsButton_Click()
        {
            byte[] pubKey = null;
            byte[] privKey = null;
            bool privKeyChanged = privKeyInput.Text != PrivKeyHiddenBlurb;

            try
            {
                if (pubKeyInput.Text.Trim() != "")
                {
                    pubKey = Convert.FromBase64String(pubKeyInput.Text);
                    if (!CryptoHandler.IsKeyValid(pubKey))
                    {
                        PopUpHelper.Toast("Invalid Public Key");
                        return;
                    }
                }
            }
            catch (FormatException)
            {
                PopUpHelper.Toast("Invalid Public Key Format");
                return;
            }

            try
            {
                if (privKeyChanged && privKeyInput.Text.Trim() != "")
                {
                    privKey = Convert.FromBase64String(privKeyInput.Text);
                    if (!CryptoHandler.IsKeyValid(privKey))
                    {
                        PopUpHelper.Toast("Invalid Private Key");
                        return;
                    }
                }
            }
            catch (FormatException)
            {
                PopUpHelper.Toast("Invalid Private Key Format");
                return;
            }

            AppSettings.Global.TargetUrl = targetUrlSettingInput.Text;
            AppSettings.Global.Positions = posAvailInput.Text;
            AppSettings.Global.DeviceId = deviceIdInput.Text;
            AppSettings.Global.GroupId = groupIdInput.Text;
            AppSettings.Global.KioskMode = kioskModeSwitch.Checked;
            AppSettings.Global.PubKey = pubKey;

            if (privKeyChanged)
                AppSettings.Global.PrivKey = privKey;

            PopUpHelper.Toast("Settings Saved");
        }
    }
}
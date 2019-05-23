using System;
using System.Text;
using Android.OS;
using Android.Runtime;
using Android.Security.Keystore;
using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;

namespace FlagCarrierAndroid.Helpers
{
    public class AndroidKeyStore
    {
        const string alias = "de.oromit.flagcarrier.aks_secret";
        const string androidKeyStore = "AndroidKeyStore";
        const string cipherTransformation = "AES/GCM/NoPadding";
        const int initializationVectorLen = 12;

        internal AndroidKeyStore()
        {
            keyStore = KeyStore.GetInstance(androidKeyStore);
            keyStore.Load(null);
        }

        private KeyStore keyStore;

        ISecretKey GetKey()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                return null;

            IKey existingKey = keyStore.GetKey(alias, null);

            if (existingKey != null)
            {
                ISecretKey existingSecretKey = existingKey.JavaCast<ISecretKey>();
                return existingSecretKey;
            }

            var keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, androidKeyStore);
            var builder = new KeyGenParameterSpec.Builder(alias, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                .SetBlockModes(KeyProperties.BlockModeGcm)
                .SetEncryptionPaddings(KeyProperties.EncryptionPaddingNone)
                .SetRandomizedEncryptionRequired(false);

            keyGenerator.Init(builder.Build());

            return keyGenerator.GenerateKey();
        }

        public byte[] Encrypt(byte[] data)
        {
            ISecretKey key = GetKey();
            if (key == null || data == null)
                return null;

            byte[] iv = new byte[initializationVectorLen];

            SecureRandom sr = new SecureRandom();
            sr.NextBytes(iv);

            Cipher cipher;

            try
            {
                cipher = Cipher.GetInstance(cipherTransformation);
                cipher.Init(CipherMode.EncryptMode, key, new GCMParameterSpec(128, iv));
            }
            catch (InvalidAlgorithmParameterException)
            {
                cipher = Cipher.GetInstance(cipherTransformation);
                cipher.Init(CipherMode.EncryptMode, key, new IvParameterSpec(iv));
            }

            byte[] encryptedBytes = cipher.DoFinal(data);

            byte[] r = new byte[iv.Length + encryptedBytes.Length];
            Buffer.BlockCopy(iv, 0, r, 0, iv.Length);
            Buffer.BlockCopy(encryptedBytes, 0, r, iv.Length, encryptedBytes.Length);

            return r;
        }

        public byte[] Decrypt(byte[] data)
        {
            if (data == null || data.Length < initializationVectorLen)
                return null;

            ISecretKey key = GetKey();
            if (key == null)
                return null;

            byte[] iv = new byte[initializationVectorLen];
            Buffer.BlockCopy(data, 0, iv, 0, initializationVectorLen);

            Cipher cipher;

            try
            {
                cipher = Cipher.GetInstance(cipherTransformation);
                cipher.Init(CipherMode.DecryptMode, key, new GCMParameterSpec(128, iv));
            }
            catch (InvalidAlgorithmParameterException)
            {
                cipher = Cipher.GetInstance(cipherTransformation);
                cipher.Init(CipherMode.DecryptMode, key, new IvParameterSpec(iv));
            }

            return cipher.DoFinal(data, initializationVectorLen, data.Length - initializationVectorLen);
        }
    }
}
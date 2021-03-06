//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System.Threading.Tasks;
using PCSC;
using System;

namespace PcscSdk.MifareStandard
{
    public static class CRC8
    {
        public static byte Calc(byte[] data)
        {
            byte res = 0xC7;

            foreach (byte v in data)
            {
                res ^= v;

                for (byte i = 0; i < 8; i++)
                {
                    if ((res & 0x80) != 0)
                    {
                        res = (byte)(((res << 1) ^ 0x1D) & 0xFF);
                    }
                    else
                    {
                        res <<= 1;
                    }
                }
            }

            return res;
        }
    }

    public class DefaultKeys
    {
        public static readonly byte[] FactoryDefault = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
    }
    /// <summary>
    /// Access handler class for Mifare Standard/Classic based ICC
    /// commands
    /// </summary>
    public class AccessHandler
    {
        /// <summary>
        /// connection object to smart card
        /// </summary>
        public ICardReader CardReader { set; get; }
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="ScConnection">
        /// connection object to a Mifare Standard ICC
        /// </param>
        public AccessHandler(ICardReader cardReader)
        {
            this.CardReader = cardReader;
        }
        /// <summary>
        /// Wrapper method to read 16 bytes
        /// </summary>
        /// <param name="pageAddress">
        /// start page to read
        /// </param>
        /// <returns>
        /// byte array of 16 bytes
        /// </returns>
        public void LoadKey(byte[] mifareKey, byte keySlotNumber = 0)
        {
            var apduRes = CardReader.Transceive(new MifareStandard.LoadKey(mifareKey, keySlotNumber));

            if (!apduRes.Succeeded)
            {
                throw new ApduFailedException(apduRes, "Failure loading key for MIFARE Standard card, " + apduRes.ToString());
            }

            return;
        }
        /// <summary>
        /// Reads 16 bytes
        /// </summary>
        /// <param name="blockNumber">
        /// Block number to read
        /// </param>
        /// <param name="keyType">
        /// Choose MIFARE key A or B
        /// </param>
        /// <param name="keySlotNumber">
        /// Slot where key has previously been stored with a previous call to Load Keys
        /// </param>
        /// <returns>
        /// byte array of 16 bytes
        /// </returns>
        public byte[] Read(ushort blockNumber, GeneralAuthenticate.GeneralAuthenticateKeyType keyType, byte keySlotNumber = 0)
        {
            var genAuthRes = CardReader.Transceive(new MifareStandard.GeneralAuthenticate(blockNumber, keySlotNumber, keyType));
            if (!genAuthRes.Succeeded)
            {
                throw new ApduFailedException(genAuthRes, "Failure authenticating to MIFARE Standard card, " + genAuthRes.ToString());
            }

            var readRes = CardReader.Transceive(new MifareStandard.Read(blockNumber));
            if (!readRes.Succeeded)
            {
                throw new ApduFailedException(readRes, "Failure reading MIFARE Standard card, " + readRes.ToString());
            }

            return readRes.ResponseData;
        }
        /// <summary>
        /// Wrapper method write 16 bytes at the pageAddress
        /// </param name="blockNumber">
        /// Block number
        /// </param>
        /// <param name="keyType">
        /// Choose MIFARE key A or B
        /// </param>
        /// <param name="keySlotNumber">
        /// Slot where key has previously been stored with a previous call to Load Keys
        /// </param>
        /// byte array of the data to write
        /// </returns>
        public void Write(byte blockNumber, byte[] data, GeneralAuthenticate.GeneralAuthenticateKeyType keyType, byte keySlotNumber = 0)
        {
            if (data.Length != 16)
            {
                throw new NotSupportedException();
            }

            var genAuthRes = CardReader.Transceive(new MifareStandard.GeneralAuthenticate(blockNumber, keySlotNumber, keyType));
            if (!genAuthRes.Succeeded)
            {
                throw new ApduFailedException(genAuthRes, "Failure authenticating to MIFARE Standard card, " + genAuthRes.ToString());
            }

            var apduRes = CardReader.Transceive(new MifareStandard.Write(blockNumber, ref data));
            if (!apduRes.Succeeded)
            {
                throw new ApduFailedException(apduRes, "Failure writing MIFARE Standard card, " + apduRes.ToString());
            }
        }
        /// <summary>
        /// Wrapper method to perform transparent transceive data to the Mifare Standard card
        /// </summary>
        /// <param name="commandData">
        /// The command to send to the Mifare Standard card
        /// </param>
        /// <returns>
        /// byte array of the read data
        /// </returns>
        public byte[] TransparentExchange(byte[] commandData)
        {
            byte[] responseData = CardReader.TransparentExchange(commandData);

            return responseData;
        }
        /// <summary>
        /// Wrapper method get the Mifare Standard ICC UID
        /// </summary>
        /// <returns>
        /// byte array UID
        /// </returns>
        public byte[] GetUid()
        {
            var apduRes = CardReader.Transceive(new MifareStandard.GetUid());
            if (!apduRes.Succeeded)
            {
                throw new ApduFailedException(apduRes, "Failure getting UID of MIFARE Standard card, " + apduRes.ToString());
            }

            return apduRes.ResponseData;
        }
    }
}

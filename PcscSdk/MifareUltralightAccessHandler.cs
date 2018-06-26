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

namespace PcscSdk.MifareUltralight
{
	/// <summary>
	/// Access handler class for MifareUL based ICC. It provides wrappers for different MifareUL 
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
		/// connection object to a MifareUL ICC
		/// </param>
		public AccessHandler(ICardReader cardReader)
		{
			this.CardReader = cardReader;
		}
		/// <summary>
		/// Wrapper method to read 16 bytes (4 pages) starting at pageAddress
		/// </summary>
		/// <param name="pageAddress">
		/// start page to read
		/// </param>
		/// <returns>
		/// byte array of 16 bytes
		/// </returns>
		public byte[] Read(byte pageAddress)
		{
			var apduRes = CardReader.Transceive(new MifareUltralight.Read(pageAddress));

			if (!apduRes.Succeeded)
			{
				throw new ApduFailedException(apduRes, "Failure reading MIFARE Ultralight card, " + apduRes.ToString());
			}

			return apduRes.ResponseData;
		}
		/// <summary>
		/// Wrapper method write 4 bytes at the pageAddress
		/// </param name="pageAddress">
		/// page address to write
		/// </param>
		/// byte array of the data to write
		/// </returns>
		public void Write(byte pageAddress, byte[] data)
		{
			if (data.Length != 4)
			{
				throw new NotSupportedException();
			}

			var apduRes = CardReader.Transceive(new MifareUltralight.Write(pageAddress, ref data));

			if (!apduRes.Succeeded)
			{
				throw new ApduFailedException(apduRes, "Failure writing MIFARE Ultralight card, " + apduRes.ToString());
			}
		}
		/// <summary>
		/// Wrapper method to perform transparent transceive data to the MifareUL card
		/// </summary>
		/// <param name="commandData">
		/// The command to send to the MifareUL card
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
		/// Wrapper method get the MifareUL ICC UID
		/// </summary>
		/// <returns>
		/// byte array UID
		/// </returns>
		public byte[] GetUid()
		{
			var apduRes = CardReader.Transceive(new MifareUltralight.GetUid());

			if (!apduRes.Succeeded)
			{
				throw new ApduFailedException(apduRes, "Failure getting UID of MIFARE Ultralight card, " + apduRes.ToString());
			}

			return apduRes.ResponseData;
		}
	}
}

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

using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using PCSC;

namespace PcscSdk.Common
{
	/// <summary>
	/// Class used to detect the type of the ICC card detected. It accept a connection object
	/// and gets the ATR from the ICC. After the ATR is parsed, the ICC Detection class inspects
	/// the historical bytes in order to detect the ICC type as specified by PCSC specification.
	/// </summary>
	public class IccDetection
	{
		/// <summary>
		/// PCSC device type
		/// </summary>
		public DeviceClass PcscDeviceClass { set; get; }
		/// <summary>
		/// PCSC card name provided in the nn short int
		/// </summary>
		public PcscSdk.CardName PcscCardName { set; get; }
		/// <summary>
		/// ATR byte array
		/// </summary>
		public byte[] Atr { set; get; }
		/// <summary>
		/// ATR info holds information about the interface character along other info
		/// </summary>
		public AtrInfo AtrInformation { set; get; }
		/// <summary>
		/// smard card object passed in the constructor
		/// </summary>
		private ICardReader cardReader { set; get; }
		/// <summary>
		/// class constructor.
		/// </summary>
		/// <param name="card">
		/// smart card object
		/// </param>
		/// <param name="connection">
		/// connection object to the smard card
		/// </param>
		public IccDetection(ICardReader card)
		{
			cardReader = card;
			PcscDeviceClass = DeviceClass.Unknown;
			PcscCardName = PcscSdk.CardName.Unknown;
		}
		/// <summary>
		/// Detects the ICC type by parsing, and analyzing the ATR
		/// </summary>
		/// <returns>
		/// none
		/// </returns>
		public void DetectCardType()
		{
			ReaderStatus status = cardReader.GetStatus();
			Atr = status.GetAtr();

			Debug.WriteLine("Status: " + status.State.ToString() + " ATR [" + Atr.Length + "] = " + BitConverter.ToString(Atr));

			AtrInformation = AtrParser.Parse(Atr);

			if (AtrInformation != null && AtrInformation.HistoricalBytes.Length > 0)
			{
				DetectCard();
			}
		}
		/// <summary>
		/// Internal method that analyzes the ATR Historical Bytes,
		/// it populate the object with info about the ICC
		/// </summary>
		private void DetectCard()
		{
			if (AtrInformation.HistoricalBytes.Length > 1)
			{
				byte categoryIndicator;

				using (MemoryStream mem = new MemoryStream(AtrInformation.HistoricalBytes))
				using (BinaryReader reader = new BinaryReader(mem))
				{
					categoryIndicator = reader.ReadByte();

					if (categoryIndicator == (byte)CategoryIndicator.StatusInfoPresentInTlv)
					{
						while (reader.BaseStream.Position < reader.BaseStream.Length)
						{
							const byte appIdPresenceIndTag = 0x4F;
							const byte appIdPresenceIndTagLen = 0x0C;

							var tagValue = reader.ReadByte();
							var tagLength = reader.ReadByte();

							if (tagValue == appIdPresenceIndTag && tagLength == appIdPresenceIndTagLen)
							{
								byte[] pcscRid = { 0xA0, 0x00, 0x00, 0x03, 0x06 };
								byte[] pcscRidRead = reader.ReadBytes(pcscRid.Length);

								if (pcscRid.SequenceEqual(pcscRidRead))
								{
									byte storageStandard = reader.ReadByte();

									byte[] cardNameData = reader.ReadBytes(2);
									if (BitConverter.IsLittleEndian)
										Array.Reverse(cardNameData);

									PcscCardName = (PcscSdk.CardName)BitConverter.ToUInt16(cardNameData, 0);
									PcscDeviceClass = DeviceClass.StorageClass;
								}

								reader.ReadBytes(4); // RFU bytes
							}
							else
							{
								reader.ReadBytes(tagLength);
							}
						}
					}
				}
			}
			else
			{
				// Compare with Mifare DesFire card ATR
				byte[] desfireAtr = { 0x3B, 0x81, 0x80, 0x01, 0x80, 0x80 };
				
				if (Atr.SequenceEqual(desfireAtr))
				{
					PcscDeviceClass = DeviceClass.MifareDesfire;
				}
			}
		}
		/// <summary>
		/// Helper enum to hold various constants
		/// </summary>
		enum CategoryIndicator : byte
		{
			StatusInfoPresentAtEnd = 0x00,
			StatusInfoPresentInTlv = 0x80
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using Pcsc;
using Pcsc.Common;
using Windows.Devices.Enumeration;
using NdefLibrary.Ndef;
using MifareUltralight;

namespace FlagCarrierWin
{
	public class NfcHandlerException : Exception
	{
		public NfcHandlerException()
		{
		}

		public NfcHandlerException(string message)
			: base(message)
		{
		}

		public NfcHandlerException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	class NfcHandler : IDisposable
	{
		public static async Task<List<DeviceInformation>> GetAvailableReaders()
		{
			if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.SmartCards.SmartCardConnection"))
				throw new NfcHandlerException("NFC reading is not supported on this platform");

			string query = "System.Devices.InterfaceClassGuid:=\"{DEEBE6AD-9E01-47E2-A3B2-A66AA2C036C9}\"";
			string nfcQuery = query + " AND System.Devices.SmartCards.ReaderKind:=3";
			DeviceInformationCollection nfc = await DeviceInformation.FindAllAsync(nfcQuery);
			DeviceInformationCollection any = await DeviceInformation.FindAllAsync(query);

			List<DeviceInformation> infos = nfc.ToList();
			infos.AddRange(any);

			return infos
				.GroupBy(i => i.Id)
				.Select(g => g.First())
				.OrderByDescending(i => i.IsDefault)
				.ToList();
		}

		public static async Task<NfcHandler> GetFromDevInfoAsync(DeviceInformation info)
		{
			if (!info.IsEnabled)
				throw new NfcHandlerException("Reader " + info.Name + " is disabled");

			return await GetFromDevIdAsync(info.Id);
		}

		public static async Task<NfcHandler> GetFromDevIdAsync(string devid)
		{
			SmartCardReader reader = await SmartCardReader.FromIdAsync(devid);
			return new NfcHandler(reader);
		}

		private NfcHandler(SmartCardReader reader)
		{
			this.reader = reader;

			reader.CardAdded += Reader_CardAdded;
			reader.CardRemoved += Reader_CardRemoved;
		}

		~NfcHandler()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (reader != null)
			{
				reader.CardAdded -= Reader_CardAdded;
				reader.CardRemoved -= Reader_CardRemoved;
				reader = null;
			}
		}

		public event Action<string> StatusMessage;
		public event Action<string> ErrorMessage;
		public event Action CardAdded;
		public event Action<NdefMessage> ReceiveNdefMessage;

		private SmartCardReader reader;
		private byte[] ndefDataToWrite;

		public void WriteNdefMessage(NdefMessage msg)
		{
			ndefDataToWrite = msg.ToByteArray();
		}

		private void Reader_CardRemoved(SmartCardReader sender, CardRemovedEventArgs args)
		{
			StatusMessage?.Invoke("Tag removed");
		}

		private async void Reader_CardAdded(SmartCardReader sender, CardAddedEventArgs args)
		{
			CardAdded?.Invoke();
			StatusMessage?.Invoke("Tag detected");

			try
			{
				await HandleSmartCard(args.SmartCard);
			} catch(Exception e)
			{
				ErrorMessage?.Invoke("Error handling tag:\r\n" + e.ToString() + "\r\n");
			}
		}

		private async Task HandleSmartCard(SmartCard card)
		{
			using (SmartCardConnection con = await card.ConnectAsync())
			{
				StatusMessage?.Invoke("Connected to tag");

				IccDetection cardIdent = new IccDetection(card, con);
				await cardIdent.DetectCardTypeAync();

				StatusMessage?.Invoke("Device class: " + cardIdent.PcscDeviceClass.ToString());
				StatusMessage?.Invoke("Card name: " + cardIdent.PcscCardName.ToString());
				StatusMessage?.Invoke("ATR: " + BitConverter.ToString(cardIdent.Atr));

				if (cardIdent.PcscDeviceClass == Pcsc.Common.DeviceClass.StorageClass &&
					(cardIdent.PcscCardName == CardName.MifareUltralight
					|| cardIdent.PcscCardName == CardName.MifareUltralightC
					|| cardIdent.PcscCardName == CardName.MifareUltralightEV1))
				{
					await HandleMifareUL(con);
				}
				else
				{
					throw new NfcHandlerException("Unsupported tag type");
				}
			}
		}

		private async Task HandleMifareUL(SmartCardConnection con)
		{
			var mifare = new MifareUltralight.AccessHandler(con);

			StatusMessage?.Invoke("Handling as Mifare Ultralight");

			byte[] uid = await mifare.GetUidAsync();
			StatusMessage?.Invoke("UID: " + BitConverter.ToString(uid));

			byte[] infoData = await mifare.ReadAsync(0);
			StatusMessage?.Invoke("CC: " + BitConverter.ToString(infoData.Skip(12).ToArray()));

			byte identMagic = infoData[12];
			byte identVersion = infoData[13];
			int identCapacity = infoData[14] * 8;
			int major = identVersion >> 4;
			int minor = identVersion & 0x0F;

			if (identMagic != 0xE1 || identVersion < 0x10)
				throw new NfcHandlerException("Tag format is unsupported");

			StatusMessage?.Invoke("Found Type 2 Tag version " + major + "." + minor + " with " + identCapacity + " bytes capacity.");

			if(ndefDataToWrite != null)
			{
				await WriteNdefToMifareUL(mifare, ndefDataToWrite);
				ndefDataToWrite = null;
			}
			else
			{
				byte[] data = await DumpMifareUL(mifare);
				ParseTLVData(data);
			}
		}

		private async Task WriteNdefToMifareUL(AccessHandler mifare, byte[] ndefData)
		{
			byte[] infoData = await mifare.ReadAsync(3);
			int capacity = infoData[2] * 8;

			byte[] wrappedData;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write((byte)0x03);
					if(ndefData.Length >= 0xFF)
					{
						writer.Write((byte)0xFF);
						writer.Write((ushort)ndefData.Length);
					}
					else
					{
						writer.Write((byte)ndefData.Length);
					}
					writer.Write(ndefData);

					writer.Write((byte)0xFE);
					writer.Write((byte)0x00);
				}

				wrappedData = stream.ToArray();
			}

			if (wrappedData.Length > capacity)
				throw new NfcHandlerException("Data size of " + wrappedData.Length + " bytes exceeds capacity of " + capacity + " bytes!");

			int data_length = wrappedData.Length;
			Array.Resize(ref wrappedData, (data_length / 4) * 4 + 4);

			for (byte pos = 4; (pos - 4) * 4 < wrappedData.Length; pos++)
			{
				await mifare.WriteAsync(pos, wrappedData.Skip((pos - 4) * 4).Take(4).ToArray());
			}

			StatusMessage?.Invoke("Written " + data_length + " bytes of data. Ndef message length is " + ndefData.Length + " bytes.");
		}

		private async Task<byte[]> DumpMifareUL(MifareUltralight.AccessHandler mifare)
		{
			byte[] infoData = await mifare.ReadAsync(3);
			int bytes_left = infoData[2] * 8;

			byte[] res = new byte[bytes_left];

			for (byte pos = 4; bytes_left > 0; pos += 4, bytes_left -= 16)
			{
				byte[] data = await mifare.ReadAsync(pos);
				if (bytes_left < 16)
					data = data.Take(bytes_left).ToArray();

				data.CopyTo(res, (pos - 4) * 4);
			}

			return res;
		}

		private void ParseTLVData(byte[] data)
		{
			using (MemoryStream stream = new MemoryStream(data))
			using (BinaryReader reader = new BinaryReader(stream))
			{
				while(stream.Position < stream.Length)
				{
					byte tag = reader.ReadByte();
					int length = reader.ReadByte();
					if (length >= 0xFF)
						length = reader.ReadUInt16();
					byte[] val = length > 0 ? reader.ReadBytes(length) : null;

					switch(tag)
					{
						case 0x00:
							StatusMessage?.Invoke("Skipping NULL TLV");
							break;
						case 0x01:
							StatusMessage?.Invoke("Skipping Lock Control TLV");
							break;
						case 0x02:
							StatusMessage?.Invoke("Skipping Memory Control TLV");
							break;
						case 0xFE:
							StatusMessage?.Invoke("Reached terminator TLV");
							return;
						case 0x03:
							if (val != null)
							{
								StatusMessage?.Invoke("Found NDEF TLV");
								NdefMessage msg = NdefMessage.FromByteArray(val);
								ReceiveNdefMessage?.Invoke(msg);
							}
							else
							{
								StatusMessage?.Invoke("Found empty NDEF TLV");
							}
							break;
						default:
							StatusMessage?.Invoke("Skipping unknown TLV " + BitConverter.ToString(new byte[] { tag }));
							break;
					}
				}
			}
		}
	}
}

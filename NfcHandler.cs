using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using NdefLibrary.Ndef;
using PCSC;
using PCSC.Utils;
using PCSC.Monitoring;
using PCSC.Exceptions;

using PcscSdk;
using PcscSdk.Common;


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
		private static readonly IContextFactory contextFactory = ContextFactory.Instance;

		public static string[] GetReaderNames()
		{
			using (var ctx = contextFactory.Establish(SCardScope.System))
			{
				return ctx.GetReaders();
			}
		}

		public NfcHandler()
		{
		}

		~NfcHandler()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (monitor != null)
			{
				monitor.Cancel();
				monitor.CardInserted -= Monitor_CardInserted;
				monitor.CardRemoved -= Monitor_CardRemoved;
				monitor.MonitorException -= Monitor_MonitorException;
				monitor.Dispose();
				monitor = null;
			}
		}

		public event Action<string> StatusMessage;
		public event Action<string> ErrorMessage;
		public event Action CardAdded;
		public event Action<NdefMessage> ReceiveNdefMessage;

		private byte[] ndefDataToWrite;
		private ISCardMonitor monitor;

		public void StartMonitoring(string[] readerNames = null)
		{
			if (monitor == null)
			{
				var monitorFactory = MonitorFactory.Instance;
				monitor = monitorFactory.Create(SCardScope.System);
				monitor.CardInserted += Monitor_CardInserted;
				monitor.CardRemoved += Monitor_CardRemoved;
				monitor.MonitorException += Monitor_MonitorException;
			}

			monitor.Cancel();

			if(readerNames != null)
			{
				monitor.Start(readerNames);
			}
			else
			{
				monitor.Start(GetReaderNames());
			}
		}

		public void WriteNdefMessage(NdefMessage msg)
		{
			ndefDataToWrite = msg.ToByteArray();
		}

		private void Monitor_CardInserted(object sender, CardStatusEventArgs args)
		{
			CardAdded?.Invoke();
			StatusMessage?.Invoke("Tag detected");

			try
			{
				HandleSmartCard(args.ReaderName);
			}
			catch (Exception e)
			{
				ErrorMessage?.Invoke("Error handling tag:\r\n" + e.ToString() + "\r\n");
			}
		}

		private void Monitor_CardRemoved(object sender, CardStatusEventArgs args)
		{
			StatusMessage?.Invoke("Tag removed");
		}

		private void Monitor_MonitorException(object sender, PCSCException exception)
		{
			ErrorMessage?.Invoke("Monitoring Error: " + exception.Message);
		}

		private void HandleSmartCard(String readerName)
		{
			using (ISCardContext ctx = contextFactory.Establish(SCardScope.System))
			using (ICardReader reader = ctx.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any))
			{
				StatusMessage?.Invoke("Connected to tag");

				IccDetection cardIdent = new IccDetection(reader);
				cardIdent.DetectCardType();

				StatusMessage?.Invoke("Device class: " + cardIdent.PcscDeviceClass.ToString());
				StatusMessage?.Invoke("Card name: " + cardIdent.PcscCardName.ToString());
				StatusMessage?.Invoke("ATR: " + BitConverter.ToString(cardIdent.Atr));

				if (cardIdent.PcscDeviceClass == PcscSdk.Common.DeviceClass.StorageClass &&
					(cardIdent.PcscCardName == CardName.MifareUltralight
					|| cardIdent.PcscCardName == CardName.MifareUltralightC
					|| cardIdent.PcscCardName == CardName.MifareUltralightEV1))
				{
					HandleMifareUL(reader);
				}
				else
				{
					throw new NfcHandlerException("Unsupported tag type");
				}
			}
		}

		private void HandleMifareUL(ICardReader reader)
		{
			var mifare = new MifareUltralight.AccessHandler(reader);

			StatusMessage?.Invoke("Handling as Mifare Ultralight");

			byte[] uid = mifare.GetUid();
			StatusMessage?.Invoke("UID: " + BitConverter.ToString(uid));

			byte[] infoData = mifare.Read(0);
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
				WriteNdefToMifareUL(mifare, ndefDataToWrite);
				ndefDataToWrite = null;
			}
			else
			{
				byte[] data = DumpMifareUL(mifare);
				ParseTLVData(data);
			}
		}

		private void WriteNdefToMifareUL(MifareUltralight.AccessHandler mifare, byte[] ndefData)
		{
			byte[] infoData = mifare.Read(3);
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
				mifare.Write(pos, wrappedData.Skip((pos - 4) * 4).Take(4).ToArray());
			}

			StatusMessage?.Invoke("Written " + data_length + " bytes of data. Ndef message length is " + ndefData.Length + " bytes.");
		}

		private byte[] DumpMifareUL(MifareUltralight.AccessHandler mifare)
		{
			byte[] infoData = mifare.Read(3);
			int bytes_left = infoData[2] * 8;

			byte[] res = new byte[bytes_left];

			for (byte pos = 4; bytes_left > 0; pos += 4, bytes_left -= 16)
			{
				byte[] data = mifare.Read(pos);
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

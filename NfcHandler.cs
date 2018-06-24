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
		public event Action CardRemoved;
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
			if (msg != null)
				ndefDataToWrite = msg.ToByteArray();
			else
				ndefDataToWrite = null;
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
				ErrorMessage?.Invoke("Error handling tag: " + e.Message);
			}
		}

		private void Monitor_CardRemoved(object sender, CardStatusEventArgs args)
		{
			CardRemoved?.Invoke();
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

				if (cardIdent.PcscDeviceClass == DeviceClass.StorageClass &&
					(cardIdent.PcscCardName == CardName.MifareUltralight
					|| cardIdent.PcscCardName == CardName.MifareUltralightC
					|| cardIdent.PcscCardName == CardName.MifareUltralightEV1))
				{
					HandleMifareUL(reader);
				}
				else if (cardIdent.PcscDeviceClass == DeviceClass.StorageClass &&
					(cardIdent.PcscCardName == CardName.MifareStandard1K
					|| cardIdent.PcscCardName == CardName.MifareStandard4K))
				{
					HandleMifareStandard(reader);
				}
				else
				{
					throw new NfcHandlerException("Unsupported tag type");
				}
			}
		}

		#region Mifare Ultralight
		private void HandleMifareUL(ICardReader reader)
		{
			var mifare = new PcscSdk.MifareUltralight.AccessHandler(reader);

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

		private void WriteNdefToMifareUL(PcscSdk.MifareUltralight.AccessHandler mifare, byte[] ndefData)
		{
			byte[] infoData = mifare.Read(3);
			int capacity = infoData[2] * 8;

			byte[] wrappedData = GenerateTLVData(ndefData);

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

		private byte[] DumpMifareUL(PcscSdk.MifareUltralight.AccessHandler mifare)
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

		#endregion

		#region Mifare Standard

		private void HandleMifareStandard(ICardReader reader)
		{
			var mifare = new PcscSdk.MifareStandard.AccessHandler(reader);

			StatusMessage?.Invoke("Handling as Mifare Standard 1K");

			byte[] uid = mifare.GetUid();
			StatusMessage?.Invoke("UID: " + BitConverter.ToString(uid));

			byte gpByte = InitAndGetGPMifareStandard(mifare);

			bool usesMad = (gpByte & 0x80) != 0;
			bool multiApp = (gpByte & 0x40) != 0;
			int madVersion = gpByte & 0x03;

			StatusMessage?.Invoke("Uses MAD: " + usesMad + "; Multi App: " + multiApp + "; Version: " + madVersion);

			if (ndefDataToWrite != null)
			{
				WriteNdefToMifareStandard(mifare, ndefDataToWrite);
				ndefDataToWrite = null;
			}
			else
			{
				if (!usesMad)
					throw new NfcHandlerException("No MAD in use");
				if (madVersion != 1)
					throw new NfcHandlerException("Unsupported MAD version: " + madVersion + " (Only version 1 is supported)");

				byte[] data = DumpMifareStandard(mifare);
				ParseTLVData(data);
			}
		}

		private byte InitAndGetGPMifareStandard(PcscSdk.MifareStandard.AccessHandler mifare)
		{
			mifare.LoadKey(new byte[] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 }, 0);
			StatusMessage?.Invoke("Loaded public MAD key in slot 0.");

			mifare.LoadKey(new byte[] { 0xD3, 0xF7, 0xD3, 0xF7, 0xD3, 0xF7 }, 1);
			StatusMessage?.Invoke("Loaded public NDEF key in slot 1.");

			byte[] infoData;
			try
			{
				infoData = mifare.Read(3, GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 0);
			}
			catch (Exception)
			{
				StatusMessage?.Invoke("Failed reading with default key.");

				mifare.LoadKey(PcscSdk.MifareStandard.DefaultKeys.FactoryDefault, 0);
				StatusMessage?.Invoke("Loaded factory default key in slot 0.");

				infoData = mifare.Read(3, GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 0);
				StatusMessage?.Invoke("Card uses factory default key!");
			}
			byte gpByte = infoData[9];
			StatusMessage?.Invoke("General purpose byte: " + BitConverter.ToString(new[] { gpByte }));

			return gpByte;
		}

		private void WriteNdefToMifareStandard(PcscSdk.MifareStandard.AccessHandler mifare, byte[] ndefData)
		{
			InitializeMifareStandard(mifare);

			int capacity = 15 * 3 * 16;

			byte[] wrappedData = GenerateTLVData(ndefData);

			if (wrappedData.Length > capacity)
				throw new NfcHandlerException("Data size of " + wrappedData.Length + " bytes exceeds capacity of " + capacity + " bytes!");

			int data_length = wrappedData.Length;
			Array.Resize(ref wrappedData, (data_length / 16) * 16 + 16);

			byte writeBlock = 3;
			for (byte pos = 0; pos < wrappedData.Length; pos += 16)
			{
				if (++writeBlock % 4 == 3)
					writeBlock++;
				mifare.Write(writeBlock, wrappedData.Skip(pos).Take(16).ToArray(), GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 1);
			}

			StatusMessage?.Invoke("Written " + data_length + " bytes of data. Ndef message length is " + ndefData.Length + " bytes.");
		}

		private void InitializeMifareStandard(PcscSdk.MifareStandard.AccessHandler mifare)
		{
			byte[][] madData = new byte[][] {
				null,
				new byte[] { 0x14, 0x01, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 },
				new byte[] { 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 },
				new byte[] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0x78, 0x77, 0x88, 0xC1, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }
			};

			byte[] ndefSectorIdent = new byte[] { 0xD3, 0xF7, 0xD3, 0xF7, 0xD3, 0xF7, 0x7F, 0x07, 0x88, 0x40, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
			byte[] ndefComparator = ndefSectorIdent.Skip(6).Take(4).ToArray();

			byte[] mad1 = mifare.Read(1, PcscSdk.MifareStandard.GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 0);
			byte[] mad2 = mifare.Read(2, PcscSdk.MifareStandard.GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 0);

			mifare.LoadKey(PcscSdk.MifareStandard.DefaultKeys.FactoryDefault, 0);
			StatusMessage?.Invoke("Loaded factory default key in slot 0.");

			if (!mad1.SequenceEqual(madData[1]) || !mad2.SequenceEqual(madData[2]))
			{
				StatusMessage?.Invoke("Writing NDEF MAD block.");

				for (byte block = 1; block < 4; block++)
				{
					StatusMessage?.Invoke("Writing MAD block " + block);
					mifare.Write(block, madData[block], GeneralAuthenticate.GeneralAuthenticateKeyType.PicoTagPassKeyB, 0);
				}
			}

			for (int sector = 1; sector < 16; sector++)
			{
				byte block = (byte)(sector * 4 + 3);
				try
				{
					for (int i = 1; i < 4; i++)
					{
						var authRes = mifare.CardReader.Transceive(new PcscSdk.MifareStandard.GeneralAuthenticate((byte)(block - i), 1, GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA));
						if (!authRes.Succeeded)
						{
							StatusMessage?.Invoke("NDEF read authentication failed, trying trailr rewrite.");
							throw new Exception();
						}
					}

					byte[] sectorIdent = mifare.Read(block, GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 1).Skip(6).Take(4).ToArray();
					if (!sectorIdent.SequenceEqual(ndefComparator))
						throw new Exception();
				}
				catch (Exception)
				{
					StatusMessage?.Invoke("Writing NDEF trailer into sector " + sector);
					mifare.Write(block, ndefSectorIdent, GeneralAuthenticate.GeneralAuthenticateKeyType.PicoTagPassKeyB, 0);
				}
			}

			StatusMessage?.Invoke("Tag is NDEF formated");
		}

		private byte[] DumpMifareStandard(PcscSdk.MifareStandard.AccessHandler mifare)
		{
			byte[] madData = new byte[32];
			mifare.Read(1, PcscSdk.MifareStandard.GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 0).CopyTo(madData, 0);
			mifare.Read(2, PcscSdk.MifareStandard.GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 0).CopyTo(madData, 16);

			byte crc = madData[0];
			byte calcCrc = PcscSdk.MifareStandard.CRC8.Calc(madData.Skip(1).ToArray());

			if (crc != calcCrc)
				throw new NfcHandlerException("MAD CRC mismatch. 0x" + BitConverter.ToString(new[] { crc }) + " != 0x" + BitConverter.ToString(new[] { calcCrc }));

			StatusMessage?.Invoke("CRC 0x" + BitConverter.ToString(new[] { crc }) + " OK");

			using (MemoryStream mem = new MemoryStream())
			{
				for (int sec = 1; sec < 16; sec++)
				{
					if (madData[sec * 2] != 0x03 || madData[sec * 2 + 1] != 0xE1)
						continue;
					for (int block = sec * 4; block < sec * 4 + 3; block++)
					{
						mem.Write(mifare.Read((ushort)block, PcscSdk.MifareStandard.GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 1), 0, 16);
					}
				}

				return mem.ToArray();
			}
		}

		#endregion

		#region Helpers

		private byte[] GenerateTLVData(byte[] ndefData)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write((byte)0x03);
					if (ndefData.Length >= 0xFF)
					{
						writer.Write((byte)0xFF);
						byte[] lengthBytes = BitConverter.GetBytes((ushort)ndefData.Length);
						if (BitConverter.IsLittleEndian)
							Array.Reverse(lengthBytes);
						writer.Write(lengthBytes);
					}
					else
					{
						writer.Write((byte)ndefData.Length);
					}
					writer.Write(ndefData);

					writer.Write((byte)0xFE);
					writer.Write((byte)0x00);
				}

				return stream.ToArray();
			}
		}

		private void ParseTLVData(byte[] data)
		{
			using (MemoryStream stream = new MemoryStream(data))
			using (BinaryReader reader = new BinaryReader(stream))
			{
				while(stream.Position < stream.Length)
				{
					byte tag = reader.ReadByte();
					int length = 0;
					byte[] val = null;
					if (tag != 0x00 && tag != 0xFE)
					{
						length = reader.ReadByte();
						if (length >= 0xFF)
						{
							byte[] lengthBytes = reader.ReadBytes(2);
							if (BitConverter.IsLittleEndian)
								Array.Reverse(lengthBytes);
							length = BitConverter.ToUInt16(lengthBytes, 0);
						}
						val = length > 0 ? reader.ReadBytes(length) : null;
					}

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
								StatusMessage?.Invoke("Found NDEF TLV (" + length + " bytes)");
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

		#endregion
	}
}

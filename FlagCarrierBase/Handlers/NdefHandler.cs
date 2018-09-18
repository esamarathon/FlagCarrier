using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using NdefLibrary.Ndef;
using Ionic.Zlib;

namespace FlagCarrierBase
{
	public class NdefHandlerException : Exception
	{
		public NdefHandlerException()
		{
		}

		public NdefHandlerException(string message)
			: base(message)
		{
		}

		public NdefHandlerException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	public static class NdefHandler
	{
		static readonly string EXPECTED_MIME_TYPE = "application/vnd.de.oromit.flagcarrier";
		static readonly string EXPECTED_APP_REC = "de.oromit.flagcarrier";

		#region TagReading

		public static Dictionary<string, string> ParseNdefMessage(byte[] raw)
		{
			NdefMessage ndefMessage;
			try
			{
				ndefMessage = NdefMessage.FromByteArray(raw);
			}
			catch (NdefException e)
			{
				throw new NdefHandlerException("Error parsing ndef: " + e.Message, e);
			}

			return ParseNdefMessage(ndefMessage);
		}

		public static Dictionary<string, string> ParseNdefMessage(NdefMessage msg)
		{
			foreach(NdefRecord rec in msg)
			{
				if(IsOurRecord(rec))
				{
					return ParsePayload(rec.Payload);
				}
			}

			throw new NdefHandlerException("Unsupported Tag");
		}

		private static Dictionary<string, string> ParsePayload(byte[] payload)
		{
			var res = ZlibStream.UncompressBuffer(payload);
			return ParseInflatedPayload(res);
		}

		private static string ReadUTF(BinaryReader reader)
		{
			byte[] b = reader.ReadBytes(2);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(b);
			ushort keyLength = BitConverter.ToUInt16(b, 0);

			if (keyLength > reader.BaseStream.Length - reader.BaseStream.Position)
				throw new NdefHandlerException("invalid readUTF length data");

			return Encoding.UTF8.GetString(reader.ReadBytes(keyLength));
		}

		private static Dictionary<string, string> ParseInflatedPayload(byte[] payload)
		{
			Dictionary<string, string> res = new Dictionary<string, string>();

			using (var mem = new MemoryStream(payload))
			using (var reader = new BinaryReader(mem))
			{
				while (reader.BaseStream.Length - reader.BaseStream.Position > 4)
				{
					string key = ReadUTF(reader);
					string val = ReadUTF(reader);

					res.Add(key, val);
				}

				if (reader.BaseStream.Position < reader.BaseStream.Length)
					throw new NdefHandlerException("Leftover data after reading");
			}

			return res;
		}

		private static bool IsOurRecord(NdefRecord rec)
		{
			if(rec.TypeNameFormat != NdefRecord.TypeNameFormatType.Mime)
				return false;

			if (Encoding.ASCII.GetString(rec.Type) != EXPECTED_MIME_TYPE)
				return false;

			return true;
		}

		#endregion

		#region TagWriting

		public static byte[] GenerateRawNdefMessage(Dictionary<string, string> values)
		{
			return GenerateNdefMessage(values).ToByteArray();
		}

		public static NdefMessage GenerateNdefMessage(Dictionary<string, string> values)
		{
			NdefMessage res = new NdefMessage
			{
				GenerateMimeRecord(values),
				GenerateAppRecord()
			};
			return res;
		}

		private static NdefRecord GenerateAppRecord()
		{
			NdefAndroidAppRecord rec = new NdefAndroidAppRecord
			{
				PackageName = EXPECTED_APP_REC
			};
			return rec;
		}

		private static NdefRecord GenerateMimeRecord(Dictionary<string, string> values)
		{
			NdefRecord rec = new NdefRecord(NdefRecord.TypeNameFormatType.Mime, Encoding.ASCII.GetBytes(EXPECTED_MIME_TYPE))
			{
				Payload = GenerateCompressedPayload(values)
			};
			return rec;
		}

		private static byte[] GenerateCompressedPayload(Dictionary<string, string> values)
		{
			byte[] rawData = GeneratePayload(values);

			using (var stream = new MemoryStream())
			{
				using (var compressor = new ZlibStream(stream, CompressionMode.Compress, CompressionLevel.BestCompression))
				{
					compressor.Write(rawData, 0, rawData.Length);
				}
				return stream.ToArray();
			}
		}

		private static byte[] GeneratePayload(Dictionary<string, string> values)
		{
			using (MemoryStream mem = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(mem))
				{
					foreach (var entry in values)
					{
						String key = entry.Key.Trim();
						String val = entry.Value.Trim();
						if (String.IsNullOrEmpty(val))
							continue;
						WriteUTF(writer, key);
						WriteUTF(writer, val);
					}
				}

				return mem.ToArray();
			}
		}

		private static void WriteUTF(BinaryWriter writer, string str)
		{
			byte[] data = Encoding.UTF8.GetBytes(str);
			byte[] len = BitConverter.GetBytes((ushort)data.Length);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(len);

			writer.Write(len);
			writer.Write(data);
		}

		#endregion
	}
}

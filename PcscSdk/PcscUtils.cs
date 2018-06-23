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
using System.IO;
using System.Threading.Tasks;
using System.Linq;

using PCSC;

namespace PcscSdk
{
	public static class SCardReaderExtension
	{
		/// <summary>
		/// Extension method to SmartCardConnection class similar to Transmit asyc method, however it accepts PCSC SDK commands.
		/// </summary>
		/// <param name="apduCommand">
		/// APDU command object to send to the ICC
		/// </param>
		/// <param name="reader">
		/// SmartCardConnection object
		/// </param>
		/// <returns>APDU response object of type defined by the APDU command object</returns>
		public static Iso7816.ApduResponse Transceive(this ICardReader reader, Iso7816.ApduCommand apduCommand)
		{
			Iso7816.ApduResponse apduRes = Activator.CreateInstance(apduCommand.ApduResponseType) as Iso7816.ApduResponse;

			byte[] resp = new byte[256];
			int bytesReceived = reader.Transmit(apduCommand.ToByteArray(), resp);
			Array.Resize(ref resp, bytesReceived);

			apduRes.ExtractResponse(resp);

			return apduRes;
		}

		/// <summary>
		/// Extension method to SmartCardConnection class to perform a transparent exchange to the ICC
		/// </summary>
		/// <param name="reader">
		/// SmartCardConnection object
		/// </param>
		/// <param name="commandData">
		/// Command object to send to the ICC
		/// </param>
		/// <returns>Response received from the ICC</returns>
		public static byte[] TransparentExchange(this ICardReader reader, byte[] commandData)
		{
			byte[] responseData = null;
			ManageSessionResponse apduRes = Transceive(reader, new ManageSession(new byte[2] { (byte)ManageSession.DataObjectType.StartTransparentSession, 0x00 })) as ManageSessionResponse;

			if (!apduRes.Succeeded)
			{
				throw new Exception("Failure to start transparent session, " + apduRes.ToString());
			}

			using (MemoryStream dataWriter = new MemoryStream())
			{
				dataWriter.WriteByte((byte)PcscSdk.TransparentExchange.DataObjectType.Transceive);
				dataWriter.WriteByte((byte)commandData.Length);
				dataWriter.Write(commandData, 0, commandData.Length);
				dataWriter.Flush();

				TransparentExchangeResponse apduRes1 = Transceive(reader, new TransparentExchange(dataWriter.ToArray())) as TransparentExchangeResponse;

				if (!apduRes1.Succeeded)
				{
					throw new Exception("Failure transceive with card, " + apduRes1.ToString());
				}

				responseData = apduRes1.IccResponse;
			}

			ManageSessionResponse apduRes2 = Transceive(reader, new ManageSession(new byte[2] { (byte)ManageSession.DataObjectType.EndTransparentSession, 0x00 })) as ManageSessionResponse;

			if (!apduRes2.Succeeded)
			{
				throw new Exception("Failure to end transparent session, " + apduRes2.ToString());
			}

			return responseData;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Pcsc;


namespace FlagCarrierWin
{
	static class SmartCardUtils
	{
		internal static async Task<SmartCardReader> FindNfcReaderAsync()
		{
			DeviceInformation info = await SmartCardReaderUtils.GetFirstSmartCardReaderInfo(SmartCardReaderKind.Nfc);
			if (info == null)
				info = await SmartCardReaderUtils.GetFirstSmartCardReaderInfo(SmartCardReaderKind.Any);

			if (info == null)
				return null;

			return await SmartCardReader.FromIdAsync(info.Id);
		}
	}
}

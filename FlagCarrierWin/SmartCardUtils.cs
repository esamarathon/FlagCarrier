using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Windows.Foundation.Metadata;


namespace FlagCarrierWin
{
	static class SmartCardUtils
	{
		internal static async Task<SmartCardReader> FindNfcReaderAsync()
		{
			if (!ApiInformation.IsTypePresent("Windows.Devices.SmartCards.SmartCardConnection"))
				return null;

			string query = SmartCardReader.GetDeviceSelector(SmartCardReaderKind.Nfc);
			string queryAny = SmartCardReader.GetDeviceSelector(SmartCardReaderKind.Any);

			DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(query);
			if(devices.Count < 1)
				devices = await DeviceInformation.FindAllAsync(queryAny);

			DeviceInformation info = devices.Where(d => d.IsEnabled).OrderByDescending(d => d.IsDefault).FirstOrDefault();
			if (info == null)
				return null;

			return await SmartCardReader.FromIdAsync(info.Id);
		}
	}
}

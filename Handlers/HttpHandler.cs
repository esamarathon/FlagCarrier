using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;

namespace FlagCarrierWin
{
	class HttpHandlerException : Exception
	{
		public HttpHandlerException()
		{
		}

		public HttpHandlerException(string message) : base(message)
		{
		}

		public HttpHandlerException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}

	class HttpHandler
	{
		private readonly HttpClient client = new HttpClient();

		public async Task<string> DoRequestAsync(string action, Dictionary<String, String> tagData = null, Dictionary<String, String> extraData = null)
		{
			string json = DataToJson(action, tagData, extraData);
			StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

			HttpResponseMessage response = await client.PostAsync(Properties.Settings.Default.targetUrl, content);

			if (response.StatusCode != HttpStatusCode.OK)
				throw new HttpHandlerException("HTTP Request failed: " + response.StatusCode.ToString());

			return await response.Content.ReadAsStringAsync();
		}

		private string DataToJson(string action, Dictionary<String, String> tagData = null, Dictionary<String, String> extraData = null)
		{
			JavaScriptSerializer ser = new JavaScriptSerializer();
			string deviceId = Properties.Settings.Default.deviceID;
			string groupId = Properties.Settings.Default.groupID;
			Dictionary<string, object> data = new Dictionary<string, object>();

			data.Add("action", action);
			data.Add("device_id", deviceId);
			data.Add("group_id", groupId);

			if (extraData != null)
			{
				foreach(var kv in extraData)
				{
					data.Add(kv.Key, kv.Value);
				}
			}

			if (tagData != null)
			{
				data.Add("tag_data", tagData);
			}

			return ser.Serialize(data);
		}
	}
}

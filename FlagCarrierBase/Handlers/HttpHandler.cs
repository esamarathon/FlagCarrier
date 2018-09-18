using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace FlagCarrierBase
{
	public class HttpHandlerException : Exception
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

	public class HttpHandler
	{
		private readonly HttpClient client = new HttpClient();

		public async Task<string> DoRequestAsync(string url, string deviceId, string groupId, string action, Dictionary<String, String> tagData = null, Dictionary<String, String> extraData = null)
		{
			string json = DataToJson(deviceId, groupId, action, tagData, extraData);
			StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

			HttpResponseMessage response = await client.PostAsync(url, content);

			if (response.StatusCode != HttpStatusCode.OK)
				throw new HttpHandlerException("HTTP Request failed: " + response.StatusCode.ToString());

			return await response.Content.ReadAsStringAsync();
		}

		private string DataToJson(string deviceId, string groupId, string action, Dictionary<String, String> tagData = null, Dictionary<String, String> extraData = null)
		{
			Dictionary<string, object> data = new Dictionary<string, object>
			{
				{ "action", action },
				{ "device_id", deviceId },
				{ "group_id", groupId }
			};

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

			return JsonConvert.SerializeObject(data);
		}
	}
}

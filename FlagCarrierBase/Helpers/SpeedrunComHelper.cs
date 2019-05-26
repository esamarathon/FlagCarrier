using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json.Linq;

namespace FlagCarrierBase.Helpers
{
	public class SpeedrunComHelperException : Exception
	{
		public SpeedrunComHelperException()
		{
		}

		public SpeedrunComHelperException(string message)
			: base(message)
		{
		}

		public SpeedrunComHelperException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	public class SpeedrunComHelperData
	{
		public string DisplayName = null;
		public string CountryCode = null;
		public string SrComName = null;
		public string TwitchName = null;
		public string TwitterHandle = null;
	}

	public static class SpeedrunComHelper
	{
		[ThreadStatic]
		private static readonly HttpClient httpClient = new HttpClient();

		public static async Task<SpeedrunComHelperData> GetUserInfo(string lookup_name)
		{
			if (lookup_name == null || lookup_name == "")
				return null;

			HttpResponseMessage response = await httpClient.GetAsync("https://www.speedrun.com/api/v1/users?lookup=" + HttpUtility.UrlEncode(lookup_name));

			if (response.StatusCode != HttpStatusCode.OK)
				throw new SpeedrunComHelperException("sr.com request failed: " + response.StatusCode.ToString());

			string data = await response.Content.ReadAsStringAsync();

			try
			{
				var srdata = JObject.Parse(data);
				var userdata = srdata["data"];

				if (userdata.Count() <= 0)
					throw new SpeedrunComHelperException("Not found on speedrun.com: " + lookup_name);

				userdata = userdata[0];

				SpeedrunComHelperData res = new SpeedrunComHelperData();

				try
				{
					res.DisplayName = (string)userdata["names"]["international"];
				}
				catch (Exception)
				{
					res.DisplayName = "";
				}

				try
				{
					res.CountryCode = (string)userdata["location"]["country"]["code"];
				}
				catch (Exception)
				{
					res.CountryCode = "DE";
				}

				try
				{
					string srname = (string)userdata["weblink"];
					res.SrComName = srname.Split('/').Last();
				}
				catch (Exception)
				{
					res.SrComName = "";
				}

				try
				{
					string twitch = (string)userdata["twitch"]["uri"];
					res.TwitchName = twitch.Split('/').Last();
				}
				catch (Exception)
				{
					res.TwitchName = "";
				}

				try
				{
					string twitter = (string)userdata["twitter"]["uri"];
					res.TwitterHandle = twitter.Split('/').Last();
				}
				catch (Exception)
				{
					res.TwitterHandle = "";
				}

				return res;
			}
			catch (Exception ex)
			{
				throw new SpeedrunComHelperException("Failed parsing sr.com data:\n" + ex.Message, ex);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagCarrierWin
{
    public static class Definitions
    {
        public static readonly string DISPLAY_NAME = "display_name";
        public static readonly string PRONOUNS = "pronouns";
        public static readonly string COUNTRY_CODE = "country_code";
        public static readonly string SRCOM_NAME = "speedruncom_name";
        public static readonly string TWITCH_NAME = "twitch_name";
        public static readonly string TWITTER_HANDLE = "twitter_handle";
        public static readonly Dictionary<string, string> KV_DISPLAY_VALUES = new Dictionary<string, string>
        {
                { DISPLAY_NAME, "Display Name" },
                { PRONOUNS, "Pronouns" },
                { COUNTRY_CODE, "Country Code" },
                { SRCOM_NAME, "Speedrun.com Name" },
                { TWITCH_NAME, "Twitch Name" },
                { TWITTER_HANDLE, "Twitter Handle" }
        };
    }
}

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
        public static readonly string USER_ID = "user_id";
        public static readonly Dictionary<string, string> KV_DISPLAY_VALUES = new Dictionary<string, string>
        {
                { DISPLAY_NAME, "Display Name" },
                { USER_ID, "User ID" }
        };
    }
}

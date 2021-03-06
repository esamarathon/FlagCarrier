using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FlagCarrierMini
{
    public class TagScannedEvent
    {
        public class FlagCarrierData
        {
            [JsonProperty("id")]
            public string ID { get; set; }

            [JsonProperty("group")]
            public string Group { get; set; }

            [JsonProperty("uid")]
            [JsonConverter(typeof(HexConverter))]
            public byte[] UID { get; set; }

            [JsonProperty("validSignature", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ValidSignature { get; set; } = null;

            [JsonProperty("pubKey", NullValueHandling = NullValueHandling.Ignore)]
            [JsonConverter(typeof(Base64Converter))]
            public byte[] PubKey { get; set; } = null;

            [JsonProperty("time")]
            public TimeData Time { get; set; }
        }

        public class TimeData
        {
            private DateTimeOffset dto = DateTimeOffset.UtcNow;

            [JsonProperty("iso")]
            public DateTimeOffset Iso
            {
                get => dto;
                set => dto = value;
            }

            [JsonProperty("unix")]
            public double Unix
            {
                get => dto.ToUnixTimeMilliseconds() / 1000.0;
                set => dto = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(value * 1000));
            }
        }

        public class UserData
        {
            [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
            public string ID { get; set; } = null;

            [JsonProperty(PropertyName = "displayName")]
            public string DisplayName { get; set; }
        }

        [JsonProperty(PropertyName = "flagcarrier")]
        public FlagCarrierData FlagCarrier { get; set; } = new FlagCarrierData();

        [JsonProperty(PropertyName = "user")]
        public UserData User { get; set; } = new UserData();

        [JsonProperty(PropertyName = "raw")]
        public Dictionary<string, string> RawData { get; set; } = new Dictionary<string, string>();
    }
}

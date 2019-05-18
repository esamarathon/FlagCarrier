using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace FlagCarrierMini
{
	class Options
	{
		[Option('k', "pubkey", Required = false, HelpText = "Base64 encoded ed25519 public key for signature verification.")]
		public string PubKey { get; set; }

		public byte[] PubKeyBin
		{
			get
			{
				return Convert.FromBase64String(PubKey);
			}
			set
			{
				PubKey = Convert.ToBase64String(value);
			}
		}

		[Option('v', "verbose", Default = false)]
		public bool Verbose { get; set; }
	}
}

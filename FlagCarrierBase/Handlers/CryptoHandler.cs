using System;
using System.Collections.Generic;
using System.Text;

using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math.EC.Rfc8032;


namespace FlagCarrierBase
{
	public class CryptoHandlerException : Exception
	{
		public CryptoHandlerException()
		{
		}

		public CryptoHandlerException(string message)
			: base(message)
		{
		}

		public CryptoHandlerException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	public class KeyPair
	{
		public byte[] PublicKey { get; set; }
		public byte[] PrivateKey { get; set; }
	}

	public static class CryptoHandler
	{
		private static readonly SecureRandom random = new SecureRandom();

		public static KeyPair GenKeyPair()
		{
			KeyPair res = new KeyPair
			{
				PrivateKey = new byte[Ed25519.SecretKeySize + Ed25519.PublicKeySize],
				PublicKey = new byte[Ed25519.PublicKeySize]
			};

			random.NextBytes(res.PrivateKey, 0, Ed25519.SecretKeySize);
			Ed25519.GeneratePublicKey(res.PrivateKey, 0, res.PublicKey, 0);
			Array.Copy(res.PublicKey, 0, res.PrivateKey, Ed25519.SecretKeySize, Ed25519.PublicKeySize);

			return res;
		}

		public static byte[] SignDetached(byte[] msg, byte[] privateKey)
		{
			if (privateKey.Length != Ed25519.SecretKeySize && privateKey.Length != Ed25519.SecretKeySize + Ed25519.PublicKeySize)
				throw new CryptoHandlerException("Invalid private key size for signing.");

			byte[] res = new byte[Ed25519.SignatureSize];
			Ed25519.Sign(privateKey, 0, msg, 0, msg.Length, res, 0);
			return res;
		}

		public static bool VerifyDetached(byte[] sig, byte[] msg, byte[] publicKey)
		{
			if (sig.Length != Ed25519.SignatureSize)
				throw new CryptoHandlerException("Signature size is invalid for verification.");
			if (publicKey.Length != Ed25519.PublicKeySize)
				throw new CryptoHandlerException("Public key size is invalid for verification.");

			return Ed25519.Verify(sig, 0, publicKey, 0, msg, 0, msg.Length);
		}

		public static bool IsKeyValid(byte[] key)
		{
			if (key == null)
				return false;

			return key.Length == Ed25519.SecretKeySize || key.Length == Ed25519.PublicKeySize || key.Length == Ed25519.SecretKeySize + Ed25519.PublicKeySize;
		}
	}
}

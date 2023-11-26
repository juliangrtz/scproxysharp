using SupercellProxy.Configuration;
using SupercellProxy.Logging;
using SupercellProxy.Utils;
using System.Collections.Generic;

namespace SupercellProxy.Crypto.Piranha
{
    internal class Keys
    {
        // Constant public key length
        private const int PublicKeyLength = 32;

        private static readonly Dictionary<SupercellGame, string> KeyVersions = new();

        /// <summary>
        /// The generated private key, according to the modded public key.
        /// </summary>
        public static byte[] GeneratedPrivateKey
            => "1891d401fadb51d25d3a9174d472a9f691a45b974285d47729c45c6538070d85".ToByteArray();

        /// <summary>
        /// The modded public key.
        /// </summary>
        public static byte[] ModdedPublicKey
            => "72f1a4a4c48e44da0c42310f800e96624e6dc6a641a9d41c3b5039d8dfadc27e".ToByteArray();

        /// <summary>
        /// The original, unmodified public key.
        /// Needed for SecretBox.Encrypt() and SecretBox.Decrypt()
        /// </summary>
        public static byte[] ServerPublicKey = new byte[PublicKeyLength];

        /// <summary>
        /// Sets the original public key
        /// </summary>
        public static void SetServerPublicKey()
        {
            try
            {
                // Add Versions
                KeyVersions.Add(SupercellGame.CLASH_OF_CLANS, "14.635.5");

                // Set the server public key
                switch (Config.SupercellGame)
                {
                    case SupercellGame.CLASH_OF_CLANS:
                        //ServerPublicKey = "2afb2f4e2c37b680239a9feaf3e2cebfedbb5d9133a971892a2f595564af994f".ToByteArray();
                        //ServerPublicKey = "edbb5d9133a971892a2f595564af994f2afb2f4e2c37b680239a9feaf3e2cebf".ToByteArray();
                        break;

                    default:
                        break;
                }
                Logger.Log("Public key initialized: " + ServerPublicKey.ToHexString());
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to set the publickey (" + ex.GetType() + ")", LogType.EXCEPTION);
            }
        }
    }
}
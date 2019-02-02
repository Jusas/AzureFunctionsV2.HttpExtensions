using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public static class JwtAuthHelpers
    {

        public static JsonWebKeySet JsonWebKeySetFromPublicKey(string publicKey)
        {
            using (var textReader = new StringReader(publicKey))
            {
                var pubkeyReader = new PemReader(textReader);
                RsaKeyParameters KeyParameters = (RsaKeyParameters)pubkeyReader.ReadObject();
                var e = Base64UrlEncoder.Encode(KeyParameters.Exponent.ToByteArrayUnsigned());
                var n = Base64UrlEncoder.Encode(KeyParameters.Modulus.ToByteArrayUnsigned());
                var dict = new Dictionary<string, string>() {
                    {"e", e},
                    {"kty", "RSA"},
                    {"n", n}
                };
                var hash = SHA256.Create();
                Byte[] hashBytes = hash.ComputeHash(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(dict)));
                JsonWebKey jsonWebKey = new JsonWebKey()
                {
                    Kid = Base64UrlEncoder.Encode(hashBytes),
                    Kty = "RSA",
                    E = e,
                    N = n
                };
                JsonWebKeySet jsonWebKeySet = new JsonWebKeySet();
                jsonWebKeySet.Keys.Add(jsonWebKey);

                return jsonWebKeySet;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Common
{
    public static class Utils
    {
        public static bool ValidUrl(string url)
        {
            Uri validatedUri;

            if (Uri.TryCreate(url, UriKind.Absolute, out validatedUri)) //.NET URI validation.
            {
                //If true: validatedUri contains a valid Uri. Check for the scheme in addition.
                return (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps);
            }
            return false;
        }

        internal static bool ValidateSignature(string base64EncodedData, string signature, string privateKey)
        {
            //var base64DecodedData = CryptoUtils.Base64Decode(data);

            var stringToBeHashed = privateKey + base64EncodedData + privateKey;
            var sha1HashedString = CryptoUtils.HashSHA1(stringToBeHashed);
            var base64EncodedSha1String = CryptoUtils.Base64Encode(sha1HashedString);

            return base64EncodedSha1String == signature;
        }
    }
}

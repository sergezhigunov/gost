﻿// <auto-generated />
using System.Resources;

namespace OpenGost.Security.Cryptography.Properties
{
    internal static class CryptographyStrings
    {
        private static ResourceManager ResourceManager { get; } = new ResourceManager(
            "OpenGost.Security.Cryptography.Properties.CryptographyStrings",
            typeof(CryptographyStrings).Assembly);

        public static string ArgumentInvalidOffLen
            => GetString("ArgumentInvalidOffLen");

        public static string ArgumentOutOfRangeIndexLength
            => GetString("ArgumentOutOfRangeIndexLength");

        public static string ArgumentOutOfRangeNeedNonNegNum
            => GetString("ArgumentOutOfRangeNeedNonNegNum");

        public static string ArgumentOutOfRangeNeedPositiveNum
            => GetString("ArgumentOutOfRangeNeedPositiveNum");

        public static string ArgumentOutOfRangeNegativeLength
            => GetString("ArgumentOutOfRangeNegativeLength");

        public static string ArgumentOutOfRangeStartIndex
            => GetString("ArgumentOutOfRangeStartIndex");

        public static string ArgumentOutOfRangeStartIndexLargerThanLength
            => GetString("ArgumentOutOfRangeStartIndexLargerThanLength");

        public static string CryptographicInsufficientOutputBuffer
            => GetString("CryptographicInsufficientOutputBuffer");

        public static string CryptographicInvalidBlockSize
            => GetString("CryptographicInvalidBlockSize");

        public static string CryptographicInvalidCipherMode
            => GetString("CryptographicInvalidCipherMode");

        public static string CryptographicInvalidDataSize
            => GetString("CryptographicInvalidDataSize");

        public static string CryptographicInvalidFeedbackSize
            => GetString("CryptographicInvalidFeedbackSize");

        public static string CryptographicInvalidHashSize(object size)
            => string.Format(
                GetString("CryptographicInvalidHashSize", nameof(size)),
                size);

        public static string CryptographicInvalidIVSize
            => GetString("CryptographicInvalidIVSize");

        public static string CryptographicInvalidOperation
            => GetString("CryptographicInvalidOperation");

        public static string CryptographicInvalidPadding
            => GetString("CryptographicInvalidPadding");

        public static string CryptographicInvalidSignatureSize(object size)
            => string.Format(
                GetString("CryptographicInvalidSignatureSize", nameof(size)),
                size);

        public static string CryptographicMissingDomainParameters
            => GetString("CryptographicMissingDomainParameters");

        public static string CryptographicMissingECDsaKeyValue
            => GetString("CryptographicMissingECDsaKeyValue");

        public static string CryptographicMissingKey
            => GetString("CryptographicMissingKey");

        public static string CryptographicMissingOid
            => GetString("CryptographicMissingOid");

        public static string CryptographicMissingPublicKey
            => GetString("CryptographicMissingPublicKey");

        public static string CryptographicSymmetricAlgorithmKeySet
            => GetString("CryptographicSymmetricAlgorithmKeySet");

        public static string CryptographicSymmetricAlgorithmNameNullOrEmpty
            => GetString("CryptographicSymmetricAlgorithmNameNullOrEmpty");

        public static string CryptographicSymmetricAlgorithmNameSet
            => GetString("CryptographicSymmetricAlgorithmNameSet");

        public static string CryptographicUnknownSymmetricAlgorithm(object algorithm)
            => string.Format(
                GetString("CryptographicUnknownSymmetricAlgorithm", nameof(algorithm)),
                algorithm);

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = ResourceManager.GetString(name);
            for (var i = 0; i < formatterNames.Length; i++)
            {
                value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
            }

            return value;
        }
    }
}

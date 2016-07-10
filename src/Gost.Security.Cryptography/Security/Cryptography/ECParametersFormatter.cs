﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Gost.Security.Cryptography
{
    using static SecurityCryptographyStrings;

    internal static class ECParametersFormatter
    {
        private const string ATag = "A";
        private const string BasePointParamsTag = "BasePointParams";
        private const string BasePointTag = "BasePoint";
        private const string BTag = "B";
        private const string CofactorTag = "Cofactor";
        private const string CurveParamsTag = "CurveParams";
        private const string DomainParametersTag = "DomainParameters";
        private const string ECDsaKeyValueTag = "ECDSAKeyValue";
        private const string ExplicitParamsTag = "ExplicitParams";
        private const string FieldParamsTag = "FieldParams";
        private const string Namespace = "http://www.w3.org/2001/04/xmldsig-more#";
        private const string OrderTag = "Order";
        private const string PrimeFieldElemTypeValue = "PrimeFieldElemType";
        private const string PrimeFieldParamsTypeValue = "PrimeFieldParamsType";
        private const string PTag = "P";
        private const string PublicKeyTag = "PublicKey";
        private const string TypeTag = "type";
        private const string ValueTag = "Value";
        private const string XsiPrefix = "xsi";
        private const string XmlnsPrefix = "xmlns";
        private const string XTag = "X";
        private const string YTag = "Y";

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        internal static ECParameters FromXml(string xmlString, int keySize)
        {
            using (var textReader = new StringReader(xmlString))
            using (XmlReader reader = XmlReader.Create(textReader))
            {
                if (!reader.IsStartElement(ECDsaKeyValueTag, Namespace))
                    throw new ArgumentException(CryptographicMissingECDsaKeyValue, nameof(xmlString));
                reader.ReadStartElement();

                if (!reader.IsStartElement(DomainParametersTag, Namespace))
                    throw new ArgumentException(CryptographicMissingDomainParameters, nameof(xmlString));
                ECCurve curve = ReadDomainParameters(reader, keySize);

                if (!reader.IsStartElement(PublicKeyTag, Namespace))
                    throw new ArgumentException(CryptographicMissingPublicKey, nameof(xmlString));
                ECPoint publicKey = ReadECPoint(reader, PublicKeyTag, Namespace, keySize);

                reader.ReadEndElement();

                return new ECParameters { Curve = curve, Q = publicKey };
            }
        }

        private static ECCurve ReadDomainParameters(XmlReader reader, int keySize)
        {
            reader.ReadStartElement();
            reader.MoveToContent();
            reader.ReadStartElement(ExplicitParamsTag, Namespace);
            reader.MoveToContent();
            byte[]
                prime = ReadPrimeFieldParameters(reader, FieldParamsTag, Namespace, keySize),
                a, b, order, cofactor;
            reader.MoveToContent();
            reader.ReadStartElement();
            reader.MoveToContent();
            a = ReadPrimeFieldElement(reader, ATag, Namespace, keySize);
            reader.MoveToContent();
            b = ReadPrimeFieldElement(reader, BTag, Namespace, keySize);
            reader.ReadEndElement();
            reader.MoveToContent();
            reader.ReadStartElement(BasePointParamsTag, Namespace);
            reader.MoveToContent();
            ECPoint baseBoint = ReadECPoint(reader, BasePointTag, Namespace, keySize);
            reader.MoveToContent();
            order = BigInteger.Parse(reader.ReadElementContentAsString(OrderTag, Namespace), CultureInfo.InvariantCulture).ToByteArray();
            if (reader.IsStartElement(CofactorTag, Namespace))
                cofactor = BigInteger.Parse(reader.ReadElementContentAsString(), CultureInfo.InvariantCulture).ToByteArray();
            else cofactor = null;
            reader.ReadEndElement();
            reader.ReadEndElement();
            reader.ReadEndElement();

            return new ECCurve
            {
                Prime = prime,
                A = a,
                B = b,
                G = baseBoint,
                Order = order,
                Cofactor = cofactor,
            };
        }

        private static byte[] ReadPrimeFieldParameters(XmlReader reader, string localName, string ns, int keySize)
        {
            reader.ReadStartElement(localName, ns);
            reader.MoveToContent();
            byte[] value = ToNormalizedByteArray(BigInteger.Parse(reader.ReadElementContentAsString(PTag, Namespace), CultureInfo.InvariantCulture), keySize);
            reader.ReadEndElement();
            return value;
        }

        private static byte[] ReadPrimeFieldElement(XmlReader reader, string localName, string ns, int keySize)
        {
            bool isEmpty = reader.IsEmptyElement;
            if (!reader.MoveToAttribute(TypeTag, XmlSchema.InstanceNamespace))
                throw new NotImplementedException();
            reader.ReadAttributeValue();
            string xsiType = reader.Value;
            if (xsiType != PrimeFieldElemTypeValue)
                throw new NotImplementedException();
            if (!reader.MoveToAttribute(ValueTag))
                throw new NotImplementedException();
            reader.ReadAttributeValue();
            byte[] result = ToNormalizedByteArray(BigInteger.Parse(reader[ValueTag], CultureInfo.InvariantCulture), keySize);
            reader.MoveToElement();
            reader.ReadStartElement(localName, ns);
            if (!isEmpty)
                reader.ReadEndElement();
            return result;
        }

        private static ECPoint ReadECPoint(XmlReader reader, string localName, string ns, int keySize)
        {
            reader.ReadStartElement(localName, ns);
            reader.MoveToContent();
            byte[] x = ReadPrimeFieldElement(reader, XTag, Namespace, keySize);
            reader.MoveToContent();
            byte[] y = ReadPrimeFieldElement(reader, YTag, Namespace, keySize);
            reader.ReadEndElement();
            return new ECPoint
            {
                X = x,
                Y = y
            };
        }

        internal static string ToXmlString(ECParameters parameters)
        {
            var xml = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
            };

            using (XmlWriter writer = XmlWriter.Create(xml, settings))
            {
                writer.WriteStartElement(ECDsaKeyValueTag, Namespace);
                writer.WriteAttributeString(null, XmlnsPrefix, null, Namespace);
                writer.WriteAttributeString(XmlnsPrefix, XsiPrefix, null, XmlSchema.InstanceNamespace);
                WriteDomainParameters(writer, parameters);
                WriteECPoint(writer, PublicKeyTag, Namespace, parameters.Q);
                writer.WriteEndElement();
            }

            return xml.ToString();
        }

        private static void WriteDomainParameters(XmlWriter writer, ECParameters parameters)
        {
            writer.WriteStartElement(DomainParametersTag, Namespace);
            WriteExplicitParameters(writer, parameters.Curve);
            writer.WriteEndElement();
        }

        private static void WriteExplicitParameters(XmlWriter writer, ECCurve curve)
        {
            writer.WriteStartElement(ExplicitParamsTag, Namespace);
            WritePrimeFieldParameters(writer, curve.Prime);
            WriteCurveParameters(writer, curve);
            WriteBasePointParameters(writer, curve);
            writer.WriteEndElement();
        }

        private static void WritePrimeFieldParameters(XmlWriter writer, byte[] prime)
        {
            writer.WriteStartElement(FieldParamsTag, Namespace);
            WriteXsiType(writer, PrimeFieldParamsTypeValue, Namespace);
            writer.WriteElementString(PTag, Namespace, ToNonNegativeNumericString(prime));
            writer.WriteEndElement();
        }

        private static void WriteCurveParameters(XmlWriter writer, ECCurve curve)
        {
            writer.WriteStartElement(CurveParamsTag, Namespace);
            WritePrimeFieldElement(writer, ATag, Namespace, curve.A);
            WritePrimeFieldElement(writer, BTag, Namespace, curve.B);
            writer.WriteEndElement();
        }

        private static void WriteBasePointParameters(XmlWriter writer, ECCurve curve)
        {
            writer.WriteStartElement(BasePointParamsTag, Namespace);
            WriteECPoint(writer, BasePointTag, Namespace, curve.G);
            writer.WriteElementString(OrderTag, Namespace, ToNonNegativeNumericString(curve.Order));
            if (curve.Cofactor != null)
                writer.WriteElementString(CofactorTag, Namespace, ToNonNegativeNumericString(curve.Cofactor));
            writer.WriteEndElement();
        }

        private static void WriteECPoint(XmlWriter writer, string localName, string ns, ECPoint point)
        {
            writer.WriteStartElement(localName, ns);
            WritePrimeFieldElement(writer, XTag, Namespace, point.X);
            WritePrimeFieldElement(writer, YTag, Namespace, point.Y);
            writer.WriteEndElement();
        }

        private static void WritePrimeFieldElement(XmlWriter writer, string localName, string ns, byte[] value)
        {
            writer.WriteStartElement(localName, ns);
            WriteXsiType(writer, PrimeFieldElemTypeValue, Namespace);
            writer.WriteAttributeString(ValueTag, ToNonNegativeNumericString(value));
            writer.WriteEndElement();
        }

        private static void WriteXsiType(XmlWriter writer, string localName, string ns)
        {
            writer.WriteStartAttribute(TypeTag, XmlSchema.InstanceNamespace);
            writer.WriteQualifiedName(localName, ns);
            writer.WriteEndAttribute();
        }

        private static string ToNonNegativeNumericString(byte[] value)
        {
            var numericValue = new BigInteger(value);
            if (numericValue < BigInteger.Zero)
                numericValue += (BigInteger.One << value.Length * 8);
            return numericValue.ToString("R", CultureInfo.InvariantCulture);
        }

        private static byte[] ToNormalizedByteArray(BigInteger value, int keySize)
        {
            if (value < BigInteger.Zero)
                value += (BigInteger.One << keySize);
            keySize /= 8;
            byte[] result = new byte[keySize];
            for (int i = 0; i < keySize; i++)
            {
                if (value == BigInteger.Zero)
                    break;
                result[i] = (byte)(value % 256);
                value >>= 8;
            }
            return result;
        }
    }
}
﻿using System;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;

namespace Gost.Security.Cryptography
{
    using static Buffer;
    using static CryptoUtils;
    using static Math;
    using static SecurityCryptographyStrings;

    /// <summary>
    /// Provides a managed implementation of the <see cref="GostECDsa"/> algorithm. 
    /// </summary>
    public sealed class GostECDsaManaged : GostECDsa
    {
        #region Constants

        private static ECCurve ECCurve256ParamsetA { get; } = new ECCurve
        {
            Prime = new byte[]
            {
                0x97, 0xfd, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            },
            A = new byte[]
            {
                0x35, 0x73, 0x7e, 0x27, 0x6f, 0x65, 0x2c, 0xb2, 0x33, 0xaa, 0x95, 0xbf, 0x13, 0x20, 0x5e, 0xe2,
                0x7c, 0xa2, 0x35, 0x30, 0xc2, 0x92, 0x48, 0xaf, 0x73, 0x16, 0x98, 0x13, 0x15, 0x3f, 0x17, 0xc2,
            },
            B = new byte[]
            {
                0x13, 0x95, 0xae, 0xf8, 0xa6, 0x37, 0x93, 0xba, 0xf7, 0x7b, 0xe1, 0x08, 0x91, 0xcd, 0xfc, 0x22,
                0x1a, 0xd4, 0xa9, 0x59, 0xc3, 0xe7, 0x20, 0xcc, 0x9c, 0xed, 0x28, 0x74, 0xae, 0x9b, 0x5f, 0x29,
            },
            Order = new byte[]
            {
                0x67, 0x0c, 0x36, 0x6c, 0x55, 0xaf, 0x15, 0xc1, 0x35, 0x66, 0x7b, 0xc8, 0xdf, 0xcd, 0xd8, 0x0f,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40,
            },
            Cofactor = new byte[]
            {
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            },
            G = new ECPoint
            {
                X = new byte[]
                {
                    0x28, 0xaa, 0x2d, 0x74, 0xfe, 0x82, 0x25, 0x8b, 0xc7, 0x02, 0x2e, 0x93, 0x96, 0x91, 0x8b, 0x65,
                    0xbb, 0xb2, 0x12, 0x57, 0x42, 0x23, 0x09, 0x88, 0x0d, 0x2c, 0xe8, 0xa5, 0x43, 0x84, 0xe3, 0x91,
                },
                Y = new byte[]
                {
                    0x5c, 0x2e, 0x32, 0x32, 0xdb, 0x8a, 0x26, 0xaf, 0x40, 0x67, 0x76, 0x44, 0x53, 0x0b, 0xde, 0x5f,
                    0x56, 0xe9, 0x46, 0xbb, 0xc4, 0x86, 0x57, 0x89, 0x75, 0x03, 0x1a, 0xab, 0x23, 0x94, 0x87, 0x32,
                },
            },
        };

        private static ECCurve ECCurve512ParamsetA { get; } = new ECCurve
        {
            Prime = new byte[]
            {
                0xc7, 0xfd, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            },
            A = new byte[]
            {
                0xc4, 0xfd, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            },
            B = new byte[]
            {
                0x60, 0xc7, 0x71, 0x5a, 0x78, 0x90, 0x31, 0x50, 0x61, 0x47, 0xee, 0xeb, 0xd4, 0xf9, 0x2e, 0x86,
                0xdd, 0x90, 0xda, 0x10, 0x40, 0x57, 0xb4, 0x4c, 0x61, 0x27, 0x0d, 0xf3, 0x90, 0xb0, 0x3c, 0xee,
                0x65, 0x62, 0x0b, 0xfd, 0x1c, 0x08, 0xbd, 0x79, 0xe8, 0xb0, 0x1c, 0x76, 0x74, 0x25, 0xb8, 0x34,
                0xda, 0xf1, 0x67, 0x66, 0x2b, 0x0b, 0xbd, 0xc1, 0xdd, 0x86, 0xfc, 0xed, 0x5d, 0x50, 0xc2, 0xe8,
            },
            Order = new byte[]
            {
                0x75, 0xb2, 0x10, 0x1f, 0x41, 0xb1, 0xcd, 0xca, 0x5d, 0xb8, 0xd2, 0xfa, 0xab, 0x38, 0x4b, 0x9b,
                0x60, 0x60, 0x05, 0x4e, 0x8d, 0x2b, 0xf2, 0x6f, 0x11, 0x89, 0x8d, 0xf4, 0x32, 0x95, 0xe6, 0x27,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            },
            Cofactor = new byte[]
            {
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            },
            G = new ECPoint
            {
                X = new byte[]
                {
                    0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                },
                Y = new byte[]
                {
                    0xa4, 0xf2, 0x15, 0x52, 0xcb, 0x89, 0xa5, 0x89, 0xb8, 0xf5, 0x35, 0xc2, 0x5f, 0xfe, 0x28, 0x80,
                    0xe9, 0x41, 0x3a, 0x0e, 0xa5, 0xe6, 0x75, 0x3d, 0xe9, 0x36, 0xd0, 0x4f, 0xbe, 0x26, 0x16, 0xdf,
                    0x21, 0xa9, 0xef, 0xcb, 0xfd, 0x64, 0x80, 0x77, 0xc1, 0xab, 0xf1, 0xac, 0x93, 0x1c, 0x5e, 0xce,
                    0xe6, 0x50, 0x54, 0xe2, 0x16, 0x88, 0x1b, 0xa6, 0xe3, 0x6a, 0x83, 0x7a, 0xe8, 0xcf, 0x03, 0x75,
                },
            },
        };

        private static ECCurve ECCurve512ParamsetB { get; } = new ECCurve
        {
            Prime = new byte[]
            {
                0x6f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80,
            },
            A = new byte[]
            {
                0x6c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80,
            },
            B = new byte[]
            {
                0x16, 0x01, 0x14, 0xc5, 0xc7, 0xcb, 0x8c, 0xfb, 0x6e, 0x10, 0xa3, 0x1f, 0xee, 0x8b, 0xf7, 0x50,
                0x9c, 0xb6, 0x1a, 0xad, 0x6f, 0x27, 0x8b, 0x7f, 0x21, 0x6d, 0x41, 0xb1, 0x2d, 0x5d, 0x96, 0x3e,
                0x9f, 0x28, 0x4b, 0x6c, 0x80, 0xdc, 0x85, 0xbf, 0xbc, 0x38, 0xf1, 0x4a, 0x61, 0x7d, 0x7c, 0xb9,
                0x17, 0x25, 0x5e, 0x6f, 0xcf, 0x06, 0x3e, 0x7e, 0x45, 0x41, 0xc8, 0x9d, 0x45, 0x1b, 0x7d, 0x68,
            },
            Order = new byte[]
            {
                0xbd, 0x25, 0x4f, 0x37, 0x54, 0x6c, 0x34, 0xc6, 0x0e, 0xea, 0x1b, 0x10, 0x12, 0x67, 0x99, 0x8b,
                0xfa, 0x0c, 0xd4, 0xd9, 0x7b, 0xb7, 0xfd, 0xac, 0x45, 0xa5, 0x65, 0x25, 0x14, 0xec, 0xa1, 0x49,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80,
            },
            Cofactor = new byte[]
            {
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            },
            G = new ECPoint
            {
                X = new byte[]
                {
                    0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                },
                Y = new byte[]
                {
                    0xbd, 0x41, 0xfe, 0x80, 0x07, 0x34, 0x21, 0x7e, 0xec, 0xee, 0x4c, 0xf9, 0x55, 0x10, 0x04, 0x28,
                    0x88, 0x39, 0xc0, 0xf8, 0xaa, 0xbc, 0x2c, 0x15, 0x39, 0x4a, 0xdf, 0x1e, 0xfd, 0x28, 0xb2, 0xdc,
                    0x35, 0x73, 0xec, 0xc8, 0xe6, 0xd9, 0x6d, 0xbe, 0x13, 0xc2, 0x78, 0x75, 0x69, 0x3b, 0x12, 0x3c,
                    0x0f, 0x94, 0xa8, 0x47, 0x36, 0x1e, 0x07, 0x2c, 0x4c, 0x09, 0x9b, 0x38, 0xda, 0x7e, 0x8f, 0x1a,
                },
            },
        };

        private static ECCurve ECCurve512ParamsetC { get; } = new ECCurve
        {
            Prime = new byte[]
            {
                0xc7, 0xfd, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            },
            A = new byte[]
            {
                0xd3, 0x9b, 0x68, 0x39, 0x6f, 0x54, 0xb6, 0x2e, 0x2a, 0x9f, 0xda, 0x1f, 0x95, 0x7f, 0xd9, 0x2a,
                0xff, 0x50, 0xcf, 0x6f, 0xf4, 0x71, 0xde, 0x2a, 0xd9, 0xed, 0xc9, 0xe2, 0xc0, 0x61, 0xe8, 0x46,
                0x45, 0x06, 0x43, 0xe1, 0x68, 0x1c, 0xe4, 0x4d, 0x64, 0x66, 0xb8, 0x0e, 0x98, 0xc8, 0x7b, 0x18,
                0xfb, 0x22, 0xc7, 0xd2, 0x29, 0xa5, 0x85, 0x54, 0x87, 0x21, 0xa7, 0x14, 0xe5, 0x03, 0x92, 0xdc,
            },
            B = new byte[]
            {
                0xe1, 0x57, 0x25, 0x31, 0xa5, 0x19, 0x23, 0x8d, 0x3c, 0x0a, 0xbf, 0xf5, 0xa5, 0xc7, 0x8c, 0x2b,
                0xb5, 0xf3, 0xfe, 0x8b, 0x4b, 0x28, 0xe0, 0x8d, 0xc1, 0xd2, 0x19, 0xf7, 0xff, 0xc2, 0xcb, 0x38,
                0xe0, 0xad, 0xe5, 0x0d, 0x4f, 0x2e, 0xda, 0xff, 0x57, 0x4b, 0x9f, 0xf6, 0xa9, 0xb6, 0xef, 0xc7,
                0x6a, 0xf1, 0x37, 0xcf, 0x52, 0x29, 0xc1, 0x8a, 0x2c, 0x6c, 0xbc, 0xce, 0x28, 0xee, 0xc4, 0xb4,
            },
            Order = new byte[]
            {
                0xed, 0x23, 0xf0, 0x47, 0xef, 0x3c, 0x62, 0x94, 0x26, 0xa1, 0x69, 0xa7, 0xe7, 0xa9, 0xed, 0xc8,
                0x2c, 0x50, 0x47, 0x51, 0xff, 0xa9, 0x33, 0x4c, 0x00, 0xab, 0x06, 0x65, 0xa4, 0xdb, 0x8c, 0xc9,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x3f,
            },
            Cofactor = new byte[]
            {
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            },
            G = new ECPoint
            {
                X = new byte[]
                {
                    0x48, 0x01, 0x95, 0xc1, 0x28, 0x79, 0xbc, 0xc5, 0xaa, 0x97, 0xae, 0x7e, 0x48, 0x85, 0xfb, 0xc6,
                    0x10, 0x36, 0xed, 0xb9, 0x3d, 0x03, 0xb9, 0xa7, 0xf2, 0x2b, 0x60, 0xae, 0xa7, 0x72, 0x72, 0xa2,
                    0x3a, 0x04, 0xea, 0x4c, 0x07, 0xf7, 0x85, 0xd3, 0x21, 0xf0, 0xae, 0xcb, 0xa9, 0xb7, 0x95, 0x22,
                    0xde, 0xf5, 0x3e, 0x59, 0xce, 0x41, 0xe2, 0xeb, 0xbd, 0xe7, 0x3d, 0xc2, 0xdf, 0x1e, 0xe3, 0xe2,
                },
                Y = new byte[]
                {
                    0x0f, 0xc4, 0xdd, 0x9a, 0x9a, 0x6e, 0x39, 0xd0, 0x07, 0xae, 0x4b, 0x85, 0xaa, 0x26, 0xf7, 0x04,
                    0x63, 0x3b, 0x42, 0x22, 0x58, 0xd8, 0x32, 0xef, 0xd2, 0x1e, 0x02, 0xe3, 0x33, 0x2d, 0x8e, 0xe1,
                    0x9b, 0xff, 0x90, 0x20, 0x3d, 0x8c, 0x10, 0x8c, 0x8b, 0x37, 0x27, 0x65, 0x4d, 0x80, 0x39, 0x79,
                    0x57, 0xb8, 0x1c, 0x91, 0xf5, 0xcf, 0xbc, 0xab, 0x99, 0xb8, 0x5e, 0x5b, 0xd9, 0x40, 0xce, 0xf5,
                },
            },
        };

        #endregion

        private static readonly KeySizes[] s_legalKeySizes = { new KeySizes(256, 512, 256) };
        private static readonly BigInteger
            s_two = 2,
            s_three = 3,
            s_twoPow256 = BigInteger.One << 256,
            s_twoPow512 = BigInteger.One << 512;

        private ECCurve _curve;
        private ECPoint _publicKey;
        private byte[] _privateKey;
        private bool
            _parametersSet = false,
            _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GostECDsaManaged" /> class
        /// with a random key pair.
        /// </summary>
        public GostECDsaManaged()
            : this(512)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GostECDsaManaged" /> class
        /// with a random key pair, using the specified key size.
        /// </summary>
        /// <param name="keySize">
        /// The size of the key. Valid key sizes are 256 and 512 bits.
        /// </param>
        /// <exception cref="CryptographicException">
        /// <paramref name="keySize"/> specifies an invalid length.
        /// </exception>
        public GostECDsaManaged(int keySize)
        {
            LegalKeySizesValue = s_legalKeySizes;
            KeySize = keySize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GostECDsaManaged" /> class
        /// with a specified <see cref="ECParameters"/>.
        /// </summary>
        /// <param name="parameters">
        /// The elliptic curve parameters. Valid key sizes are 256 and 512 bits.
        /// </param>
        /// <exception cref="CryptographicException">
        /// <paramref name="parameters"/> specifies an invalid key length.
        /// </exception>
        public GostECDsaManaged(ECParameters parameters)
        {
            LegalKeySizesValue = s_legalKeySizes;

            ImportParameters(parameters);
        }

        /// <summary>
        /// Generates a new public/private key pair for the specified curve.
        /// </summary>
        /// <param name="curve">
        /// The curve to use.
        /// </param>
        /// <exception cref="CryptographicException">
        /// <paramref name="curve"/> is invalid.
        /// </exception>
        public override void GenerateKey(ECCurve curve)
        {
            curve.Validate();

            int
                keySizeInByted = curve.Prime.Length,
                keySize = keySizeInByted * 8;

            BigInteger
                modulus = keySizeInByted == 64 ? s_twoPow512 : s_twoPow256,
                prime = Normalize(new BigInteger(curve.Prime), modulus),
                subgroupOrder = Normalize(new BigInteger(curve.Order), modulus) / Normalize(new BigInteger(curve.Cofactor), modulus),
                a = Normalize(new BigInteger(curve.A), modulus);

            byte[] privateKey = new byte[keySizeInByted];
            BigInteger key;
            do
            {
                StaticRandomNumberGenerator.GetBytes(privateKey);
                key = Normalize(new BigInteger(privateKey), modulus);
            } while (BigInteger.Zero >= key || key >= subgroupOrder);

            var basePoint = new BigIntegerPoint(curve.G, modulus);

            ECPoint publicKey = BigIntegerPoint.Multiply(basePoint, key, prime, a).ToECPoint(KeySize);

            EraseData(ref _privateKey);
            KeySize = keySize;
            _curve = CloneECCurve(curve);
            _publicKey = publicKey;
            _privateKey = privateKey;
            _parametersSet = true;
        }

        /// <summary>
        /// Exports the <see cref="ECParameters"/> for an <see cref="ECCurve"/>.
        /// </summary>
        /// <param name="includePrivateParameters">
        /// <c>true</c> to include private parameters;
        /// otherwise, <c>false</c>.</param>
        /// <returns>
        /// An <see cref="ECParameters"/>.
        /// </returns>
        /// <exception cref="CryptographicException">
        /// The key cannot be exported. 
        /// </exception>
        public override ECParameters ExportParameters(bool includePrivateParameters)
        {
            ThrowIfDisposed();

            if (!_parametersSet)
                GenerateKey(GetDefaultCurve());

            return new ECParameters
            {
                Curve = CloneECCurve(_curve),
                Q = CloneECPoint(_publicKey),
                D = includePrivateParameters ? CloneBuffer(_privateKey) : null,
            };
        }

        /// <summary>
        /// Imports the specified <see cref="ECParameters"/>.
        /// </summary>
        /// <param name="parameters">
        /// The curve parameters.
        /// </param>
        /// <exception cref="CryptographicException">
        /// <paramref name="parameters"/> are invalid.
        /// </exception>
        public override void ImportParameters(ECParameters parameters)
        {
            ThrowIfDisposed();

            parameters.Validate();
            KeySize = parameters.Q.X.Length * 8;

            _curve = CloneECCurve(parameters.Curve);
            _publicKey = CloneECPoint(parameters.Q);
            _privateKey = CloneBuffer(parameters.D);
            _parametersSet = true;
        }

        /// <summary>
        /// Generates a digital signature for the specified hash value.
        /// </summary>
        /// <param name="hash">
        /// The hash value of the data that is being signed.
        /// </param>
        /// <returns>
        /// A digital signature that consists of the given hash value encrypted with the private key.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="hash"/> parameter is <c>null</c>.
        /// </exception>
        public override byte[] SignHash(byte[] hash)
        {
            if (hash == null) throw new ArgumentNullException(nameof(hash));

            ThrowIfDisposed();

            if (KeySize / 8 != hash.Length)
                throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, CryptographicInvalidHashSize, KeySize / 8));

            int keySizeInByted = KeySize / 8;

            if (!_parametersSet)
                GenerateKey(GetDefaultCurve());

            BigInteger
                modulus = keySizeInByted == 64 ? s_twoPow512 : s_twoPow256,
                subgroupOrder = Normalize(new BigInteger(_curve.Order), modulus) / Normalize(new BigInteger(_curve.Cofactor), modulus);

            BigInteger e = Normalize(new BigInteger(hash), modulus) % subgroupOrder;

            if (e == BigInteger.Zero)
                e = BigInteger.One;

            BigInteger
                prime = Normalize(new BigInteger(_curve.Prime), modulus),
                a = Normalize(new BigInteger(_curve.A), modulus),
                d = Normalize(new BigInteger(_privateKey), modulus),
                k, r, s;

            var rgb = new byte[keySizeInByted];

            do
            {
                do
                {
                    do
                    {
                        StaticRandomNumberGenerator.GetBytes(rgb);
                        k = Normalize(new BigInteger(rgb), modulus);
                    } while (k <= BigInteger.Zero || k >= subgroupOrder);

                    r = BigIntegerPoint.Multiply(new BigIntegerPoint(_curve.G, modulus), k, prime, a).X;
                } while (r == BigInteger.Zero);

                s = (r * d + k * e) % subgroupOrder;
            } while (s == BigInteger.Zero);

            byte[]
                signature = new byte[keySizeInByted * 2],
                array = s.ToByteArray();

            BlockCopy(array, 0, signature, 0, Min(array.Length, keySizeInByted));
            array = r.ToByteArray();
            BlockCopy(array, 0, signature, keySizeInByted, Min(array.Length, keySizeInByted));

            return signature;
        }

        /// <summary>
        /// Verifies a digital signature against the specified hash value. 
        /// </summary>
        /// <param name="hash">
        /// The hash value of a block of data.
        /// </param>
        /// <param name="signature">
        /// The digital signature to be verified.
        /// </param>
        /// <returns>
        /// <c>true</c> if the hash value equals the decrypted signature;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="hash"/> parameter is <c>null</c>.
        /// -or-
        /// The <paramref name="signature"/> parameter is <c>null</c>.
        /// </exception>
        public override bool VerifyHash(byte[] hash, byte[] signature)
        {
            if (hash == null) throw new ArgumentNullException(nameof(hash));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            ThrowIfDisposed();

            if (KeySize / 8 != hash.Length)
                throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, CryptographicInvalidHashSize, KeySize / 8));
            if (KeySize / 4 != signature.Length)
                throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, CryptographicInvalidSignatureSize, KeySize / 4));

            // There is no necessity to generate new parameter, just return false
            if (!_parametersSet)
                return false;

            int keySizeInByted = KeySize / 8;

            BigInteger
                modulus = keySizeInByted == 64 ? s_twoPow512 : s_twoPow256,
                subgroupOrder = Normalize(new BigInteger(_curve.Order), modulus) / Normalize(new BigInteger(_curve.Cofactor), modulus);

            byte[] array = new byte[keySizeInByted];

            BlockCopy(signature, 0, array, 0, keySizeInByted);
            BigInteger s = Normalize(new BigInteger(array), modulus);
            if (s < BigInteger.One || s > subgroupOrder)
                return false;

            BlockCopy(signature, keySizeInByted, array, 0, keySizeInByted);
            BigInteger r = Normalize(new BigInteger(array), modulus);
            if (r < BigInteger.One || r > subgroupOrder)
                return false;

            BigInteger e = Normalize(new BigInteger(hash), modulus) % subgroupOrder;

            if (e == BigInteger.Zero)
                e = BigInteger.One;

            BigInteger
                v = BigInteger.ModPow(e, subgroupOrder - s_two, subgroupOrder),
                z1 = (s * v) % subgroupOrder,
                z2 = (subgroupOrder - r) * v % subgroupOrder,
                prime = Normalize(new BigInteger(_curve.Prime), modulus),
                a = Normalize(new BigInteger(_curve.A), modulus);

            BigIntegerPoint c = BigIntegerPoint.Add(
                BigIntegerPoint.Multiply(new BigIntegerPoint(_curve.G, modulus), z1, prime, a),
                BigIntegerPoint.Multiply(new BigIntegerPoint(_publicKey, modulus), z2, prime, a),
                prime);

            return c.X == r;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="GostECDsaManaged"/> class
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c>true to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                EraseData(ref _privateKey);

                if (disposing)
                {
                    EraseData(ref _curve.Prime);
                    EraseData(ref _curve.A);
                    EraseData(ref _curve.B);
                    EraseData(ref _curve.Order);
                    EraseData(ref _curve.Cofactor);
                    EraseData(ref _publicKey.X);
                    EraseData(ref _publicKey.Y);
                    ECPoint g = _curve.G;
                    EraseData(ref g.X);
                    EraseData(ref g.Y);
                }
            }

            base.Dispose(disposing);
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private ECCurve GetDefaultCurve()
        {
            switch (KeySize)
            {
                case 512:
                    return CloneECCurve(ECCurve512ParamsetA);

                case 256:
                    return CloneECCurve(ECCurve256ParamsetA);

                default:
                    throw new CryptographicException(CryptographicInvalidKeySize);
            }
        }

        private static ECCurve CloneECCurve(ECCurve curve)
        {
            return new ECCurve
            {
                Prime = CloneBuffer(curve.Prime),
                A = CloneBuffer(curve.A),
                B = CloneBuffer(curve.B),
                Order = CloneBuffer(curve.Order),
                Cofactor = CloneBuffer(curve.Cofactor),
                G = CloneECPoint(curve.G),
            };
        }

        private static ECPoint CloneECPoint(ECPoint point)
        {
            return new ECPoint
            {
                X = CloneBuffer(point.X),
                Y = CloneBuffer(point.Y)
            };
        }

        private static BigInteger Normalize(BigInteger value, BigInteger order)
            => value >= BigInteger.Zero ? value : value + order;

        private struct BigIntegerPoint
        {
            private readonly BigInteger _modulus;

            public BigInteger X { get; private set; }

            public BigInteger Y { get; private set; }

            public BigIntegerPoint(ECPoint point, BigInteger modulus)
            {
                _modulus = modulus;
                X = Normalize(new BigInteger(point.X), modulus);
                Y = Normalize(new BigInteger(point.Y), modulus);
            }

            public ECPoint ToECPoint(int keySize)
            {
                int size = keySize / 8;

                return new ECPoint
                {
                    X = ToNormalizedByteArray(X, size),
                    Y = ToNormalizedByteArray(Y, size),
                };
            }

            public static BigIntegerPoint Add(BigIntegerPoint left, BigIntegerPoint right, BigInteger prime)
            {
                BigInteger
                    dy = Normalize(right.Y - left.Y, prime),
                    dx = Normalize(right.X - left.X, prime),
                    lambda = Normalize((dy * BigInteger.ModPow(dx, prime - s_two, prime)) % prime, prime),
                    x = Normalize((BigInteger.Pow(lambda, 2) - left.X - right.X) % prime, prime);

                return new BigIntegerPoint()
                {
                    X = x,
                    Y = Normalize((lambda * (left.X - x) - left.Y) % prime, prime),
                };
            }

            private static BigIntegerPoint MultipleTwo(BigIntegerPoint value, BigInteger prime, BigInteger a)
            {
                BigInteger
                    dy = Normalize(s_three * BigInteger.Pow(value.X, 2) + a, prime),
                    dx = Normalize(s_two * value.Y, prime),
                    lambda = (dy * BigInteger.ModPow(dx, prime - s_two, prime)) % prime,
                    x = Normalize((BigInteger.Pow(lambda, 2) - s_two * value.X) % prime, prime);

                return new BigIntegerPoint
                {
                    X = x,
                    Y = Normalize((lambda * (value.X - x) - value.Y) % prime, prime)
                };
            }

            public static BigIntegerPoint Multiply(BigIntegerPoint point, BigInteger multiplier, BigInteger prime, BigInteger a)
            {
                BigIntegerPoint result = point;
                multiplier--;

                while (multiplier > BigInteger.Zero)
                {
                    if ((multiplier % s_two) != BigInteger.Zero)
                    {
                        if ((result.X == point.X) && (result.Y == point.Y))
                            result = MultipleTwo(result, prime, a);
                        else
                            result = Add(result, point, prime);
                        multiplier--;
                    }

                    multiplier /= s_two;
                    point = MultipleTwo(point, prime, a);
                }

                return result;
            }
        }
    }
}
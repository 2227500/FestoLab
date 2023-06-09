﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Xml.Linq;
using ThreeMf.Extensions;

namespace ThreeMf
{
    public class ThreeMfTexture2DCoordinate : IThreeMfPropertyItem
    {
        private const string UAttributeName = "u";
        private const string VAttributeName = "v";

        internal static XName Texture2DCoordinateName = XName.Get("tex2coord", ThreeMfModel.MaterialNamespace);

        public double U { get; set; }
        public double V { get; set; }

        public ThreeMfTexture2DCoordinate(double u, double v)
        {
            U = u;
            V = v;
        }

        internal XElement ToXElement()
        {
            return new XElement(Texture2DCoordinateName,
                new XAttribute(UAttributeName, U),
                new XAttribute(VAttributeName, V));
        }

        internal static ThreeMfTexture2DCoordinate ParseCoordinate(XElement element)
        {
            var u = element.AttributeDoubleValueOrThrow(UAttributeName);
            var v = element.AttributeDoubleValueOrThrow(VAttributeName);
            return new ThreeMfTexture2DCoordinate(u, v);
        }
    }
}

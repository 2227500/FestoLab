﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Xml.Linq;
using ThreeMf.Extensions;

namespace ThreeMf
{
    public class ThreeMfColor : IThreeMfPropertyItem
    {
        private const string ColorAttributeName = "color";

        internal static XName ColorName = XName.Get("color", ThreeMfModel.MaterialNamespace);

        public ThreeMfsRGBColor Color { get; set; }

        public ThreeMfColor(ThreeMfsRGBColor color)
        {
            Color = color;
        }

        internal XElement ToXElement()
        {
            return new XElement(ColorName,
                new XAttribute(ColorAttributeName, Color.ToString()));
        }

        internal static ThreeMfColor ParseColor(XElement element)
        {
            var color = ThreeMfsRGBColor.Parse(element.AttributeValueOrThrow(ColorAttributeName));
            return new ThreeMfColor(color);
        }
    }
}

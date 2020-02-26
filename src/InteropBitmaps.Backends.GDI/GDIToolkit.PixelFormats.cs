﻿using System;
using System.Collections.Generic;
using System.Text;

using GDIFMT = System.Drawing.Imaging.PixelFormat;
using ELEMENT = InteropBitmaps.ComponentFormat;

namespace InteropBitmaps
{
    partial class GDIToolkit
    {
        public static PixelFormat ToInteropPixelFormat(this GDIFMT fmt)
        {
            switch (fmt)
            {
                case GDIFMT.Format8bppIndexed: return PixelFormat.Standard.GRAY8;

                case GDIFMT.Format16bppRgb565: return PixelFormat.Standard.BGR565;
                case GDIFMT.Format16bppRgb555: return PixelFormat.Standard.BGRA5551;
                case GDIFMT.Format16bppArgb1555: return PixelFormat.Standard.BGRA5551;

                case GDIFMT.Format24bppRgb: return PixelFormat.Standard.RGB24;

                case GDIFMT.Format32bppRgb: return PixelFormat.Standard.RGBA32;
                case GDIFMT.Format32bppArgb: return PixelFormat.Standard.ARGB32;

                case GDIFMT.PAlpha:
                case GDIFMT.Format32bppPArgb:
                case GDIFMT.Format64bppPArgb:
                    throw new NotSupportedException($"Premultiplied {fmt} not supported.");

                default: throw new NotImplementedException(fmt.ToString());
            }
        }

        public static int GetPixelSize(this GDIFMT fmt)
        {
            switch (fmt)
            {
                case GDIFMT.Format8bppIndexed: return 1;

                case GDIFMT.Format16bppArgb1555: return 2;
                case GDIFMT.Format16bppGrayScale: return 2;
                case GDIFMT.Format16bppRgb555: return 2;
                case GDIFMT.Format16bppRgb565: return 2;

                case GDIFMT.Format24bppRgb: return 3;

                case GDIFMT.Format32bppRgb: return 4;
                case GDIFMT.Format32bppArgb: return 4;
                case GDIFMT.Format32bppPArgb: return 4;

                case GDIFMT.Format48bppRgb: return 6;

                case GDIFMT.Format64bppArgb: return 8;
                case GDIFMT.Format64bppPArgb: return 8;

                default: throw new NotImplementedException();
            }
        }        
    }
}

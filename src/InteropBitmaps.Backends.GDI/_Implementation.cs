﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using GDIFMT = System.Drawing.Imaging.PixelFormat;
using INTEROPFMT = InteropBitmaps.Pixel.Format;

namespace InteropBitmaps
{
    static class _Implementation
    {
        #region pixel formats

        public static INTEROPFMT ToPixelFormat(GDIFMT fmt)
        {
            switch (fmt)
            {
                case GDIFMT.Format8bppIndexed: return Pixel.Luminance8.Format;

                case GDIFMT.Format16bppGrayScale: return Pixel.Luminance16.Format;
                case GDIFMT.Format16bppRgb565: return Pixel.BGR565.Format;
                case GDIFMT.Format16bppRgb555: return Pixel.BGRA5551.Format;
                case GDIFMT.Format16bppArgb1555: return Pixel.BGRA5551.Format;

                case GDIFMT.Format24bppRgb: return Pixel.BGR24.Format;

                case GDIFMT.Format32bppRgb: return Pixel.BGRA32.Format;
                case GDIFMT.Format32bppArgb: return Pixel.BGRA32.Format;

                case GDIFMT.PAlpha:
                case GDIFMT.Format32bppPArgb:
                case GDIFMT.Format64bppPArgb:
                    throw new NotSupportedException($"Premultiplied {fmt} not supported.");

                default: throw new NotImplementedException(fmt.ToString());
            }
        }

        public static GDIFMT ToPixelFormat(INTEROPFMT fmt, bool allowCompatibleFormats)
        {
            switch (fmt)
            {
                case Pixel.Luminance16.Code: return GDIFMT.Format16bppGrayScale;

                case Pixel.BGR565.Code: return GDIFMT.Format16bppRgb565;
                case Pixel.BGRA5551.Code: return GDIFMT.Format16bppArgb1555;

                case Pixel.BGR24.Code: return GDIFMT.Format24bppRgb;

                case Pixel.BGRA32.Code: return GDIFMT.Format32bppArgb;                    
            }

            if (allowCompatibleFormats)
            {
                switch (fmt)
                {
                    case Pixel.Luminance8.Code: // return GDIFMT.Format16bppGrayScale;                   

                    case Pixel.RGB24.Code: return GDIFMT.Format24bppRgb;

                    case Pixel.RGBA32.Code: return GDIFMT.Format32bppArgb;
                    case Pixel.ARGB32.Code: return GDIFMT.Format32bppArgb;

                    default: throw new NotImplementedException(fmt.ToString());
                }
            }

            return GDIFMT.Undefined;
        }        

        #endregion

        #region API

        public static BitmapInfo GetBitmapInfo(System.Drawing.Imaging.BitmapData bits)
        {
            var fmt = ToPixelFormat(bits.PixelFormat);
            return new BitmapInfo(bits.Width, bits.Height, fmt, bits.Stride);
        }

        public static Bitmap WrapAsGDIBitmap(PointerBitmap src)
        {
            if (src.IsEmpty) return null;
            var fmt = ToPixelFormat(src.Info.PixelFormat, false);
            if (fmt == GDIFMT.Undefined) throw new ArgumentException($"Invalid format {src.Info.PixelFormat}", nameof(src));

            if ((src.Info.StepByteSize & 3) != 0) throw new ArgumentException($"Must be multiple of 4 but found {src.Info.StepByteSize}", nameof(src.Info.StepByteSize));

            return new Bitmap(src.Info.Width, src.Info.Height, src.Info.StepByteSize, fmt, src.Pointer);
        }

        public static Bitmap CloneToGDIBitmap(SpanBitmap src, bool allowCompatibleFormats, GDIFMT? fmtOverride = null)
        {
            if (!fmtOverride.HasValue)
            {
                fmtOverride = ToPixelFormat(src.PixelFormat, allowCompatibleFormats);
                if (fmtOverride.Value == GDIFMT.Undefined) throw new ArgumentException(nameof(src));
            }

            var dst = new Bitmap(src.Width, src.Height, fmtOverride.Value);

            dst.SetPixels(0, 0, src);

            return dst;
        }

        public static MemoryBitmap CloneToMemoryBitmap(Image img, INTEROPFMT? fmtOverride = null)
        {
            using (var bmp = new Bitmap(img))
            {
                return CloneToMemoryBitmap(bmp, fmtOverride);
            }
        }

        public static MemoryBitmap CloneToMemoryBitmap(Bitmap bmp, INTEROPFMT? fmtOverride = null)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            System.Drawing.Imaging.BitmapData bits = null;

            try
            {
                bits = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                var span = bits.AsSpanBitmap();

                return fmtOverride.HasValue ? span.ToMemoryBitmap(fmtOverride.Value) : span.ToMemoryBitmap();
            }
            finally
            {
                if (bits != null) bmp.UnlockBits(bits);
            }
        }

        public static void Mutate(Bitmap bmp, Action<PointerBitmap> action)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            System.Drawing.Imaging.BitmapData bits = null;

            try
            {
                bits = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

                action(bits.AsPointerBitmap());
            }
            finally
            {
                bmp.UnlockBits(bits);
            }
        }

        public static void SetPixels(Bitmap dst, int dstx, int dsty, in SpanBitmap src)
        {
            var rect = new Rectangle(0, 0, dst.Width, dst.Height);

            System.Drawing.Imaging.BitmapData dstbits = null;

            try
            {
                dstbits = dst.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, dst.PixelFormat);

                dstbits.AsSpanBitmap().SetPixels(dstx, dsty, src);
            }
            finally
            {
                dst.UnlockBits(dstbits);
            }
        }        

        #endregion
    }
}

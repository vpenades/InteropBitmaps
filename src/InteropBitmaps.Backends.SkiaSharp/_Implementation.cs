﻿using System;
using System.Collections.Generic;
using System.Text;

using INTEROPFMT = InteropBitmaps.Pixel.Format;
using SKIACOLOR = SkiaSharp.SKColorType;
using SKIAALPHA = SkiaSharp.SKAlphaType;

namespace InteropBitmaps
{
    static class _Implementation
    {
        #region pixel format

        public static (SKIACOLOR Color, SKIAALPHA Alpha) ToPixelFormat(INTEROPFMT fmt, bool allowCompatibleFormats = false)
        {
            switch (fmt)
            {
                case Pixel.Alpha8.Code: return (SKIACOLOR.Alpha8, SKIAALPHA.Opaque);
                case Pixel.Luminance8.Code: return (SKIACOLOR.Gray8, SKIAALPHA.Opaque);

                case Pixel.BGR565.Code: return (SKIACOLOR.Rgb565, SKIAALPHA.Opaque);
                
                case Pixel.RGBA32.Code: return (SKIACOLOR.Rgba8888, SKIAALPHA.Unpremul);
                case Pixel.BGRA32.Code: return (SKIACOLOR.Bgra8888, SKIAALPHA.Unpremul);
            }

            if (allowCompatibleFormats)
            {
                switch (fmt)
                {
                    case Pixel.BGR24.Code: return (SKIACOLOR.Rgb888x, SKIAALPHA.Opaque);
                    case Pixel.RGB24.Code: return (SKIACOLOR.Rgb888x, SKIAALPHA.Opaque);
                    case Pixel.Luminance16.Code: return (SKIACOLOR.Gray8, SKIAALPHA.Opaque);
                }
            }

            return (SKIACOLOR.Unknown, SKIAALPHA.Unknown);
        }

        private static INTEROPFMT ToPixelFormat(SKIACOLOR color, bool allowCompatibleFormats = false)
        {
            switch (color)
            {
                case SKIACOLOR.Alpha8: return Pixel.Alpha8.Format;
                case SKIACOLOR.Gray8: return Pixel.Luminance8.Format;
                case SKIACOLOR.Rgba8888: return Pixel.RGBA32.Format;
                case SKIACOLOR.Rgb888x: return Pixel.RGBA32.Format;
                case SKIACOLOR.Bgra8888: return Pixel.BGRA32.Format;
            }

            if (allowCompatibleFormats)
            {
                switch(color)
                {
                    case SKIACOLOR.Argb4444: return Pixel.BGRA4444.Format;
                    case SKIACOLOR.Rgb565: return Pixel.BGR565.Format;
                    case SKIACOLOR.Rgb888x: return Pixel.RGB24.Format;
                }
            }

            throw new NotSupportedException();
        }

        public static INTEROPFMT ToPixelFormat(SKIACOLOR color, SKIAALPHA alpha, bool allowCompatibleFormats = false)
        {
            switch(alpha)
            {
                case SKIAALPHA.Opaque: return ToPixelFormat(color, allowCompatibleFormats);
                case SKIAALPHA.Unpremul: return ToPixelFormat(color, allowCompatibleFormats);
            }
            throw new NotImplementedException();
        }

        #endregion

        #region Skia => Interop

        public static BitmapInfo ToBitmapInfo(SkiaSharp.SKImageInfo info, int rowBytes = 0)
        {
            var fmt = ToPixelFormat(info.ColorType, info.AlphaType);
            return new BitmapInfo(info.Width, info.Height, fmt, Math.Max(info.RowBytes, rowBytes));
        }

        public static PointerBitmap AsPointerBitmap(SkiaSharp.SKPixmap map)
        {
            var ptr = map.GetPixels();
            if (ptr == IntPtr.Zero) throw new ArgumentNullException();

            var binfo = ToBitmapInfo(map.Info, map.RowBytes);

            return new PointerBitmap(ptr, binfo);
        }

        public static SpanBitmap AsSpanBitmap(SkiaSharp.SKPixmap map, bool readOnly = false)
        {
            var binfo = ToBitmapInfo(map.Info, map.RowBytes);

            if (readOnly) return new SpanBitmap(map.GetPixelSpan(), binfo);

            var ptr = map.GetPixels();
            if (ptr == IntPtr.Zero) throw new ArgumentNullException();

            return new SpanBitmap(ptr, binfo);
        }        

        public static SpanBitmap AsSpanBitmap(SkiaSharp.SKBitmap bmp, bool readOnly = false)
        {
            var binfo = ToBitmapInfo(bmp.Info, bmp.RowBytes);

            if (readOnly) return new SpanBitmap(bmp.GetPixelSpan(), binfo);

            var ptr = bmp.GetPixels();
            if (ptr == IntPtr.Zero) throw new ArgumentNullException();

            return new SpanBitmap(ptr, binfo);

            // should call bmp.NotifyPixelsChanged(); afterwards
        }

        #endregion

        #region Interop => Skia

        public static SkiaSharp.SKImageInfo ToSkia(BitmapInfo binfo, bool allowCompatibleFormats = false)
        {
            var (color, alpha) = ToPixelFormat(binfo.PixelFormat, allowCompatibleFormats);
            if (color == SKIACOLOR.Unknown) throw new ArgumentException(nameof(binfo));
            if (alpha == SKIAALPHA.Unknown) throw new ArgumentException(nameof(binfo));

            return new SkiaSharp.SKImageInfo(binfo.Width, binfo.Height, color, alpha);
        }
        
        public static SkiaSharp.SKBitmap ToSKBitmap(SpanBitmap bmp)
        {
            var (color, alpha) = ToPixelFormat(bmp.PixelFormat);
            var img = new SkiaSharp.SKBitmap(bmp.Width, bmp.Height, color, alpha);

            var binfo = ToBitmapInfo(img.Info, img.RowBytes);

            var ptr = img.GetPixels();
            if (ptr == IntPtr.Zero) throw new ArgumentNullException();            

            var dst = new SpanBitmap(ptr, binfo);

            dst.SetPixels(0, 0, bmp);

            img.NotifyPixelsChanged();

            return img;
        }

        public static SkiaSharp.SKImage ToSKImage(SpanBitmap bmp, (SKIACOLOR Color, SKIAALPHA Alpha)? fmtOverride = null)
        {            
            var skinfo = fmtOverride.HasValue
                ?
                new SkiaSharp.SKImageInfo(bmp.Width,bmp.Height,fmtOverride.Value.Color,fmtOverride.Value.Alpha)
                :
                ToSkia(bmp.Info, true);

            var img = SkiaSharp.SKImage.Create(skinfo);

            var pix = img.PeekPixels();

            AsSpanBitmap(pix).SetPixels(0,0,bmp);

            return img;            
        }

        public static SkiaSharp.SKImage ToSKImage(PointerBitmap bmp)
        {
            var skinfo = ToSkia(bmp.Info, false);

            return SkiaSharp.SKImage.FromPixelCopy(skinfo, bmp.Pointer, bmp.Info.StepByteSize);
        }

        public static SkiaSharp.SKImage AsSKImage(PointerBitmap bmp)
        {
            var skinfo = ToSkia(bmp.Info, false);

            return SkiaSharp.SKImage.FromPixels(skinfo, bmp.Pointer, bmp.Info.StepByteSize);
        }

        public static SkiaSharp.SKBitmap AsSKBitmap(PointerBitmap bmp)
        {
            var skinfo = ToSkia(bmp.Info, false);
            
            var dst = new SkiaSharp.SKBitmap();
            dst.InstallPixels(skinfo, bmp.Pointer, bmp.Info.StepByteSize);
            
            return dst;
        }

        #endregion
    }
}

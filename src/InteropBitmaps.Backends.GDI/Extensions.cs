﻿using System;
using System.Drawing;

using GDIICON = System.Drawing.Icon;
using GDIIMAGE = System.Drawing.Image;
using GDIBITMAP = System.Drawing.Bitmap;
using GDIPTR = System.Drawing.Imaging.BitmapData;

namespace InteropBitmaps
{
    /// <see href="https://github.com/dotnet/runtime/tree/master/src/libraries/System.Drawing.Common"/>
    public static partial class GDIToolkit
    {
        #region GDI facade

        public static Adapters.GDIFactory WithGDI(this BitmapInfo binfo) { return new Adapters.GDIFactory(binfo); }

        public static Adapters.GDISpanAdapter WithGDI(this SpanBitmap bmp) { return new Adapters.GDISpanAdapter(bmp); }        

        public static Adapters.GDISpanAdapter WithGDI<TPixel>(this SpanBitmap<TPixel> bmp) where TPixel : unmanaged
        { return new Adapters.GDISpanAdapter(bmp.AsTypeless()); }
        
        public static Adapters.GDIMemoryAdapter UsingGDI(this MemoryBitmap bmp) { return new Adapters.GDIMemoryAdapter(bmp); }

        public static Adapters.GDIMemoryAdapter WithGDI<TPixel>(this MemoryBitmap<TPixel> bmp) where TPixel : unmanaged
        { return new Adapters.GDIMemoryAdapter(bmp); }        

        #endregion

        #region As MemoryBitmap

        public static PointerBitmap.ISource UsingPointerBitmap(this GDIBITMAP bmp)
        {
            return new Adapters.GDIMemoryManager(bmp);
        }

        public static MemoryBitmap.ISource UsingMemoryBitmap(this GDIBITMAP bmp)
        {
            return new Adapters.GDIMemoryManager(bmp);
        }

        #endregion

        #region As SpanBitmap

        public static BitmapInfo GetBitmapInfo(this GDIPTR data)
        {
            var binfo = _Implementation.GetBitmapInfo(data);
            System.Diagnostics.Debug.Assert(binfo.StepByteSize == data.Stride);
            return binfo;
        }

        public static PointerBitmap AsPointerBitmap(this GDIPTR data)
        {
            var info = _Implementation.GetBitmapInfo(data);
            System.Diagnostics.Debug.Assert(info.StepByteSize == data.Stride);
            return new PointerBitmap(data.Scan0, info);
        }

        public static SpanBitmap AsSpanBitmap(this GDIPTR data)
        {
            return data.AsPointerBitmap();
        }

        public static SpanBitmap<TPixel> AsSpanBitmap<TPixel>(this GDIPTR data)
            where TPixel: unmanaged
        {
            return data.AsPointerBitmap().AsSpanBitmapOfType<TPixel>();
        }

        #endregion        

        #region Generic API
        
        public static void Mutate(this GDIBITMAP bmp, Action<PointerBitmap> action)
        {
            _Implementation.Mutate(bmp, action);
        }

        public static void SetPixels(this GDIBITMAP dst, int dstx, int dsty, in SpanBitmap src)
        {
            _Implementation.TransferSpan(src, dst, (s, d) => d.SetPixels(dstx, dsty, s));
        }

        public static void SetPixels(this SpanBitmap dst, int dstx, int dsty, in GDIBITMAP src)
        {
            _Implementation.TransferSpan(src, dst, (s, d) => d.SetPixels(dstx, dsty, s));
        }

        public static void SetPixels(this GDIBITMAP dst, System.Numerics.Matrix3x2 dstSRT, in SpanBitmap src)
        {
            _Implementation.TransferSpan(src, dst, (s, d) => d.SetPixels(dstSRT, s));
        }

        public static void SetPixels(this SpanBitmap dst, System.Numerics.Matrix3x2 dstSRT, in GDIBITMAP src)
        {
            _Implementation.TransferSpan(src, dst, (s, d) => d.SetPixels(dstSRT, s));
        }

        public static void FitPixels(this GDIBITMAP dst, in SpanBitmap src)
        {
            _Implementation.TransferSpan(src, dst, (s, d) => d.FitPixels(s));
        }

        public static void FitPixels(this SpanBitmap dst, in GDIBITMAP src)
        {
            _Implementation.TransferSpan(src, dst, (s, d) => d.FitPixels(s));
        }

        public static bool CopyTo(this GDIBITMAP src, ref MemoryBitmap dst, Pixel.Format? fmtOverride = null)
        {
            if (src == null)
            {
                // if both are empty, exit with no changes
                if (dst.IsEmpty) return false;

                // if src is empty, clear dst
                dst = default; return true;
            }

            var refreshed = _Implementation.Reshape(ref dst, src, fmtOverride);
            dst.AsSpanBitmap().SetPixels(0, 0, src);
            return refreshed;
        }

        public static bool CopyTo(this GDIPTR src, ref MemoryBitmap dst, Pixel.Format? fmtOverride = null)
        {
            if (src == null)
            {
                // if both are empty, exit with no changes
                if (dst.IsEmpty) return false;

                // if src is empty, clear dst
                dst = default; return true;
            }

            var refreshed = _Implementation.Reshape(ref dst, src, fmtOverride);
            dst.SetPixels(0, 0, src.AsSpanBitmap());
            return refreshed;
        }

        #endregion

        #region Specific API

        public static MemoryBitmap ToMemoryBitmap(this TextureBrush brush, Pixel.Format? fmtOverride = null)
        {
            return brush.Image.ToMemoryBitmap(fmtOverride);
        }

        public static MemoryBitmap ToMemoryBitmap(this GDIIMAGE img, Pixel.Format? fmtOverride = null)
        {
            using (var bmp = new GDIBITMAP(img))
            {
                return bmp.ToMemoryBitmap(fmtOverride);
            }
        }

        public static MemoryBitmap ToMemoryBitmap(this GDIICON icon, Pixel.Format? fmtOverride = null)
        {
            using (var bmp = icon.ToBitmap())
            {
                return bmp.ToMemoryBitmap(fmtOverride);
            }
        }

        public static MemoryBitmap ToMemoryBitmap(this GDIBITMAP bmp, Pixel.Format? fmtOverride = null)
        {
            MemoryBitmap dst = default;
            return CopyTo(bmp, ref dst, fmtOverride) ? dst : throw new ArgumentException(nameof(bmp));
        }

        public static GDIBITMAP UsingAsGDIBitmap(this PointerBitmap bmp)
        {
            if (_Implementation.TryWrap(bmp, out var dst, out var err)) return dst;
            throw new ArgumentException(err, nameof(bmp));
        }

        public static GDIBITMAP ToGDIBitmap(this MemoryBitmap bmp)
        {
            return _Implementation.CloneAsGDIBitmap(bmp.AsSpanBitmap());
        }


        #endregion
    }
}

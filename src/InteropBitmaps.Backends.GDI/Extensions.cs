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

        public static IPointerBitmapOwner UsingPointerBitmap(this Bitmap bmp)
        {
            return new Adapters.GDIMemoryManager(bmp);
        }

        public static IMemoryBitmapOwner UsingMemoryBitmap(this Bitmap bmp)
        {
            return new Adapters.GDIMemoryManager(bmp);
        }

        #endregion

        #region As SpanBitmap

        public static BitmapInfo GetBitmapInfo(this GDIPTR data)
        {
            return _Implementation.GetBitmapInfo(data);
        }

        public static PointerBitmap AsPointerBitmap(this GDIPTR data)
        {
            var info = _Implementation.GetBitmapInfo(data);
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

        #region API
        
        public static void Mutate(this GDIBITMAP bmp, Action<PointerBitmap> action)
        {
            _Implementation.Mutate(bmp, action);
        }

        public static void SetPixels(this GDIBITMAP dst, int dstx, int dsty, in SpanBitmap src)
        {
            _Implementation.SetPixels(dst, dstx, dsty, src);
        }        

        public static MemoryBitmap ToMemoryBitmap(this GDIBITMAP bmp, Pixel.Format? fmtOverride = null)
        {
            return _Implementation.CloneToMemoryBitmap(bmp, fmtOverride);
        }        

        public static MemoryBitmap ToMemoryBitmap(this GDIIMAGE img, Pixel.Format? fmtOverride = null)
        {
            using (var bmp = new Bitmap(img))
            {
                return _Implementation.CloneToMemoryBitmap(bmp, fmtOverride);
            }
        }

        public static MemoryBitmap ToMemoryBitmap(this TextureBrush brush, Pixel.Format? fmtOverride = null)
        {
            return ToMemoryBitmap(brush.Image, fmtOverride);
        }        

        public static MemoryBitmap ToMemoryBitmap(this GDIICON icon, Pixel.Format? fmtOverride = null)
        {
            using (var bmp = icon.ToBitmap())
            {
                return _Implementation.CloneToMemoryBitmap(bmp, fmtOverride);
            }
        }

        public static GDIBITMAP UsingAsGDIBitmap(this PointerBitmap bmp)
        {
            return _Implementation.WrapAsGDIBitmap(bmp);
        }

        public static GDIBITMAP ToGDIBitmap(this MemoryBitmap bmp)
        {
            return _Implementation.CloneToGDIBitmap(bmp.AsSpanBitmap(),true);
        }

        

        public static bool CopyTo(this GDIBITMAP src, ref MemoryBitmap dst, Pixel.Format? fmtOverride = null)
        {
            if (src == null)
            {
                if (dst.IsEmpty) return false;
                dst = default;
                return true;
            }

            if (dst.IsEmpty) { dst = src.ToMemoryBitmap(fmtOverride); return true; }

            var bits = src.LockBits(Rectangle.Empty, System.Drawing.Imaging.ImageLockMode.ReadOnly, src.PixelFormat);

            try { return bits.CopyTo(ref dst, fmtOverride); }
            finally { src.UnlockBits(bits); }
        }

        public static bool CopyTo(this GDIPTR src, ref MemoryBitmap dst, Pixel.Format? fmtOverride = null)
        {
            if (src == null)
            {
                if (dst.IsEmpty) return false;
                dst = default;
                return true;
            }
            
            var srcSpan = src.AsSpanBitmap();

            var srcInfo = srcSpan.Info;
            if (fmtOverride.HasValue) srcInfo = srcInfo.WithPixelFormat(fmtOverride.Value);

            bool refreshed = false;

            if (srcInfo != dst.Info)
            {
                dst = new MemoryBitmap(srcInfo);
                refreshed = true;
            }

            dst.SetPixels(0, 0, srcSpan);            

            return refreshed;
        }

        #endregion
    }
}

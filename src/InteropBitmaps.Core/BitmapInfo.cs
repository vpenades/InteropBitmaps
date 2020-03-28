﻿using System;
using System.Collections.Generic;
using System.Text;

namespace InteropBitmaps
{
    [System.Diagnostics.DebuggerDisplay("{Width}x{Height}-{PixelFormat}")]
    public readonly struct BitmapInfo : IEquatable<BitmapInfo>
    {
        #region lifecycle        

        public BitmapInfo(int width, int height, PixelFormat pixelFormat, int scanlineSize = 0)
        {
            var pixelSize = pixelFormat.ByteCount;

            Guard.BitmapRect(width, height, pixelSize, scanlineSize);

            Width = width;
            Height = height;
            PixelByteSize = pixelSize;
            PixelFormat = pixelFormat;
            ScanlineByteSize = scanlineSize > 0 ? scanlineSize : width * pixelSize;
        }

        private BitmapInfo(int width, int height, in BitmapInfo other)
        {
            Width = width;
            Height = height;
            PixelByteSize = other.PixelByteSize;
            PixelFormat = other.PixelFormat;
            ScanlineByteSize = other.ScanlineByteSize;
        }

        #endregion

        #region data

        public readonly int Width;
        public readonly int Height;
        public readonly int PixelByteSize;
        public readonly int ScanlineByteSize;
        public readonly PixelFormat PixelFormat;

        public override int GetHashCode() { return Width.GetHashCode() ^ Height.GetHashCode() ^ PixelFormat.GetHashCode(); }

        public static bool AreEqual(in BitmapInfo a, in BitmapInfo b)
        {
            if (a.Width != b.Width) return false;
            if (a.Height != b.Height) return false;
            if (a.PixelByteSize != b.PixelByteSize) return false;            
            if (a.PixelFormat != b.PixelFormat) return false;
            if (a.ScanlineByteSize != b.ScanlineByteSize) return false;
            return true;
        }

        public bool Equals(BitmapInfo other) { return AreEqual(this, other); }

        public override bool Equals(object obj) { return obj is BitmapInfo other ? AreEqual(this, other) : false; }

        public static bool operator ==(in BitmapInfo a, in BitmapInfo b) { return AreEqual(a, b); }

        public static bool operator !=(in BitmapInfo a, in BitmapInfo b) { return !AreEqual(a, b); }

        #endregion

        #region properties

        public bool IsEmpty => (Width * Height * PixelByteSize) == 0;

        // under some circunstances, the last stride of the last row must not be accounted for.
        public int BitmapByteSize => Height == 0 ? 0 : ScanlineByteSize * (Height - 1) + PixelByteSize * Width;

        public (int Width, int Height) Size => (Width, Height);

        public BitmapBounds Bounds => new BitmapBounds(0,0,Width, Height);

        /// <summary>
        /// Gets a value indicating whether the buffer can be accessed continuously.
        /// </summary>
        public bool IsContinuous => Width * PixelByteSize == ScanlineByteSize;

        #endregion

        #region data

        public (int Offset, BitmapInfo Info) Slice(in BitmapBounds rect)
        {
            Guard.IsTrue(nameof(rect), Bounds.Contains(rect));

            var offset = this.ScanlineByteSize * rect.X + this.PixelByteSize * rect.Y;

            // todo: if (Rect.X &1 ^ Rect.Y &1) == 1, pixel format must call SwitchScanlineFormatOrder(

            var info = new BitmapInfo(rect.Width, rect.Height, this);

            return (offset, info);
        }

        public Span<Byte> UseScanline(Span<Byte> bitmap, int y)
        {
            return bitmap.Slice(y * ScanlineByteSize, Width * PixelByteSize);
        }

        public ReadOnlySpan<Byte> GetScanline(ReadOnlySpan<Byte> bitmap, int y)
        {
            return bitmap.Slice(y * ScanlineByteSize, Width * PixelByteSize);
        }

        public Span<Byte> UsePixel(Span<Byte> bitmap, int x, int y)
        {
            return bitmap.Slice(y * ScanlineByteSize + x * PixelByteSize, PixelByteSize);
        }

        public Span<Byte> UsePixels(Span<Byte> bitmap, int x, int y, int pixelCount)
        {
            return bitmap.Slice(y * ScanlineByteSize + x * PixelByteSize, PixelByteSize * pixelCount);
        }

        public ReadOnlySpan<Byte> GetPixel(ReadOnlySpan<Byte> bitmap, int x, int y)
        {
            return bitmap.Slice(y * ScanlineByteSize + x * PixelByteSize, PixelByteSize);
        }

        public ReadOnlySpan<Byte> GetPixels(ReadOnlySpan<Byte> bitmap, int x, int y, int pixelCount)
        {
            return bitmap.Slice(y * ScanlineByteSize + x * PixelByteSize, PixelByteSize * pixelCount);
        }

        #endregion
    }
}

﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using SIZE = System.Drawing.Size;
using POINT = System.Drawing.Point;

namespace InteropBitmaps
{
    /// <summary>
    /// Represents a Bitmap wrapped around a <see cref="Span{Byte}"/>
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Info.ToDebuggerDisplayString(),nq}")]
    // [System.Diagnostics.DebuggerTypeProxy(typeof(Debug.SpanBitmapProxy<>))]
    public readonly ref struct SpanBitmap<TPixel> where TPixel : unmanaged
    {
        #region lifecycle        

        public unsafe SpanBitmap(IntPtr data, in BitmapInfo info, bool isReadOnly = false)
        {
            Guard.NotNull(nameof(data), data);
            Guard.IsValidPixelFormat<TPixel>(info);

            _Info = info;

            var span = new Span<Byte>(data.ToPointer(), info.BitmapByteSize);

            _Readable = span;
            _Writable = isReadOnly ? null : span;
        }

        internal SpanBitmap(Span<Byte> data, in BitmapInfo info)
        {
            Guard.IsValidPixelFormat<TPixel>(info);
            _Info = info;
            _Readable = _Writable = data.Slice(0, _Info.BitmapByteSize);
        }

        internal SpanBitmap(ReadOnlySpan<Byte> data, in BitmapInfo info)
        {
            Guard.IsValidPixelFormat<TPixel>(info);
            _Info = info;
            _Readable = data.Slice(0, _Info.BitmapByteSize);
            _Writable = null;
        }

        public unsafe SpanBitmap(Span<Byte> data, int width, int height, Pixel.Format pixelFormat, int scanlineSize = 0)
        {
            _Info = new BitmapInfo(width, height, pixelFormat, scanlineSize);
            Guard.IsValidPixelFormat<TPixel>(_Info);
            _Readable = _Writable = data.Slice(0, _Info.BitmapByteSize);
        }

        public unsafe SpanBitmap(Span<TPixel> data, int width, int height, Pixel.Format pixelFormat, int scanlineSize = 0)
        {
            var span = System.Runtime.InteropServices.MemoryMarshal.Cast<TPixel, Byte>(data);

            _Info = new BitmapInfo(width, height, pixelFormat, scanlineSize);
            Guard.IsValidPixelFormat<TPixel>(_Info);
            _Readable = _Writable = span.Slice(0, _Info.BitmapByteSize);
        }

        public unsafe SpanBitmap(ReadOnlySpan<Byte> data, int width, int height, Pixel.Format pixelFormat, int scanlineSize = 0)
        {
            _Info = new BitmapInfo(width, height, pixelFormat, scanlineSize);
            Guard.IsValidPixelFormat<TPixel>(_Info);
            _Readable = data.Slice(0, _Info.BitmapByteSize);
            _Writable = null;
        }

        #endregion

        #region data

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly BitmapInfo _Info;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly Span<Byte> _Writable;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly ReadOnlySpan<Byte> _Readable;

        public override int GetHashCode() { return _Implementation.CalculateHashCode(_Readable, _Info); }

        #endregion

        #region properties

        public Span<Byte> WritableBytes => _Writable;       

        public ReadOnlySpan<Byte> ReadableBytes => _Readable;        

        #endregion

        #region properties - Info

        /// <summary>
        /// Gets the layout information of the bitmap; Width, Height, PixelFormat, etc.
        /// </summary>
        public BitmapInfo Info => _Info;

        /// <summary>
        /// Gets the pixel format of the bitmap.
        /// </summary>
        public Pixel.Format PixelFormat => _Info.PixelFormat;

        /// <summary>
        /// Gets the size of the bitmap, in pixels.
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public SIZE Size => _Info.Size;

        /// <summary>
        /// Gets the width of the bitmap, in pixels.
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public int Width => _Info.Width;

        /// <summary>
        /// Gets the height of the bitmap, in pixels.
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public int Height => _Info.Height;

        /// <summary>
        /// Gets the size of a single pixel, in bytes.
        /// </summary>
        public int PixelByteSize => _Info.PixelByteSize;

        /// <summary>
        /// Gets the number of bytes required to jump from one row to the next, in bytes. This is also known as the ByteStride.
        /// </summary>
        public int StepByteSize => _Info.StepByteSize;

        /// <summary>
        /// Gets the bounds of the bitmap, in pixels.
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public BitmapBounds Bounds => _Info.Bounds;

        #endregion

        #region API - Buffers

        public SpanBitmap<TPixel> AsReadOnly() { return new SpanBitmap<TPixel>(_Readable, _Info); }
        public ReadOnlySpan<Byte> GetScanlineBytes(int y) { return _Info.GetScanlineBytes(_Readable, y); }        
        public ReadOnlySpan<TPixel> GetScanlinePixels(int y) { return _Info.GetScanlinePixels<TPixel>(_Readable, y); }        
        public Span<Byte> UseScanlineBytes(int y) { return _Info.UseScanlineBytes(_Writable, y); }        
        public Span<TPixel> UseScanlinePixels(int y) { return _Info.UseScanlinePixels<TPixel>(_Writable, y); }

        #endregion

        #region API - Cast

        public unsafe void PinWritablePointer(Action<PointerBitmap> onPin)
        {
            Guard.IsFalse(nameof(SpanBitmap), _Writable.IsEmpty);
            SpanBitmapImpl.PinWritablePointer(_Writable, _Info, onPin);
        }

        public unsafe void PinReadablePointer(Action<PointerBitmap> onPin)
        {
            Guard.IsFalse(nameof(SpanBitmap), _Readable.IsEmpty);
            SpanBitmapImpl.PinReadablePointer(_Readable, _Info, onPin);
        }

        public unsafe TResult PinReadablePointer<TResult>(Func<PointerBitmap, TResult> onPin)
        {
            Guard.IsFalse(nameof(SpanBitmap), _Readable.IsEmpty);
            return SpanBitmapImpl.PinReadablePointer(_Readable, _Info, onPin);
        }

        public static implicit operator SpanBitmap(SpanBitmap<TPixel> other)
        {
            return other._Writable.IsEmpty ? new SpanBitmap(other._Readable, other._Info) : new SpanBitmap(other._Writable, other._Info);
        }

        /// <summary>
        /// Returns a pixel typeless <see cref="SpanBitmap"/>.
        /// </summary>
        /// <returns>A <see cref="SpanBitmap"/></returns>
        /// <remarks>
        /// This is the opposite operation of <see cref="SpanBitmap.OfType{TPixel}"/>
        /// </remarks>
        public unsafe SpanBitmap AsTypeless()
        {
            return _Writable.IsEmpty ? new SpanBitmap(_Readable, _Info) : new SpanBitmap(_Writable, _Info);
        }
        
        public MemoryBitmap<TPixel> ToMemoryBitmap(Pixel.Format? fmtOverride = null)
        {
            if (!fmtOverride.HasValue) return new MemoryBitmap<TPixel>(_Readable.ToArray(), _Info);

            var dst = new MemoryBitmap<TPixel>(this.Width, this.Height, fmtOverride.Value);
            dst.SetPixels(0, 0, this);
            return dst;
        }

        #endregion

        #region API - Pixel Ops

        public SpanBitmap<TPixel> Slice(in BitmapBounds rect)
        {
            var (offset, info) = _Info.Slice(rect);

            if (_Writable.IsEmpty)
            {
                var span = _Readable.Slice(offset, info.BitmapByteSize);
                return new SpanBitmap<TPixel>(span, info);
            }
            else
            {
                var span = _Writable.Slice(offset, info.BitmapByteSize);
                return new SpanBitmap<TPixel>(span, info);
            }
        }        

        public TPixel GetPixel(int x, int y) { return GetScanlinePixels(y)[x]; }

        public void SetPixel(int x, int y, TPixel value) { UseScanlinePixels(y)[x] = value; }           

        public void SetPixels(TPixel value)
        {            
            Guard.IsTrue("this", !_Writable.IsEmpty);

            if (_Info.IsContinuous)
            {
                var dst = System.Runtime.InteropServices.MemoryMarshal.Cast<Byte, TPixel>(_Writable);
                dst.Fill(value);
                return;
            }

            for (int y = 0; y < _Info.Height; ++y)
            {
                var dst = UseScanlinePixels(y);
                dst.Fill(value);
            }
        }

        public void SetPixels(int dstX, int dstY, SpanBitmap<TPixel> src)
        {
            Guard.IsTrue("this", !_Writable.IsEmpty);

            _Implementation.CopyPixels(this, dstX, dstY, src);
        }

        public void FitPixels(SpanBitmap<TPixel> src)
        {
            // _Implementation.FitPixelsNearest(this, src);

            SpanBitmap.FitPixels(src, this, (0, 1));
        }

        /// <summary>
        /// Draws <paramref name="src"/> at the location defined by <paramref name="dstSRT"/>.
        /// </summary>
        /// <param name="dstSRT">Where to draw the image.</param>
        /// <param name="src">Image to draw.</param>
        /// <remarks>
        /// This is equivalent to OpenCV's WarpAffine
        /// </remarks>
        public void SetPixels(in Matrix3x2 dstSRT, SpanBitmap<TPixel> src)
        {
            // TODO: if dstSRT has no rotation, use _NearestResizeImplementation

            Matrix3x2.Invert(dstSRT, out var iform);
            Processing._BitmapTransformImplementation.SetPixelsNearest(this, src, iform);
        }

        public void ApplyPixels<TSrcPixel>(int dstX, int dstY, SpanBitmap<TSrcPixel> src, Func<TPixel,TSrcPixel,TPixel> pixelFunc)
            where TSrcPixel: unmanaged
        {
            _Implementation.ApplyPixels(this, dstX, dstY, src, pixelFunc);
        }

        public MemoryBitmap<TDstPixel> ToMemoryBitmap<TDstPixel>(Pixel.Format fmt, Converter<TPixel, TDstPixel> pixelConverter)
            where TDstPixel:unmanaged
        {
            var dst = new MemoryBitmap<TDstPixel>(this.Width, this.Height, fmt);
            dst.ApplyPixels(0, 0, this, (a, b) => pixelConverter(b));
            return dst;
        }

        public bool CopyTo(ref MemoryBitmap<TPixel> other)
        {
            var refreshed = false;

            if (!this.Info.Equals(other.Info))
            {
                other = new MemoryBitmap<TPixel>(this.Info);
                refreshed = true;
            }

            other.SetPixels(0, 0, this);

            return refreshed;
        }

        public bool CopyTo(ref BitmapInfo otherInfo, ref Byte[] otherData)
        {
            if (!this.Info.Equals(otherInfo)) otherInfo = this.Info;

            var refreshed = false;

            if (otherData == null || otherData.Length < otherInfo.BitmapByteSize)
            {
                otherData = new byte[this.Info.BitmapByteSize];
                refreshed = true;
            }

            new SpanBitmap(otherData, otherInfo).SetPixels(0, 0, this);

            return true;
        }

        public void ApplyMirror(bool horizontal, bool vertical, bool useMultiThreading = true)
        {
            Processing._MirrorImplementation.ApplyMirror(this, horizontal, vertical, useMultiThreading);
        }
        
        #endregion

        #region API - IO

        public void Write(System.IO.Stream stream, Codecs.CodecFormat format, params Codecs.IBitmapEncoder[] factory)
        {
            AsTypeless().Write(stream, format, factory);
        }

        public void Save(string filePath, params Codecs.IBitmapEncoder[] factory)
        {
            AsTypeless().Save(filePath, factory);
        }

        #endregion

        #region nested types

        public ref struct PixelEnumerator
        {
            private readonly SpanBitmap<TPixel> _Span;
            private ReadOnlySpan<TPixel> _Line;
            private int _IndexX;
            private int _IndexY;

            internal PixelEnumerator(SpanBitmap<TPixel> span)
            {
                _Span = span;
                _IndexX = -1;
                _IndexY = 0;
                _Line = span.GetScanlinePixels(_IndexY);
            }

            public bool MoveNext()
            {
                int x = _IndexX + 1;
                if (x < _Span.Width)
                {
                    _IndexX = x;
                    return true;
                }

                _IndexX = 0;

                int y = _IndexY + 1;
                if (y < _Span.Height)
                {
                    _IndexY = y;
                    _Line = _Span.GetScanlinePixels(_IndexY);
                    return true;
                }

                return false;
            }

            public ref readonly TPixel Current => ref _Line[_IndexX];
        }

        public ref struct RowEnumerator
        {
            private readonly SpanBitmap<TPixel> _Span;
            private ReadOnlySpan<TPixel> _Line;            
            private int _IndexY;

            internal RowEnumerator(SpanBitmap<TPixel> span)
            {
                _Span = span;
                _Line = default;
                _IndexY = -1;                
            }

            public bool MoveNext()
            {
                int y = _IndexY + 1;
                if (y < _Span.Height)
                {
                    _IndexY = y;
                    _Line = _Span.GetScanlinePixels(_IndexY);
                    return true;
                }

                return false;
            }

            public ReadOnlySpan<TPixel> Current => _Line;
        }

        #endregion
    }
}

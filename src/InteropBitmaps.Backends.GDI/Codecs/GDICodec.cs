﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InteropBitmaps.Codecs
{
    [System.Diagnostics.DebuggerDisplay("GDI Codec")]
    public sealed class GDICodec : IBitmapDecoder, IBitmapEncoder
    {
        #region lifecycle

        static GDICodec() { }

        private GDICodec() { }

        private static readonly GDICodec _Default = new GDICodec();

        public static GDICodec Default => _Default;

        #endregion

        #region API

        /// <inheritdoc/>
        public bool TryRead(Stream s, out MemoryBitmap bitmap)
        {
            try
            {
                using (var img = System.Drawing.Image.FromStream(s))
                {
                    bitmap = _Implementation.CloneToMemoryBitmap(img);
                }

                return true;
            }
            catch(System.ArgumentException)
            {
                bitmap = default;
                return false;
            }
        }

        /// <inheritdoc/>
        public bool TryWrite(Lazy<System.IO.Stream> stream, CodecFormat format, SpanBitmap bmp)
        {
            var fmt = GetFormatFromExtension(format);
            if (fmt == null) return false;

            var needsConversion = _Implementation.ToPixelFormat(bmp.PixelFormat, false) == System.Drawing.Imaging.PixelFormat.Undefined;

            if (needsConversion)
            {
                using (var tmp = _Implementation.CloneToGDIBitmap(bmp, true))
                {
                    tmp.Save(stream.Value, fmt);
                }
            }
            else
            {
                void _doSave(PointerBitmap ptr)
                {
                    using (var tmp = _Implementation.WrapOrCloneAsGDIBitmap(ptr))
                    {
                        tmp.Save(stream.Value, fmt);
                    }
                }

                bmp.PinReadablePointer(_doSave);
            }           

            return true;
        }

        private static System.Drawing.Imaging.ImageFormat GetFormatFromExtension(CodecFormat format)
        {
            switch(format)
            {
                case CodecFormat.Png: return System.Drawing.Imaging.ImageFormat.Png;
                case CodecFormat.Jpeg: return System.Drawing.Imaging.ImageFormat.Jpeg;

                case CodecFormat.Tiff: return System.Drawing.Imaging.ImageFormat.Tiff;
                case CodecFormat.Icon: return System.Drawing.Imaging.ImageFormat.Icon;

                case CodecFormat.Gif: return System.Drawing.Imaging.ImageFormat.Gif;
                case CodecFormat.Bmp: return System.Drawing.Imaging.ImageFormat.Bmp;
                case CodecFormat.Emf: return System.Drawing.Imaging.ImageFormat.Emf;
                case CodecFormat.Wmf: return System.Drawing.Imaging.ImageFormat.Wmf;
                default: return null;
            }            
        }

        #endregion
    }
}

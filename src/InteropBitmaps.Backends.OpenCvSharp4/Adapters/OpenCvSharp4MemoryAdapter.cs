﻿using System;
using System.Collections.Generic;
using System.Text;

namespace InteropBitmaps.Adapters
{
    
    public struct OpenCvSharp4MemoryAdapter : IDisposable
    {
        #region constructor
        public OpenCvSharp4MemoryAdapter(MemoryBitmap bmp)
        {
            _Handle = bmp.Memory.Pin();

            _SourcePointer = new PointerBitmap(_Handle.Value, bmp.Info);
            _SourceBitmap = bmp;

            _ProxyBitmap = _Implementation.WrapAsMat(_SourcePointer);            
        }

        public void Dispose()
        {
            if (_ProxyBitmap != null)
            {
                if (_ProxyBitmap.Data != _SourcePointer.Pointer)
                {
                    // the proxy content has changed, let's try to retrieve the data
                    throw new NotImplementedException();
                }
            }

            _ProxyBitmap?.Dispose();
            _ProxyBitmap = null;

            _Handle?.Dispose();
            _Handle = null;

            _SourceBitmap = default;
        }

        #endregion

        #region data

        private MemoryBitmap _SourceBitmap;
        private PointerBitmap _SourcePointer;

        private System.Buffers.MemoryHandle? _Handle;

        private OpenCvSharp.Mat _ProxyBitmap;        

        #endregion

        #region properties

        public OpenCvSharp.Mat Mat => _ProxyBitmap;        

        #endregion
    }
}

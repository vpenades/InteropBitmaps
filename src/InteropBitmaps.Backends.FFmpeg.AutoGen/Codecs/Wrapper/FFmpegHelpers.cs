﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using FFmpeg.AutoGen;

namespace InteropBitmaps.Codecs
{
    static class FFmpegHelper
    {
        static FFmpegHelper()
        {
            if (_RegisterInitialized) return;

            _RegisterInitialized = true;

            var path = Path.GetDirectoryName(typeof(FFmpegHelper).Assembly.Location);

            path = Path.Combine(path, "FFmpegAutoGen", Environment.Is64BitProcess ? "win-x64" : "win-x86");

            if (!Directory.Exists(path)) return;

            ffmpeg.RootPath = path;
        }

        private static bool _RegisterInitialized;

        public static bool Initialize() => _RegisterInitialized;

        

        public static unsafe string av_strerror(int error)
        {
            var bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
            return message;
        }

        public static int ThrowExceptionIfError(this int result)
        {
            if (result >= 0) return result;

            var errMsg = av_strerror(result);

            if (errMsg == "No such file or directory") throw new System.IO.FileNotFoundException();

            throw new ApplicationException(errMsg);            
        }
    }
}

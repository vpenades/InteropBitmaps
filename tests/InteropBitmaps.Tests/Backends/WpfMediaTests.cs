﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using NUnit.Framework;

namespace InteropBitmaps.Backends
{
    [Category("Backends")]
    public class WPFTests
    {
        [SetUp]
        public void SetUp()
        {
            Assert.AreEqual(8, IntPtr.Size, "x64 test environment required");
        }

        [TestCase("Resources\\diagram.jpg")]
        [TestCase("Resources\\white.png")]
        [TestCase("Resources\\shannon.jpg")]
        public void LoadImageWPF(string filePath)
        {
            filePath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, filePath);

            var uri = new Uri(filePath, UriKind.Absolute);

            var image = new System.Windows.Media.Imaging.BitmapImage(uri);            

            var memory = image.ToMemoryBitmap();

            memory.AttachToCurrentTest("Result.png");

            var writable = new System.Windows.Media.Imaging.WriteableBitmap(image);
        }

        [Test]
        public void TestCopyToCroppedBitmap()
        {
            CroppedBitmap dst = null;

            new MemoryBitmap<Byte>(256, 256, Pixel.Standard.Gray8)
                .AsSpanBitmap()
                .WithWPF()
                .CopyTo(ref dst);
            Assert.NotNull(dst);
            Assert.AreEqual(typeof(WriteableBitmap), dst.Source.GetType());
            Assert.AreEqual(256, dst.PixelWidth);
            Assert.AreEqual(256, dst.PixelHeight);

            var dstWrt = dst.Source as WriteableBitmap;            

            new MemoryBitmap<Byte>(128, 128, Pixel.Standard.Gray8)
                .AsSpanBitmap()
                .WithWPF()
                .CopyTo(ref dst);
            Assert.NotNull(dst);
            Assert.AreEqual(typeof(WriteableBitmap), dst.Source.GetType());
            Assert.AreEqual(128, dst.PixelWidth);
            Assert.AreEqual(128, dst.PixelHeight);
            Assert.IsTrue(object.ReferenceEquals(dstWrt, dst.Source), "CroppedBitmap must reuse WritableBitmap when shrinking");

            new MemoryBitmap<Byte>(512, 512, Pixel.Standard.Gray8)
                .AsSpanBitmap()
                .WithWPF()
                .CopyTo(ref dst);
            Assert.NotNull(dst);
            Assert.AreEqual(typeof(WriteableBitmap), dst.Source.GetType());
            Assert.AreEqual(512, dst.PixelWidth);
            Assert.AreEqual(512, dst.PixelHeight);
        }

        [Test]
        public void TestCopyWritableBitmap()
        {
            var src256 = new MemoryBitmap<Byte>(256, 256, Pixel.Standard.Gray8)
                .AsSpanBitmap()
                .WithWPF();            

            var src512 = new MemoryBitmap<Byte>(512, 512, Pixel.Standard.Gray8)
                .AsSpanBitmap()
                .WithWPF();           

            WriteableBitmap dst = null;

            bool allPixelsEqualTo(WriteableBitmap bmp, Byte pix)
            {
                return bmp.ToMemoryBitmap()
                    .OfType<Byte>()
                    .EnumeratePixels()
                    .All(item => item.Pixel == pix);
            }

            src256.Source.SetPixels<Byte>(100);
            src256.CopyTo(ref dst);
            Assert.NotNull(dst);
            Assert.AreEqual(typeof(WriteableBitmap), dst.GetType());
            Assert.AreEqual(256, dst.PixelWidth);
            Assert.AreEqual(256, dst.PixelHeight);
            Assert.IsTrue(allPixelsEqualTo(dst, 100));

            src256.Source.SetPixels<Byte>(105);
            src256.CopyTo(ref dst);
            Assert.IsTrue(allPixelsEqualTo(dst, 105));

            src512.Source.SetPixels<Byte>(130);
            src512.CopyTo(ref dst);
            Assert.NotNull(dst);
            Assert.AreEqual(typeof(WriteableBitmap), dst.GetType());
            Assert.AreEqual(512, dst.PixelWidth);
            Assert.AreEqual(512, dst.PixelHeight);
            Assert.IsTrue(allPixelsEqualTo(dst, 130));

            src512.Source.SetPixels<Byte>(135);
            src512.CopyTo(ref dst);
            Assert.IsTrue(allPixelsEqualTo(dst, 135));
        }
    }
}

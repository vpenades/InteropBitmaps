﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using InteropDrawing;
using System.Numerics;

namespace InteropBitmaps.Drawing
{
    public class DrawingTests
    {
        [Test]
        public void DrawLinesTest()
        {
            var bmp = new MemoryBitmap<Pixel.BGR24>(512,512);

            var dc = bmp.CreateDrawingContext();

            dc.DrawLine((2, 2), (30, 10), 1, System.Drawing.Color.Red);
            dc.DrawLine((2, 2), (30, 50), 1, System.Drawing.Color.Blue);

            dc.DrawLine((40, 60),(80,160), 9, LineStyle.Yellow.With(LineCapStyle.Round).WithOutline(System.Drawing.Color.Green,1));

            dc.DrawLine((180, 260),(800,300), 9, LineStyle.Yellow.With(LineCapStyle.Round).WithOutline(System.Drawing.Color.Green, 1));

            bmp.AttachToCurrentTest("result.png");
        }

        [Test]
        public void DrawingTest()
        {
            var bmp = new MemoryBitmap<Pixel.BGR24>(512, 512);

            var dc = bmp.CreateDrawingContext();

            dc.DrawConsoleFont((10, 10), "Hello World 0123456789-+/*", System.Drawing.Color.White);
            dc.DrawConsoleFont((10, 40), "abcdefghijklmnopqrstuvwxyz", System.Drawing.Color.White);
            dc.DrawConsoleFont((10, 70), "ABCDEFGHIJKLMNOPQRSTUVWXYZ", System.Drawing.Color.White);

            dc.DrawFont((10, 200), 1, "Abc123", FontStyle.White.With(3));

            dc.DrawFont(Matrix3x2.CreateRotation(1,new Vector2(10,350)), "Abc123", FontStyle.White.With(3));

            dc.DrawEllipse((200, 200), 50, 50, (System.Drawing.Color.Red, System.Drawing.Color.Blue,3));

            bmp.AttachToCurrentTest("result.png");
        }

        [Test]
        public void FillRuleTest()
        {
            int scale = 1;

            var bmp = new MemoryBitmap<Pixel.BGR24>(16 * scale, 8 * scale);

            for(int y=0; y < bmp.Height; ++y)
            {
                for (int x = 0; x < bmp.Width; ++x)
                {
                    var z = ((x / scale) & 1) ^ ((y / scale) & 1);
                    if (z == 1) bmp.SetPixel(x, y, Pixel.GetColor<Pixel.BGR24>(System.Drawing.Color.DarkGray));
                }
            }

            var dc = bmp.CreateDrawingContext();            

            foreach(var tri in _Triangle.GetFillRuleTriangles())
            {
                dc.DrawPolygon(System.Drawing.Color.Red, tri.A * scale, tri.B * scale, tri.C * scale);
            }            

            bmp.AttachToCurrentTest("result.png");
        }
    }
}

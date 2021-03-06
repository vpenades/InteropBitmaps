﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using InteropDrawing;

namespace InteropVision.With
{
    partial class ZXingCode : IDrawable2D
    {
        #region API

        public void DrawTo(IDrawing2D dc)
        {
            if (this.Results == null) return;

            var o = Offset;

            foreach (var r in this.Results) DrawTo(dc, r, o);
        }

        private static void DrawTo(IDrawing2D dc, ZXing.Result result, Point2 offset)
        {
            if (result == null) return;
            if (result.ResultPoints == null || result.ResultPoints.Length == 0) return;

            /* TODO: the ResultPoints interpretation depends on BarcodeFormat
            switch (Result.BarcodeFormat)
            {
                case ZXing.BarcodeFormat.AZTEC:                    
            }*/

            var points = result.ResultPoints
                .Select(item => new Point2(item.X, item.Y) + offset)
                .ToArray();

            dc.DrawPolygon((Color.Red, 4), points);

            var center = Point2.Centroid(points);

            dc.DrawFont(center, 0.4f, result?.Text ?? string.Empty, Color.Red);
        }        

        #endregion
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace InteropBitmaps
{
    /// <summary>
    /// Defines a rectangle.
    /// </summary>
    /// <remarks>
    /// The name of this structure is long and ugly on purpose, so it does not conflict
    /// with many other Rectangle definitions in other libraries:
    /// - System.Drawing.Rectangle (GDI)
    /// - System.Drawing.RectangleF (GDI)
    /// - System.Windows.Rect (WIC)
    /// - System.Windows.Int32Rect (WIC)
    /// - Sixlabors.Primitives.Rectangle
    /// - OpenCvSharp.Rect
    /// - SkiaSharp.SKRectI
    /// - Microsoft.Xna.Framework.Rectangle    
    /// </remarks>
    [System.Diagnostics.DebuggerDisplay("{X},{Y} {Width}x{Height}")]
    public readonly struct BitmapBounds : IEquatable<BitmapBounds>
    {
        #region constructor

        public static implicit operator BitmapBounds(in (int x, int y, int w, int h) rect)
        {
            return new BitmapBounds(rect.x, rect.y, rect.w, rect.h);
        }
        
        public BitmapBounds(int x, int y,int w,int h)
        {
            this.X = x;
            this.Y = y;
            this.Width = w;
            this.Height = h;
        }

        #endregion

        #region data

        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;

        public override int GetHashCode() { return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode(); }

        public static bool AreEqual(in BitmapBounds a, in BitmapBounds b)
        {
            if (a.X != b.X) return false;
            if (a.Y != b.Y) return false;
            if (a.Width != b.Width) return false;
            if (a.Height != b.Height) return false;
            return true;
        }

        public bool Equals(BitmapBounds other) { return AreEqual(this, other); }

        public override bool Equals(object obj) { return obj is BitmapBounds other ? AreEqual(this, other) : false; }

        public static bool operator ==(in BitmapBounds a, in BitmapBounds b) { return AreEqual(a, b); }

        public static bool operator !=(in BitmapBounds a, in BitmapBounds b) { return !AreEqual(a, b); }

        #endregion

        #region properties

        public int Area => Width * Height;

        public (int X, int Y) Origin => (X, Y);

        public (int Width, int Height) Size => (Width, Height);
        
        #endregion

        #region API

        public bool Contains(in BitmapBounds other)
        {
            if (other.X < this.X) return false;
            if (other.Y < this.Y) return false;

            if (other.X + other.Width > this.X + this.Width) return false;
            if (other.Y + other.Height > this.Y + this.Height) return false;

            return true;
        }

        public static BitmapBounds Clamp(in BitmapBounds value, in BitmapBounds clamp)
        {
            var x = value.X;
            var y = value.Y;
            var w = value.Width;
            var h = value.Height;

            if (x < clamp.X) { w -= (clamp.X - x); x = clamp.X; }
            if (y < clamp.Y) { h -= (clamp.Y - y); y = clamp.Y; }

            if (x + w > clamp.X + clamp.Width) w -= (x + w) - (clamp.X + clamp.Width);
            if (y + h > clamp.Y + clamp.Height) h -= (y + h) - (clamp.Y + clamp.Height);

            if (w < 0) w = 0;
            if (h < 0) h = 0;

            return new BitmapBounds(x, y, w, h);
        }

        #endregion
    }    
}

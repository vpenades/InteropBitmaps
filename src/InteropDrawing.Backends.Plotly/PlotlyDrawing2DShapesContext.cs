﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

using static System.FormattableString;

using Plotly;
using Plotly.Types;

// https://plotly.com/javascript/shapes/

using SHAPES = Plotly.Box<Plotly.Types.IShapesProperty>;

namespace InteropDrawing.Backends
{
    class _PlotlyDrawing2DShapesContext : IDrawingContext2D
    {
        #region lifecycle
        public _PlotlyDrawing2DShapesContext(PlotlyDocumentBuilder owner)
        {
            _Owner = owner;            
            _Shapes = new List<SHAPES>();
        }
        public void Dispose()
        {            

            if (_Shapes != null)
            {
                foreach (var s in _Shapes) _Owner.AppendShape(s);
            }

            _Shapes = null;         
        }        

        #endregion

        #region data

        private readonly PlotlyDocumentBuilder _Owner;        
        private List<SHAPES> _Shapes;

        #endregion

        #region API

        public void DrawAsset(in Matrix3x2 transform, object asset, ColorStyle style)
        {
            throw new NotImplementedException();
        }

        public void DrawLines(ReadOnlySpan<Point2> points, float diameter, InteropDrawing.LineStyle style)
        {
            throw new NotImplementedException();
        }

        public void DrawEllipse(Point2 center, float width, float height, ColorStyle style)
        {
            var fill = Shape.fillcolor("rgb(44, 160, 101)");
            var line = Shape.line(Line.color("rgb(44, 160, 101)"));

            var shape = Shapes.shape
                (
                Shape._type.circle(),
                Shape.x0(center.X - width * 0.5f),
                Shape.x1(center.X + width * 0.5f),
                Shape.y0(center.Y - height * 0.5f),
                Shape.y1(center.Y + height * 0.5f),
                fill, line
                );

            _Shapes.Add(shape);
        }

        public void DrawSprite(in Matrix3x2 transform, in SpriteStyle style)
        {
            throw new NotImplementedException();
        }

        public void DrawPolygon(ReadOnlySpan<Point2> points, ColorStyle style)
        {
            var sb = new StringBuilder();

            // M 1 1 L 1 3 L 4 1 Z            

            foreach(var p in points)
            {
                if (sb.Length == 0) sb.Append("M ");
                else sb.Append("L ");
                sb.Append(Invariant($"{p.X} {p.Y} "));
            }

            sb.Append("Z");

            var path = Shape.path(sb.ToString());
            var fill = Shape.fillcolor("rgb(44, 160, 101)");
            var line = Shape.line(Line.color("rgb(44, 160, 101)"));

            var shape = Shapes.shape
                (
                Shape._type.path(),                
                path,
                fill, line
                );            

            _Shapes.Add(shape);
        }        

        #endregion
    }
}

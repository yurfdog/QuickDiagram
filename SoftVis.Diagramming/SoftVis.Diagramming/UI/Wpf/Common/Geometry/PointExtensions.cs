﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Codartis.SoftVis.UI.Wpf.Common.Geometry
{
    public static class PointExtensions
    {
        public static bool IsExtreme(this Point point)
        {
            return double.IsNaN(point.X) || double.IsNaN(point.Y);
        }

        public static Rect BoundingRect(this IEnumerable<Point> points)
        {
            if (points == null)
                return Rect.Empty;

            var pointArray = points as Point[] ?? points.ToArray();

            if (pointArray.Length == 0)
                return Rect.Empty;

            var left = pointArray.Select(i => i.X).Min();
            var top = pointArray.Select(i => i.Y).Min();
            var right = pointArray.Select(i => i.X).Max();
            var bottom = pointArray.Select(i => i.Y).Max();

            return new Rect(new Point(left, top), new Point(right, bottom));
        }
    }
}
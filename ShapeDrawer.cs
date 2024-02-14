using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MyPaint
{
    public class ShapeDrawer
    {
        private Graphics graphics;
        private Pen currentPen;
        private bool fillShape;

        public ShapeDrawer(Graphics graphics, Pen pen, bool fillShape)
        {
            this.graphics = graphics;
            this.currentPen = pen;
            this.fillShape = fillShape;
        }
        public void DrawLine(Point start, Point end)
        {
            graphics.DrawLine(currentPen, start, end);
        }

        public void DrawCircle(Point center, int radius)
        {
            if (fillShape)
                graphics.FillEllipse(new SolidBrush(currentPen.Color), center.X - radius, center.Y - radius, 2 * radius, 2 * radius);
            else
                graphics.DrawEllipse(currentPen, center.X - radius, center.Y - radius, 2 * radius, 2 * radius);
        }

        public void DrawPolygon(Point[] points)
        {
            if (points.Length < 3)
                return;
            GraphicsPath polygonPath = new GraphicsPath();

            polygonPath.AddPolygon(points);

            if (fillShape)
                graphics.FillPath(new SolidBrush(currentPen.Color), polygonPath);
            else
                graphics.DrawPath(currentPen, polygonPath);
        }

        public void SetFillShape(bool fill)
        {
            fillShape = fill;
        }
    }
}
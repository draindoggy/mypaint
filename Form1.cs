using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MyPaint
{
    public partial class Form1 : Form
    {
        private Stack<Bitmap> undoStack = new Stack<Bitmap>();
        private Stack<Bitmap> redoStack = new Stack<Bitmap>();
        private enum ShapeType { Line, Circle, Polygon }

        private ShapeType currentShape = ShapeType.Line;
        private Pen currentPen = new Pen(Color.Black, 2);
        private bool fillShape = false;

        private Point startPoint;
        private Point endPoint;

        private Graphics graphics;
        private Bitmap canvas;

        private Point[] polygonPoints = new Point[0];

        private bool isDrawingPolygon = false;

        private bool isLineSelected = false;
        private Point lineStartPoint;
        private Point lineEndPoint;

        private ShapeDrawer shapeDrawer;

        private List<DrawableObject> drawableObjects = new List<DrawableObject>();

        public interface IDrawable
        {
            void Draw(Graphics g);
            bool IsPointInShape(Point point);
            void Move(int dx, int dy);
        }

        public class DrawableObject
        {
            public IDrawable Shape { get; set; }
            public bool IsSelected { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            graphics = Graphics.FromImage(canvas);
            pictureBox1.Image = canvas;
            this.KeyPreview = true;

            shapeDrawer = new ShapeDrawer(graphics, currentPen, fillShape); 
        }

        private void DrawShapes()
        {
            foreach (var drawableObject in drawableObjects)
            {
                drawableObject.Shape.Draw(graphics);
            }
        }

        private void MoveSelectedShape(int dx, int dy)
        {
            var selectedObject = drawableObjects.Find(obj => obj.IsSelected);
            if (selectedObject != null)
            {
                selectedObject.Shape.Move(dx, dy);
                pictureBox1.Refresh();
            }
        }

        private void DrawShape(Point start, Point end)
        {
            switch (currentShape)
            {
                case ShapeType.Line:
                    shapeDrawer.DrawLine(start, end);
                    break;
                case ShapeType.Circle:
                    int radius = (int)Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
                    shapeDrawer.DrawCircle(start, radius);
                    break;
                case ShapeType.Polygon:
                    break;
            }
            pictureBox1.Refresh();
        }

        private void DrawPolygon(Point[] points)
        {
            if (points.Length < 3)
                return;

            shapeDrawer.DrawPolygon(points);
            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            startPoint = e.Location;
            SaveUndoState();

            foreach (var drawableObject in drawableObjects)
            {
                if (drawableObject.Shape.IsPointInShape(startPoint))
                {
                    drawableObject.IsSelected = true;
                    break;
                }
            }
        }



        private void SaveUndoState()
        {
            undoStack.Push(new Bitmap(canvas));
            redoStack.Clear();
        }

        private void Undo()
        {
            if (undoStack.Count > 1)
            {
                redoStack.Push(undoStack.Pop());
                canvas = new Bitmap(undoStack.Peek());
                pictureBox1.Image = canvas;
                graphics = Graphics.FromImage(canvas); 
                shapeDrawer = new ShapeDrawer(graphics, currentPen, fillShape);
                pictureBox1.Refresh();
            }
        }

        private void Redo()
        {
            if (redoStack.Count > 0)
            {
                undoStack.Push(redoStack.Pop());
                canvas = new Bitmap(undoStack.Peek());
                pictureBox1.Image = canvas;
                graphics = Graphics.FromImage(canvas); 
                shapeDrawer = new ShapeDrawer(graphics, currentPen, fillShape); 
                pictureBox1.Refresh();
            }
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            endPoint = e.Location;
            if (currentShape != ShapeType.Polygon)
            {
                DrawShape(startPoint, endPoint);
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentShape == ShapeType.Polygon && isDrawingPolygon)
            {
                endPoint = e.Location;
                pictureBox1.Refresh();
            }
            else if (isLineSelected)
            {
                lineEndPoint = e.Location;
                pictureBox1.Refresh();
            }
        }

        private void LineButton_Click(object sender, EventArgs e)
        {
            currentShape = ShapeType.Line;
            isDrawingPolygon = false;
            isLineSelected = false;
            btnOFF();
        }

        private void CircleButton_Click(object sender, EventArgs e)
        {
            currentShape = ShapeType.Circle;
            isDrawingPolygon = false;
            isLineSelected = false;
            btnOFF();
        }

        private void PolygonButton_Click(object sender, EventArgs e)
        {
            if (currentShape == ShapeType.Polygon && isDrawingPolygon && polygonPoints.Length >= 3)
            {
                isDrawingPolygon = false;
                DrawPolygon(polygonPoints);
            }
            else
            {
                currentShape = ShapeType.Polygon;
                isDrawingPolygon = true;
                polygonPoints = new Point[0];
            }
            isLineSelected = false;
            btnOFF();
        }

        private void ColorButton_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                currentPen.Color = colorDialog1.Color;
            }
        }

        private void FillCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            shapeDrawer.SetFillShape(FillCheckBox.Checked);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            DrawShapes();
        }

        private void btnOFF()
        {
            CircleButton.Enabled = false;
            PolygonButton.Enabled = false;
            LineButton.Enabled = false;
            ColorButton.Enabled = false;
            FillCheckBox.Enabled = false;
        }

        private void btnON()
        {
            CircleButton.Enabled = true;
            PolygonButton.Enabled = true;
            LineButton.Enabled = true;
            ColorButton.Enabled = true;
            FillCheckBox.Enabled = true;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                MoveSelectedShape(0, -35);
            }
            else if (e.KeyCode == Keys.S)
            {
                MoveSelectedShape(0, 35);
            }
            else if (e.KeyCode == Keys.A)
            {
                MoveSelectedShape(-35, 0);
            }
            else if (e.KeyCode == Keys.D)
            {
                MoveSelectedShape(35, 0);
            }

            if (e.Control && e.KeyCode == Keys.Z)
            {
                Undo();
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                Redo();
            }
            else if(e.KeyCode == Keys.Escape)
            {
                btnON();
            }
        }
    }
}

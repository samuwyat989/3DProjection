using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DProjection
{
    public partial class DrawScreen : UserControl
    {
        public DrawScreen()
        {
            InitializeComponent();
            setup();
        }

        double angle = 0;
        int cIndex = 0;

        RVectors.RNVector[] points = new RVectors.RNVector[16];

        #region 4D Cube "Tesseract"
        RVectors.Matrix projection4D = new RVectors.Matrix(new List<float>(new float[] 
        { 1, 0, 0, 0,
          0, 1, 0, 0,
          0, 0, 1, 0}), 4);
        PointF[] corners4D = new PointF[16];
        RVectors.Matrix rotationXY = new RVectors.Matrix();
        RVectors.Matrix rotationXZ = new RVectors.Matrix();
        RVectors.Matrix rotationZW = new RVectors.Matrix();
        #endregion

        #region 3D Cube
        RVectors.Matrix projection = new RVectors.Matrix(new List<float>(new float[]{1,0,0,0,1,0}),3);
        PointF[] corners3D = new PointF[8];
        RVectors.Matrix rotationZ = new RVectors.Matrix();
        RVectors.Matrix rotationY = new RVectors.Matrix();
        RVectors.Matrix rotationX = new RVectors.Matrix();
        #endregion

        Color drawColor = Color.FromArgb(210, Color.Orange);
        SolidBrush drawBrush = new SolidBrush(Color.White);       
        Pen drawPen = new Pen(Color.White);

        public void setup()
        {
            drawBrush.Color = drawColor;
            drawPen.Color = drawColor;

            //This is insane, need to refactor
            points[0] = new RVectors.RNVector(new List<float>(new float[] { -1, 1, -1, 1 }));
            points[1] = new RVectors.RNVector(new List<float>(new float[] { 1, -1, -1, 1 }));
            points[2] = new RVectors.RNVector(new List<float>(new float[] { 1, 1, -1, 1 }));
            points[3] = new RVectors.RNVector(new List<float>(new float[] { -1, -1, -1, 1 }));
            points[4] = new RVectors.RNVector(new List<float>(new float[] { -1, 1, 1, 1 }));
            points[5] = new RVectors.RNVector(new List<float>(new float[] { 1, -1, 1, 1 }));
            points[6] = new RVectors.RNVector(new List<float>(new float[] { 1, 1, 1, 1 }));
            points[7] = new RVectors.RNVector(new List<float>(new float[] { -1, -1, 1, 1 }));

            points[8] = new RVectors.RNVector(new List<float>(new float[] { -1, 1, -1, -1 }));
            points[9] = new RVectors.RNVector(new List<float>(new float[] { 1, -1, -1, -1 }));
            points[10] = new RVectors.RNVector(new List<float>(new float[] { 1, 1, -1, -1 }));
            points[11] = new RVectors.RNVector(new List<float>(new float[] { -1, -1, -1, -1 }));
            points[12] = new RVectors.RNVector(new List<float>(new float[] { -1, 1, 1, -1 }));
            points[13] = new RVectors.RNVector(new List<float>(new float[] { 1, -1, 1, -1 }));
            points[14] = new RVectors.RNVector(new List<float>(new float[] { 1, 1, 1, -1 }));
            points[15] = new RVectors.RNVector(new List<float>(new float[] { -1, -1, 1, -1 }));

            setRotation();
        }

        private void DrawScreen_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(150, 150);

            RVectors.RNVector[] points3D = proj4D_To_3D(points);
            RVectors.RNVector[] points2D = proj3D_To_2D(points3D);

            foreach (RVectors.RNVector v in points2D)
            {                
                v.scalarMult(300);
                e.Graphics.FillEllipse(drawBrush, v.components[0] - 3, v.components[1] - 3, 6, 6);
            }

            PointF[] corners = getCorners(points2D);

            swap(1, 2, corners);
            swap(5, 6, corners);

            //Connects outer cube
            for (int i = 0; i < 4; i++)
            {
                e.Graphics.DrawLine(drawPen, corners[i], corners[(i + 1) % 4]);
                e.Graphics.DrawLine(drawPen, corners[i + 4], corners[((i + 1) % 4) + 4]);
                e.Graphics.DrawLine(drawPen, corners[i], corners[i + 4]);
            }

            swap(9, 10, corners);
            swap(13, 14, corners);
            //Connects inner cube
            for (int i = 0; i < 4; i++)
            {
                e.Graphics.DrawLine(drawPen, corners[i+8], corners[((i + 1)%4)+8]);
                    e.Graphics.DrawLine(drawPen, corners[i + 12], corners[((i + 1) % 4) + 12]);
                e.Graphics.DrawLine(drawPen, corners[i + 8], corners[i + 12]);
            }
            //Connects outer cube to inner cube
            for (int i = 0; i < 8; i++)
            {
                    e.Graphics.DrawLine(drawPen, corners[i], corners[i + 8]);
            }

            //paints layer
            e.Graphics.FillPolygon(drawBrush, new PointF[] { new PointF(corners[0].X, corners[0].Y),
            new PointF(corners[1].X, corners[1].Y),
            new PointF(corners[2].X, corners[2].Y),
            new PointF(corners[3].X, corners[3].Y)});
        }

        public void swap(int indexA, int indexB, PointF[] corners)
        {
            PointF temp = corners[indexA];
            corners[indexA] = corners[indexB];
            corners[indexB] = temp;
        }

        public void setRotation()
        {
            #region 3D Cube
            //rotationZ = new RVectors.Matrix(new List<float>(new float[]
            //{(float)Math.Cos(angle),-(float)Math.Sin(angle),0,
            // (float)Math.Sin(angle),(float)Math.Cos(angle),0,
            // 0,0,1}), 3);

            //rotationY = new RVectors.Matrix(new List<float>(new float[]
            //{(float)Math.Cos(angle),0,-(float)Math.Sin(angle),
            // 0, 1, 0,
            //(float)Math.Sin(angle),0,(float)Math.Cos(angle)}), 3);

            //rotationX = new RVectors.Matrix(new List<float>(new float[]
            //{ 1, 0, 0,
            //0, (float)Math.Cos(angle),-(float)Math.Sin(angle),
            //0, (float)Math.Sin(angle),(float)Math.Cos(angle)}), 3);
            #endregion

            #region 4D Cube
            rotationXY = new RVectors.Matrix(new List<float>(new float[]
            {(float)Math.Cos(angle),-(float)Math.Sin(angle),0,0,
             (float)Math.Sin(angle), (float)Math.Cos(angle),0,0,
             0,0,1,0,
             0,0,0,1}), 4);

            rotationXZ = new RVectors.Matrix(new List<float>(new float[]
            {(float)Math.Cos(angle),0,-(float)Math.Sin(angle),0,
             0,1,0,0,
             (float)Math.Sin(angle), 0, (float)Math.Cos(angle),0,
             0,0,0,1}), 4);

            rotationZW = new RVectors.Matrix(new List<float>(new float[]
            {1, 0, 0, 0,
             0, 1, 0, 0,
             0, 0, (float)Math.Cos(angle), -(float)Math.Sin(angle),
             0, 0, (float)Math.Sin(angle), (float)Math.Cos(angle)}), 4);
            #endregion
        }

        public PointF[] getCorners(RVectors.RNVector[] points)
        {
            PointF[] corners = new PointF[points.Length];
            int cIndex = 0;
            foreach (RVectors.RNVector v in points)
            {
                corners[cIndex] = new PointF(v.components[0], v.components[1]);
                cIndex++;
            }

            return corners;
        }

        public RVectors.RNVector[] proj3D_To_2D(RVectors.RNVector[] projPoints)
        {
            RVectors.RNVector[] twoDVecs = new RVectors.RNVector[projPoints.Length];

            cIndex = 0;

            //Just to view tesseract in right angle 
            RVectors.Matrix xRotate = new RVectors.Matrix(new List<float>(new float[]
            { 1, 0, 0,
            0, (float)Math.Cos(Math.PI/2),-(float)Math.Sin(Math.PI/2),
            0, (float)Math.Sin(Math.PI/2),(float)Math.Cos(Math.PI/2)}), 3);

            foreach (RVectors.RNVector v in projPoints)
            {
                //Makes matrix out of vector
                RVectors.Matrix vecMatrix = v.ToMatrix();

                //Rotates matrix
                RVectors.Matrix rMatrix = xRotate.mult(vecMatrix);
                //rMatrix = rotationZ.mult(rMatrix);
                //rMatrix = rotationY.mult(rMatrix);

                //Perspective
                float distance = 3;
                float z = 1 / (distance - rMatrix.MRows[2][0]);
                projection = new RVectors.Matrix(new List<float>(new float[] { z, 0, 0, 0, z, 0 }), 3);

                //Projects Matrix
                RVectors.RNVector projected = projection.mult(rMatrix).ToVec();

                twoDVecs[cIndex] = projected;
                cIndex++;
            }

            return twoDVecs;
        }

        public RVectors.RNVector[] proj4D_To_3D(RVectors.RNVector[] projPoints)
        {
            RVectors.RNVector[] threeDVecs = new RVectors.RNVector[projPoints.Length];

            cIndex = 0;
            foreach (RVectors.RNVector v in projPoints)
            {
                //Makes matrix out of vector
                RVectors.Matrix vecMatrix = v.ToMatrix();

                //Rotates matrix
                RVectors.Matrix rMatrix = rotationXY.mult(vecMatrix);
                rMatrix = rotationZW.mult(rMatrix);
                //rMatrix = rotationY.mult(rMatrix);

                //Perspective
                float distance = 3;
                float w = 1 / (distance - rMatrix.MRows[3][0]);
                projection4D = new RVectors.Matrix(new List<float>(new float[] 
                { w, 0, 0, 0,
                  0, w, 0, 0,
                  0, 0, w, 0, }), 4);

                //Projects Matrix
                RVectors.RNVector projected = projection4D.mult(rMatrix).ToVec();

                threeDVecs[cIndex] = projected;
                cIndex++;
            }

            return threeDVecs;

        }

        private void drawTimer_Tick(object sender, EventArgs e)
        {
            angle += 0.03;

            setRotation();

            Refresh();
        }
    }
}

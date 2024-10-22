using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CG_Lab
{
    public partial class FormTask3 : Form
    {
        private List<PointF> controlPoints = new List<PointF>();
        private int selectedPointIndex = -1;

        public FormTask3()
        {
            InitializeComponent();
            DoubleBuffered = true;
            pictureBox1.Paint += PictureBox1_Paint;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            StartPosition = FormStartPosition.CenterScreen; 
            WindowState = FormWindowState.Maximized;

        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            DrawBezierCurve(g);
            DrawControlPoints(g);
            // Рисуем все линии
            using (Pen dashedPen = new Pen(Color.Gray))
            {
                dashedPen.DashStyle = DashStyle.Dash;
                dashedPen.DashPattern = new float[] { 2, 4 };
                if (controlPoints.Count >= 2)
                {
                    for (int i = 1; i < controlPoints.Count; i++)
                    {
                        PointF previousPoint = controlPoints[i - 1];
                        PointF currentPoint = controlPoints[i];

                        // Рисуем линию от предыдущей точки к текущей
                        g.DrawLine(dashedPen, previousPoint, currentPoint);
                    }
                }
            }
        }

        private void DrawBezierCurve(Graphics g)
        {
            //if (controlPoints.Count < 4 || controlPoints.Count % 2 != 0)
                //return;

            // Рисуем кривые Безье по группам из четырех точек
            for (int i = 0; i <= controlPoints.Count - 4; i += 3)
            {
                if (i + 3 > controlPoints.Count - 1)
                    return;
                PointF p0 = controlPoints[i];
                PointF p1 = controlPoints[i + 1];
                PointF p2 = controlPoints[i + 2];
                PointF p3 = controlPoints[i + 3];

                // Рисование кривой Безье
                for (float t = 0; t <= 1; t += 0.001f)
                {
                    PointF point = CalculateBezierPoint(t, p0, p1, p2, p3);
                    g.FillEllipse(Brushes.CornflowerBlue, point.X - 2, point.Y - 2, 3, 3);
                }

                // Рисуем линии касательных для наглядности
                g.DrawLine(Pens.Red, p0, p1);
                g.DrawLine(Pens.Red, p2, p3);
            }
        }

        private PointF CalculateBezierPoint(float t, PointF p0, PointF p1, PointF p2, PointF p3)
        {
            //Для каждого t вычисляем координаты точки по формуле из презентации
            float x = (float)(Math.Pow(1 - t, 3) * p0.X +
                              3 * Math.Pow(1 - t, 2) * t * p1.X +
                              3 * (1 - t) * Math.Pow(t, 2) * p2.X +
                              Math.Pow(t, 3) * p3.X);

            float y = (float)(Math.Pow(1 - t, 3) * p0.Y +
                              3 * Math.Pow(1 - t, 2) * t * p1.Y +
                              3 * (1 - t) * Math.Pow(t, 2) * p2.Y +
                              Math.Pow(t, 3) * p3.Y);

            return new PointF(x, y);
        }

        private void DrawControlPoints(Graphics g)
        {
            for (int i = 0; i < controlPoints.Count; i++)
            {
                //if (controlPoints.Count == 4 || i < 3 || i % 2 == 0 || i == controlPoints.Count - 1 || i == controlPoints.Count - 2)
                if (IsAuxiliaryPoint(i))
                    g.FillEllipse(Brushes.Indigo, controlPoints[i].X - 5, controlPoints[i].Y - 5, 7, 7);
                else
                    g.FillEllipse(Brushes.IndianRed, controlPoints[i].X - 5, controlPoints[i].Y - 5, 7, 7);
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
               
                for (int i = 0; i < controlPoints.Count; i++)
                {
                    if (Math.Abs(e.X - controlPoints[i].X) < 5 && Math.Abs(e.Y - controlPoints[i].Y) < 5)
                    {
                        selectedPointIndex = i;
                        return;
                    }
                }

                PointF newPoint = new PointF(e.X, e.Y);

                ///*
                // Если точек достаточно для новой кривой, добавить промежуточную точку для плавного соединения
                if (controlPoints.Count > 4 && (controlPoints.Count + 1) % 3 == 0) //(controlPoints.Count == 5 || controlPoints.Count > 5 && controlPoints.Count % 2 == 0))
                {

                    ///*
                    PointF lastPoint = controlPoints[controlPoints.Count - 2];
                    PointF prevPoint = controlPoints[controlPoints.Count - 3];

                    // Отражаем последнюю точку относительно предпоследней
                    PointF reflectedPoint = new PointF(lastPoint.X / 2 + prevPoint.X / 2, lastPoint.Y / 2 + prevPoint.Y / 2);

                    // Добавляем отраженную точку как новую контрольну
                    controlPoints.Insert(controlPoints.Count - 2, reflectedPoint);
                    //*/
                }
                //*/

                // Добавляем новую точку
                controlPoints.Add(newPoint);
                selectedPointIndex = -1;
                pictureBox1.Invalidate();
                //pictureBox1.Refresh();// Перерисовать PictureBox
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Удаление ближайшей точки, если она существует
                for (int i = 0; i < controlPoints.Count; i++)
                {
                    if (!IsAuxiliaryPoint(i) && Math.Abs(e.X - controlPoints[i].X) < 5 && Math.Abs(e.Y - controlPoints[i].Y) < 5)
                    {
                        List<int> toRemove = new List<int>();
                        for (int j = 0; j < controlPoints.Count(); j++)
                        {
                            if (IsAuxiliaryPoint(j))
                                toRemove.Add(j);
                        }
                        toRemove.Add(i);
                        toRemove.Sort();
                        for (int j = 0; j < toRemove.Count(); j++)
                            controlPoints.RemoveAt(toRemove[j] - j);
                        BuildAuxiliaryPoint();
                        pictureBox1.Invalidate();
                        break;
                    }
                }
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedPointIndex >= 0 && e.Button == MouseButtons.Left)
            {
                PointF deltaPoint = new PointF(e.X - controlPoints[selectedPointIndex].X, e.Y - controlPoints[selectedPointIndex].Y);
                controlPoints[selectedPointIndex] = new PointF(e.X, e.Y);

                if (selectedPointIndex > 0 && selectedPointIndex < controlPoints.Count - 1)
                {
                    // Перемещаем выбранную точку
                    if (selectedPointIndex % 3 == 0)
                    {
                        if (selectedPointIndex >= 3)
                            controlPoints[selectedPointIndex - 1] = new PointF(controlPoints[selectedPointIndex - 1].X + deltaPoint.X,
                                controlPoints[selectedPointIndex - 1].Y + deltaPoint.Y);
                        if (selectedPointIndex < controlPoints.Count - 1)
                            controlPoints[selectedPointIndex + 1] = new PointF(controlPoints[selectedPointIndex + 1].X + deltaPoint.X,
                                controlPoints[selectedPointIndex + 1].Y + deltaPoint.Y);
                    }
                    else if (selectedPointIndex % 3 == 2)
                    {
                        if (selectedPointIndex < controlPoints.Count - 2)
                            controlPoints[selectedPointIndex + 2] = new PointF(controlPoints[selectedPointIndex + 2].X - deltaPoint.X,
                            controlPoints[selectedPointIndex + 2].Y - deltaPoint.Y);
                    }
                    else if (selectedPointIndex > 1)
                        controlPoints[selectedPointIndex - 2] = new PointF(controlPoints[selectedPointIndex - 2].X - deltaPoint.X,
                        controlPoints[selectedPointIndex - 2].Y - deltaPoint.Y);
                }
                pictureBox1.Invalidate(); // Перерисовать PictureBox
            }
        }

        private bool IsAuxiliaryPoint(int index)
        {
            if (controlPoints.Count <= 5)
                return false;
            if (index >= 3 && index < controlPoints.Count - 2 && index % 3 == 0)
                return true;
            return false;
        }

        private void BuildAuxiliaryPoint()
        {
            if (controlPoints.Count() == 5)
                return;
            for (int i = 3; i < controlPoints.Count - 1; i += 4)
                controlPoints.Insert(i, new PointF(controlPoints[i].X / 2 + controlPoints[i - 1].X / 2,
                    controlPoints[i].Y / 2 + controlPoints[i - 1].Y / 2));
        }

        private void FormTask3_Resize(object sender, EventArgs e)
        {
            pictureBox1.Size = ClientSize;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            controlPoints.Clear();
            pictureBox1.Invalidate();
        }
    }
}

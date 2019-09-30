using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace sumkinect
{
    public class HandMotion_Tracking
    {
        private static int HandSize = 20;

        private readonly static Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(200, 255, 0, 0));

        private readonly static Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(200, 0, 255, 0));

        private readonly static Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(200, 0, 0, 255));


        static int left_down = 0x0002;
        static int left_up = 0x0004;
        static int right_down = 0x0008;
        static int right_up = 0x0010;


        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern int SetCursorPos(int x, int y);

        private static void LeftClick_down(Point t)
        {
            mouse_event((int)(left_down), 0, 0, 0, 0);
        }

        private static void LeftClick_up(Point t)
        {
            mouse_event((int)(left_up), 0, 0, 0, 0);
        }

        private static void RightClick(Point t)
        {
            mouse_event((int)(right_down | right_up), 0, 0, 0, 0);
        }


        public static void MoveMouse(Point t)
        {
            int pX = (int)t.X;
            int pY = (int)t.Y;

            SetCursorPos((pX - 100) * 6, (pY - 50) * 6);
        }

        public static void DrawHand_Right(HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Open:
                    drawingContext.DrawEllipse(handOpenBrush, null, handPosition, HandSize, HandSize);
                    MoveMouse(handPosition);
                    break;

                case HandState.Closed:
                    drawingContext.DrawEllipse(handClosedBrush, null, handPosition, HandSize, HandSize);
                    MoveMouse(handPosition);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(handLassoBrush, null, handPosition, HandSize, HandSize);
                    MoveMouse(handPosition);
                    //RightClick(handPosition);
                    break;
            }
        }

        /*public static void DrawHand_left(HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Open:
                    drawingContext.DrawEllipse(handOpenBrush, null, handPosition, HandSize, HandSize);
                    LeftClick_up(handPosition);
                    break;

                case HandState.Closed:
                    drawingContext.DrawEllipse(handClosedBrush, null, handPosition, HandSize, HandSize);
                    LeftClick_down(handPosition);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(handLassoBrush, null, handPosition, HandSize, HandSize);
                    break;
            }
        }*/
    }
}

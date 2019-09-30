using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;


namespace sumkinect
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        HandMotion_Tracking handMotion_Tracking = new HandMotion_Tracking(); //클래스 변수 선언

        private KinectSensor kinectSensor = null; //kinectsensor object 주소를 저장하기 위한 변수 선언 - null = 기본 형식의 정의되지 않은 값을 표시할 때 사용

        private ColorFrameReader colorFrameReader = null; //color data를 받기위해 선언

        private WriteableBitmap colorBitmap = null; //bitmap 업데이트를 위한 선언

        private const double HandSize = 20;

        private const double JointThicknesss = 3;

        private const double ClipBoundsThickness = 5;

        private const float InferredZPositionClamp = 0.1f;

        private readonly Brush Wristbrush = new SolidColorBrush(Color.FromArgb(200, 0, 255, 0));

        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(200, 255, 0, 0));

        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(200, 0, 255, 0));

        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(200, 68, 192, 68)); //

        private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(200, 0, 0, 255));

        private readonly Brush inferredJointBrush = Brushes.Red; //인식안되는 조인트

        private readonly Pen inferredBonePen = new Pen(Brushes.Red, 1);

        private List<Pen> bodyColors;

        private DrawingGroup drawingGroup;

        private DrawingImage imageSource;

        private CoordinateMapper coordinateMapper = null;

        private BodyFrameReader bodyFrameReader = null;

        private Body[] bodies = null;

        private List<Tuple<JointType, JointType>> bones;

        public int displayWidth, displayHeight;

        public MainWindow()
        {

            this.kinectSensor = KinectSensor.GetDefault(); //pc에 연결된 kinectsonsor의 object를 kinectsensor클래스 변수에 저장

            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader(); //colordata open

            this.colorFrameReader.FrameArrived += this.Read_ColorFrameArrived;

            FrameDescription colorframeDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            //이미지 프레임의 속성 설정
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            this.colorBitmap = new WriteableBitmap(colorframeDescription.Width, colorframeDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            

            this.coordinateMapper = kinectSensor.CoordinateMapper;

            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            this.bodyFrameReader.FrameArrived += this.Read_BodyFrameArrived;

            if (this.bones != null)
            {
                this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));

                this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

                this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));
            }
            
            this.bodyColors = new List<Pen>();
            this.bodyColors.Add(new Pen(Brushes.Red, 6));
            this.bodyColors.Add(new Pen(Brushes.Orange, 6));
            this.bodyColors.Add(new Pen(Brushes.Green, 6));
            this.bodyColors.Add(new Pen(Brushes.Blue, 6));
            this.bodyColors.Add(new Pen(Brushes.Indigo, 6)); 
            this.bodyColors.Add(new Pen(Brushes.Violet, 6));


            this.kinectSensor.Open();

            this.drawingGroup = new DrawingGroup();

            this.imageSource = new DrawingImage(this.drawingGroup);

            this.DataContext = this; //이게들어가야 데이터를 가져올 수 있다.(kinect에서)

            InitializeComponent();


        }

        public ImageSource ImageSources
        {
            get
            {
                return this.colorBitmap;
            }
        }

        public ImageSource ImageSources2
        {
            get
            {
                return this.imageSource;
            }
        }

        private void Read_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame()) // colorframe 클래스 변수에 colordata를 저장
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;
                    //FrameDescription = KinectSensor에서의 이미지 프레임의 속성
                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(colorBitmap.BackBuffer, (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4), ColorImageFormat.Bgra);
                            //colorframe의 현제 프레임 정보를 colorbitmap에 복사하는 기능, 이미지 크기 = 가로*세로*4바이트의 색상, 이미지포멧은 Bgra포맷

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                    }
                }
            }
        }

        public void Read_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if ( this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                    //Transparent

                    int penIndex = 0;
                    foreach (Body body in this.bodies)
                    {
                        Pen drawPen = this.bodyColors[penIndex++];

                        if (body.IsTracked)
                        {
                            this.DrawClippedEdges(body, dc);

                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                            foreach (JointType jointType in joints.Keys)
                            {
                                CameraSpacePoint position = joints[jointType].Position;
                                if (position.Z < 0)
                                {
                                    position.Z = InferredZPositionClamp;
                                }

                                DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);

                            }

                            this.DrawBody(joints, jointPoints, dc, drawPen);

                            HandMotion_Tracking.DrawHand_Right(jointPoints[JointType.HandRight], dc);
                            //HandMotion_Tracking.DrawHand_left(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                        }
                    }

                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        private void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Transparent,
                    null,
                    new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Transparent,
                    null,
                    new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
            }
        }

        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {

            if (this.bones != null)
            {
                foreach (var bone in this.bones)
                {
                    this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
                }
            }
            

            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrusk = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrusk = this.trackedJointBrush;
                }

                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrusk = this.inferredJointBrush;
                }

                if (drawBrusk !=null)
                {
                    drawingContext.DrawEllipse(drawBrusk, null, jointPoints[jointType], JointThicknesss, JointThicknesss);
                }
            }
        }

        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            if (joint0.TrackingState == TrackingState.NotTracked || joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }
    }    
}

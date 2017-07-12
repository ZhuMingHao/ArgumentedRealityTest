using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ArgumentedRealityTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, object e)
        {
            output.Text = "";
            output.Text = DateTime.Now.ToString("HH:mm:ss.fff") + Environment.NewLine;
            Print();
        }

        DispatcherTimer timer;
        public void WriteValue(String desc, String val)
        {
            StringBuilder b = new StringBuilder();
            int length = desc.Length + val.Length;
            int topad = 40 - length;
            if (topad < 0)
                topad = length - 40;
            output.Text += desc + val.PadLeft(topad + val.Length) + Environment.NewLine;
        }
        public String ValueToString(double value)
        {
            String ret = value.ToString("000.00000");
            if (value > 0)
                ret = " +" + ret;
            else if (value == 0)
                ret = "  " + ret;
            else
                ret = " " + ret;

            return ret;
        }
        public static double RadianToDegree(double radians)
        {
            return radians * (180 / Math.PI);
        }
        public void Print()
        {
            WriteValue("DisplayOrientation", LastDisplayOrient.ToString());

            WriteValue("Inclinometer", "");
            WriteValue("Pitch", ValueToString(LastIncline.PitchDegrees));
            WriteValue("Roll", ValueToString(LastIncline.RollDegrees));
            WriteValue("Yaw", ValueToString(LastIncline.YawDegrees));
            WriteValue("YawAccuracy", LastIncline.YawAccuracy.ToString());


            WriteValue("OrientationSensor", "");
            var q = LastOrient.Quaternion;


            double ysqr = q.Y * q.Y;
            // roll (x-axis rotation)
            double t0 = +2.0f * (q.W * q.X + q.Y * q.Z);
            double t1 = +1.0f - 2.0f * (q.X * q.X + ysqr);
            double Roll = RadianToDegree(Math.Atan2(t0, t1));
            // pitch (y-axis rotation)
            double t2 = +2.0f * (q.W * q.Y - q.Z * q.X);
            t2 = t2 > 1.0f ? 1.0f : t2;
            t2 = t2 < -1.0f ? -1.0f : t2;
            double Pitch = RadianToDegree(Math.Asin(t2));
            // yaw (z-axis rotation)
            double t3 = +2.0f * (q.W * q.Z + q.X * q.Y);
            double t4 = +1.0f - 2.0f * (ysqr + q.Z * q.Z);
            double Yaw = RadianToDegree(Math.Atan2(t3, t4));
            WriteValue("Roll", ValueToString(Roll));
            WriteValue("Pitch", ValueToString(Pitch));
            WriteValue("Yaw", ValueToString(Yaw));
        }
        Inclinometer sIncline;
        DisplayInformation sDisplay;
        OrientationSensor sOrient;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            sIncline = Inclinometer.GetDefault(SensorReadingType.Absolute);
            sDisplay = DisplayInformation.GetForCurrentView();
            sOrient = OrientationSensor.GetDefault(SensorReadingType.Absolute);
            sOrient.ReadingChanged += SOrient_ReadingChanged;
            sDisplay.OrientationChanged += SDisplay_OrientationChanged;
            sIncline.ReadingChanged += SIncline_ReadingChanged;

            LastDisplayOrient = sDisplay.CurrentOrientation;
            LastIncline = sIncline.GetCurrentReading();
            LastOrient = sOrient.GetCurrentReading();
            timer.Start();
        }

        private void SOrient_ReadingChanged(OrientationSensor sender, OrientationSensorReadingChangedEventArgs args)
        {
            LastOrient = args.Reading;
        }

        private void SDisplay_OrientationChanged(DisplayInformation sender, object args)
        {
            LastDisplayOrient = sDisplay.CurrentOrientation;
        }
        OrientationSensorReading LastOrient;
        InclinometerReading LastIncline;
        DisplayOrientations LastDisplayOrient;
        private void SIncline_ReadingChanged(Inclinometer sender, InclinometerReadingChangedEventArgs args)
        {
            LastIncline = args.Reading;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            sIncline.ReadingChanged -= SIncline_ReadingChanged;
            sDisplay.OrientationChanged -= SDisplay_OrientationChanged;
            sOrient.ReadingChanged -= SOrient_ReadingChanged;
            timer.Stop();
        }
    }
}

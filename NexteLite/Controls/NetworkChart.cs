using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Collections;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using System.Timers;

namespace NexteLite.Controls
{
    public class NetworkChartData
    {
        public NetworkChartData(double speed, DateTime time)
        {
            Speed = speed;
            Time = time;
        }

        public double Speed { get; set; }
        public DateTime Time { get; set; }
    }

    public class NetworkChart : FrameworkElement
    {
        private TimeSpan Interval = new TimeSpan(0, 0, 1);
        Timer _RenderTimer;

        public static DependencyProperty LineBrushProperty = DependencyProperty.Register("LineBrush", typeof(SolidColorBrush), typeof(NetworkChart), new FrameworkPropertyMetadata(Brushes.Blue) { AffectsRender = true });

        public SolidColorBrush LineBrush
        {
            get { return (SolidColorBrush)GetValue(LineBrushProperty); }
            set { SetValue(LineBrushProperty, value); }
        }

        public static DependencyProperty LineBackBrushProperty = DependencyProperty.Register("LineBackBrush", typeof(SolidColorBrush), typeof(NetworkChart), new FrameworkPropertyMetadata(Brushes.Blue) { AffectsRender = true });

        public SolidColorBrush LineBackBrush
        {
            get { return (SolidColorBrush)GetValue(LineBackBrushProperty); }
            set { SetValue(LineBackBrushProperty, value); }
        }

        public static DependencyProperty DataCollectionProperty = DependencyProperty.Register("DataCollection", typeof(IEnumerable<NetworkChartData>), typeof(NetworkChart), new FrameworkPropertyMetadata(new List<NetworkChartData>()) { AffectsRender = false });

        public IEnumerable<NetworkChartData> DataCollection
        {
            get { return (IEnumerable<NetworkChartData>)GetValue(DataCollectionProperty); }
            set { SetValue(DataCollectionProperty, value); }
        }

        public static DependencyProperty ResolutionProperty = DependencyProperty.Register("Resolution", typeof(int), typeof(NetworkChart), new FrameworkPropertyMetadata(500) { AffectsRender = true });

        public int Resolution
        {
            get { return (int)GetValue(ResolutionProperty); }
            set { SetValue(ResolutionProperty, value); }
        }

        ObservableCollection<NetworkChartData> _DataRender = new ObservableCollection<NetworkChartData>();
        public ObservableCollection<NetworkChartData> DataRender => _DataRender;

        public NetworkChartData ChartData => DataRender.LastOrDefault();


        public NetworkChart()
        {

            ClipToBounds = true;
            this.SnapsToDevicePixels = true;

            _RenderTimer = new Timer(Interval.TotalSeconds);
            _RenderTimer.AutoReset = true;
            _RenderTimer.Elapsed += _RenderTimer_Elapsed;
            _RenderTimer.Start();

        }

        private void _RenderTimer_Elapsed(object? sender, ElapsedEventArgs args)
        {
            Action dlg = () =>
            {
                if (DataCollection == null && DataCollection.Count() == 0)
                    return;

                var rawData = new List<NetworkChartData>();

                var newData = DataCollection.Where(x => IsTickNew(x)).ToArray();

                if (newData != null && newData.Length > 0)
                {
                    var time = Round(newData.Last().Time, Interval);
                    var average = Average(newData);
                    DataRender.Add(new NetworkChartData(average, time));
                }

                InvalidateVisual();
            };
            Dispatcher?.Invoke(dlg, System.Windows.Threading.DispatcherPriority.Normal);
        }

        private double Average(NetworkChartData[] datas)
        {
            var value = 0d;
            for (int i = 0; i < datas.Length; i++)
            {
                var item = datas[i];
                if (item != null)
                    value += item.Speed;
            }
            value = value / datas.Length;

            return value;
        }

        private TimeSpan Round(TimeSpan time, TimeSpan roundingInterval)
        {
            return Round(time, roundingInterval, MidpointRounding.ToEven);
        }
        private TimeSpan Round(TimeSpan time, TimeSpan roundingInterval, MidpointRounding roundingType)
        {
            return new TimeSpan(
                Convert.ToInt64(Math.Round(
                    time.Ticks / (decimal)roundingInterval.Ticks,
                    roundingType
                )) * roundingInterval.Ticks
            );
        }

        private DateTime Round(DateTime datetime, TimeSpan roundingInterval)
        {
            return new DateTime(Round((datetime - DateTime.MinValue), roundingInterval).Ticks);
        }

        private bool IsTickNew(NetworkChartData tick)
        {
            if (ChartData == null)
                return true;

            return Round(tick.Time, Interval) > ChartData.Time;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var LinePen = new Pen(LineBrush, 1);
            if (LinePen.CanFreeze)
                LinePen.Freeze();

            var step = ActualWidth / Resolution;

            var count = (int)(ActualWidth / step) + 1;

            var toRender = DataRender.TakeLast(count).Reverse().ToList();

            if(toRender.Count > 0)
            {
                var maxSpeed = toRender.MaxBy(x => x.Speed).Speed;
                double range = (maxSpeed * 1.75) - 1;
                var points = new List<Point>();
                var segments = new List<LineSegment>();

                for (int i = 0; i < toRender.Count; i++)
                {
                    var x = ActualWidth - (step * i);

                    var item = toRender[i];

                    double speedFirst = (1.0 - (item.Speed - 1) / range) * ActualHeight;

                    var point = new Point(x, speedFirst);

                    points.Add(point);

                    segments.Add(new LineSegment(point, true));
                }

                for(int i = 0; i < points.Count; i++)
                {
                    var nextIndex = i + 1;
                    var point = points[i];

                    if (nextIndex > points.Count - 1)
                        continue;

                    var nextPoint = points[nextIndex];

                    drawingContext.DrawLine(LinePen, point, nextPoint);
                }

                if(points.Count > 0)
                {
                    segments.Add(new LineSegment(new Point(points.Last().X, ActualHeight), true));
                    var figure = new PathFigure(new Point(points.First().X, ActualHeight), segments, true);
                    var geo = new PathGeometry(new[] { figure });

                    drawingContext.DrawGeometry(LineBackBrush, null, geo);
                }

            }

        }

    }
}

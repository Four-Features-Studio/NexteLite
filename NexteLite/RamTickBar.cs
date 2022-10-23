using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace NexteLite
{
    public class RamTickBar : TickBar
    {
        protected override void OnRender(DrawingContext dc)
        {
            Size size = new Size(ActualWidth, ActualHeight);
            double range = Maximum - Minimum;
            double tickLen = 0.0d;  // Height for Primary Tick (for Mininum and Maximum value)
            double tickLen2;        // Height for Secondary Tick 
            double logicalToPhysical = 1.0;
            double progression = 1.0d;
            Point startPoint = new Point(0d, 0d);
            Point endPoint = new Point(0d, 0d);

            // Take Thumb size in to account
            double halfReservedSpace = ReservedSpace * 0.5;

            switch (Placement)
            {
                case TickBarPlacement.Bottom:
                    size.Width -= ReservedSpace;
                    tickLen = size.Height;
                    startPoint = new Point(halfReservedSpace, 0d);
                    endPoint = new Point(halfReservedSpace + size.Width, 0d);
                    logicalToPhysical = size.Width / range;
                    progression = 1;
                    break;
            };

            tickLen2 = tickLen * 0.75;

            Pen pen = new Pen(Fill, 1.0d);

            var countGb = (Maximum / 1024d);
            var count = (countGb / 16) * 1024d;
            // Reduce tick interval if it is more than would be visible on the screen
            double interval = count;
            if (interval > 0.0)
            {
                double minInterval = (Maximum - Minimum) / size.Width;
                if (interval < minInterval)
                {
                    interval = minInterval;
                }
            }

            var minText = (this.Minimum / 1024d).ToString("0.#") + "G";
            var minTextFormatted = new FormattedText(minText, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdena"), 10, base.Fill);

            var maxText = (this.Maximum / 1024d).ToString("0.#") + "G";
            var maxTextFormatted = new FormattedText(maxText, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdena"), 10, base.Fill);

            dc.DrawText(minTextFormatted, new Point(startPoint.X - minTextFormatted.Width/2, 0));
            dc.DrawText(maxTextFormatted, new Point(endPoint.X - minTextFormatted.Width/2, 0));

            // Draw ticks using specified TickFrequency
            if (interval > 0.0)
            {
                for (double i = interval; i < range; i += interval)
                {
                    var value = Minimum + i;

                    if (value % 512 != 0)
                        continue;

                    var valueText = (value / 1024d).ToString("0.#") + "G";
                    var valueTextFormatted = new FormattedText(valueText, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdena"), 10, base.Fill);

                    double x = i * logicalToPhysical + startPoint.X;

                    dc.DrawText(valueTextFormatted, new Point(x - minTextFormatted.Width / 2, 0));
                }
            }
            return;
        }

        //protected override void OnRender(DrawingContext dc)
        //{
        //    FormattedText formattedText;
        //    double ramFreq = this.Maximum / 8d;
        //    int tickCount = 8;
        //    Size size = new Size(base.ActualWidth, base.ActualHeight);
        //    double tickFrequencySize = (size.Width * ramFreq / (this.Maximum - this.Minimum));
        //    int i = 0;
        //    string text = "";
        //    // Draw each tick text
        //    for (i = 0; i < tickCount; i++)
        //    {
        //        text = Convert.ToString(Convert.ToInt32(this.Minimum + ramFreq * i) / 1024, 10);
        //        if (i == 0)
        //        {
        //            formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.RightToLeft, new Typeface("Verdena"), 10, base.Fill);
        //            dc.DrawText(formattedText, new Point(((tickFrequencySize) * i), 30));
        //        }
        //        else if (i == tickCount)
        //        {
        //            formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdena"), 10, base.Fill);
        //            dc.DrawText(formattedText, new Point(((tickFrequencySize - 10) * i), 30));
        //        }
        //        else
        //        {
        //            formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdena"), 10, base.Fill);
        //            dc.DrawText(formattedText, new Point(((tickFrequencySize) * i), 30));
        //        }
        //    }

        //    base.OnRender(dc); //This is essential so that tick marks are displayed. 
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace NexteLite
{
    public class RamTickBar : TickBar
    {
        protected override void OnRender(DrawingContext dc)
        {

            Size size = new Size(base.ActualWidth, base.ActualHeight);
            int tickModifer = 4;
            double widthModifer = (this.Maximum - this.Minimum);
            if (this.Maximum >= 16 * 1024)
            {
                tickModifer = 15;
                widthModifer = this.Maximum;
            }
            else if (this.Maximum >= 8 * 1024)
            {
                tickModifer = 8;
            }
            else if (this.Maximum >= 4 * 1024)
            {
                tickModifer = 4;
            }


            int tickFreq = (int)this.TickFrequency * tickModifer;
            int tickCount = (int)((this.Maximum - this.Minimum) / tickFreq) + 1;
            if ((this.Maximum - this.Minimum) % tickFreq == 0)
                tickCount -= 1;
            Double tickFrequencySize;
            // Calculate tick's setting
            tickFrequencySize = (size.Width * tickFreq / widthModifer);
            string text = "";
            FormattedText formattedText = null;
            double num = this.Maximum - this.Minimum;
            int i = 0;
            // Draw each tick text
            for (i = 0; i <= tickCount; i++)
            {
                text = ((this.Minimum + (double)tickFreq * i) / 1024d).ToString("#.#") + "Gb" ;

                formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 8, this.Fill);
                dc.DrawText(formattedText, new Point((tickFrequencySize * i), 30));

            }
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

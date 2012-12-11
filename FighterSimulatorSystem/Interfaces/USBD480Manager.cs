using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FighterSimulatorSystem.Interfaces
{
    class USBD480Manager
    {
        // lcd stuff
        UInt32[] pixels;
        int width = 480;
        int height = 272;

        Wrapper.DisplayInfo disp;

        public USBD480Manager()
        {
            pixels = new UInt32[width * height];

            try
            {
                // init USBD480
                FSS.AddLog("Init USBD480 display");
                int result;
                disp = new Wrapper.DisplayInfo();
                int i = Wrapper.USBD480_GetNumberOfDisplays();
                if (i > 0)
                {
                    result = Wrapper.USBD480_GetDisplayConfiguration(0, ref disp);
                    result = Wrapper.USBD480_Open(ref disp, 0);
                    // turn on backlight
                    result = Wrapper.USBD480_SetBrightness(ref disp, 255);
                }
            }
            catch (Exception ex)
            {
                FSS.AddLog(ex.ToString());
            }
        }

        public void Close()
        {
            // clear display
            for (int i = 0; i < width * height; i++)
                pixels[i] = 0;
            int res = Wrapper.USBD480_DrawFullScreenRGBA32(ref disp, pixels);
            res = Wrapper.USBD480_SetBrightness(ref disp, 0);
            res = Wrapper.USBD480_Close(ref disp);
        }

        /// <summary>
        /// DrawRWR on USBD480
        /// </summary>
        private void DrawRWR()
        {
            //// Create a visual for the page.
            //DrawingVisual visual = new DrawingVisual();
            //// Get the drawing context.
            //using (DrawingContext dc = visual.RenderOpen())
            //{
            //    // draw background
            //    dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, width, height));

            //    // draw circles
            //    Point center = new Point(136, 136);
            //    Pen gridPen = new Pen(Brushes.LightCyan, 3);
            //    dc.DrawEllipse(new RadialGradientBrush(Colors.Green, Colors.Black), new Pen(Brushes.LightCyan, 5), center, 136, 136);
            //    dc.DrawEllipse(null, gridPen, center, 81, 81);
            //    dc.DrawEllipse(null, gridPen, center, 26, 26);

            //    // draw lines
            //    dc.DrawLine(gridPen, new Point(0, 136), new Point(110, 136));
            //    dc.DrawLine(gridPen, new Point(162, 136), new Point(272, 136));
            //    dc.DrawLine(gridPen, new Point(136, 0), new Point(136, 110));
            //    dc.DrawLine(gridPen, new Point(136, 162), new Point(136, 272));


            //    for (int i = 0; i < FSS.simconnect.simData.RwrObjectCount; i++)
            //    {
            //        // draw symbol
            //        int symbolSize = 16;
            //        Brush symbolBrush = Brushes.LightGreen;
            //        if (FSS.simconnect.simData.newDetection[i] == 1)
            //        {
            //            if (DateTime.UtcNow.Millisecond > 500) // flash
            //                symbolSize = 24;
            //        }

            //        bool bAircraft = false;
            //        string symbol = "";
            //        switch (FSS.simconnect.simData.RWRsymbol[i])
            //        {
            //            case 4:
            //                symbol = "M"; // missile
            //                break;
            //            case 5:
            //                symbol = "H"; // HAWK
            //                break;
            //            case 6:
            //                symbol = "P"; // PATRIOT
            //                break;
            //            case 7:
            //                symbol = "2"; // SA - 2
            //                break;
            //            case 8:
            //                symbol = "3"; // SA-3
            //                break;
            //            case 9:
            //                symbol = "4"; // SA-4
            //                break;
            //            case 10:
            //                symbol = "5"; // SA-5
            //                break;
            //            case 11:
            //                symbol = "6"; // SA-6
            //                break;
            //            case 12:
            //                symbol = "8"; // SA-8
            //                break;
            //            case 14:
            //                symbol = "10"; // SA-10
            //                break;
            //            case 111:
            //                symbol = "11"; // SA-11
            //                break;
            //            case 15:
            //                symbol = "13"; // SA-13
            //                break;
            //            case 16:
            //                symbol = "A/S"; // Mistral
            //                break;
            //            case 21:
            //                symbol = "N"; // Nike
            //                break;
            //            case 22:
            //                symbol = "A"; // Shilka
            //                break;
            //            case 117:
            //                symbol = "17"; // SA-17
            //                break;
            //            // -------------- A/C
            //            case 3:
            //                symbol = ""; // Older aircrafts
            //                bAircraft = true;
            //                break;
            //            case 32:
            //                symbol = "4"; // F-4
            //                bAircraft = true;
            //                break;
            //            case 33:
            //                symbol = "5"; // F-5
            //                bAircraft = true;
            //                break;
            //            case 35:
            //                symbol = "14"; // F-14
            //                bAircraft = true;
            //                break;
            //            case 36:
            //                symbol = "15"; // F-15
            //                bAircraft = true;
            //                break;
            //            case 37:
            //                symbol = "16"; // F-16
            //                bAircraft = true;
            //                break;
            //            case 38:
            //                symbol = "18"; // F-18
            //                bAircraft = true;
            //                break;
            //            case 41:
            //                symbol = "21"; // mig-21
            //                bAircraft = true;
            //                break;
            //            case 42:
            //                symbol = "22"; // F-22
            //                bAircraft = true;
            //                break;
            //            case 43:
            //                symbol = "23"; // Mig-23
            //                bAircraft = true;
            //                break;
            //            case 44:
            //                symbol = "25"; // Mig-25
            //                bAircraft = true;
            //                break;
            //            case 46:
            //                symbol = "29"; // Mig-29
            //                bAircraft = true;
            //                break;
            //            case 52:
            //                symbol = "B"; // B1
            //                bAircraft = true;
            //                break;
            //            case 53:
            //                symbol = "S"; // AWACS
            //                bAircraft = true;
            //                break;
            //            case 69:
            //                symbol = "E"; // Typhoon
            //                bAircraft = true;
            //                break;
            //            case 84:
            //                symbol = "T"; // Tornado
            //                bAircraft = true;
            //                break;
            //            case 108:
            //                symbol = "8"; // F-8
            //                bAircraft = true;
            //                break;
            //            case 120:
            //                symbol = "20"; // Mirage
            //                bAircraft = true;
            //                break;
            //            default: // show code
            //                symbol = FSS.simconnect.simData.RWRsymbol[i].ToString();
            //                break;
            //        }

            //        FormattedText text = new FormattedText(symbol,
            //            CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            //            new Typeface("Calibri"), symbolSize, symbolBrush);

            //        // Get the size required for the text.
            //        Size textSize = new Size(text.Width, text.Height);

            //        double fRwrBearing = FSS.simconnect.simData.bearing[i] - FSS.simconnect.simData.yaw;
            //        while (fRwrBearing < 0)
            //        {
            //            fRwrBearing = 2 * 3.14 + fRwrBearing;
            //        }

            //        if (Math.Abs(FSS.simconnect.simData.roll * 100) > (314 / 2))
            //        {
            //            fRwrBearing = 2 * 3.14 - fRwrBearing;
            //        }

            //        // Find the top-left corner where you want to place the text.
            //        Point point = new Point(
            //            center.X + 136.0 * Math.Sin(fRwrBearing) * (1 - FSS.simconnect.simData.lethality[i]) - textSize.Width / 2,
            //            center.Y - 136.0 * Math.Cos(fRwrBearing) * (1 - FSS.simconnect.simData.lethality[i]) - textSize.Height / 2);

            //        // Draw the content.
            //        dc.DrawText(text, point);

            //        // if aircraft
            //        if (bAircraft)
            //        {
            //            dc.DrawLine(gridPen, new Point(point.X, point.Y), new Point(point.X + 10, point.Y - 10));
            //            dc.DrawLine(gridPen, new Point(point.X + 10, point.Y - 10), new Point(point.X + 20, point.Y));
            //        }

            //        // if selected
            //        if (FSS.simconnect.simData.selected[i] == 1)
            //        {
            //            dc.DrawLine(gridPen, new Point(point.X - 5, point.Y + 10), new Point(point.X + 10, point.Y - 10));
            //            dc.DrawLine(gridPen, new Point(point.X + 10, point.Y - 10), new Point(point.X + 25, point.Y + 10));
            //            dc.DrawLine(gridPen, new Point(point.X - 5, point.Y + 10), new Point(point.X + 10, point.Y + 30));
            //            dc.DrawLine(gridPen, new Point(point.X + 10, point.Y + 30), new Point(point.X + 25, point.Y + 10));
            //        }

            //        // if missile activity
            //        if (FSS.simconnect.simData.missileActivity[i] == 1)
            //        {
            //            dc.DrawEllipse(null, gridPen, new Point(point.X + 8, point.Y + 8), 10, 10);
            //        }

            //        // if missile launch
            //        if (FSS.simconnect.simData.missileLaunch[i] == 1)
            //        {
            //            dc.DrawEllipse(null, gridPen, new Point(point.X + 8, point.Y + 8), 12, 12);
            //        }

            //    }

            //}

            //// copy to LCD buffer via bitmap
            //RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            //bmp.Render(visual);

            //int res = Wrapper.USBD480_DrawFullScreenRGBA32(ref disp, pixels);
        }
    }
}

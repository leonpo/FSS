using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Globalization;

namespace FighterSimulatorSystem.Interfaces
{
    class G15Manager
    {
        // G15 stuff
        LgLcd.NET.LgLcd.lgLcdConnectContext conn = new LgLcd.NET.LgLcd.lgLcdConnectContext();
        LgLcd.NET.LgLcd.lgLcdOpenContext opn = new LgLcd.NET.LgLcd.lgLcdOpenContext();
        LgLcd.NET.LgLcd.lgLcdBitmap160x43x1 bmpG15 = new LgLcd.NET.LgLcd.lgLcdBitmap160x43x1();
        UInt32[] pixels2;
        System.Drawing.Font dedFont = new System.Drawing.Font("Courier New", 8);

        public G15Manager()
        {
            try
            {
                // init G15
                FSS.AddLog("Init G15 display");

                LgLcd.NET.LgLcd.lgLcdInit();
                conn.appFriendlyName = "FSS";
                conn.isAutostartable = false;
                conn.isPersistent = false;
                conn.connection = LgLcd.NET.LgLcd.LGLCD_INVALID_CONNECTION;
                LgLcd.NET.LgLcd.lgLcdConnect(ref conn);
                opn.connection = conn.connection;
                opn.index = 1;
                LgLcd.NET.LgLcd.lgLcdOpen(ref opn);
                bmpG15.hdr = new LgLcd.NET.LgLcd.lgLcdBitmapHeader();
                bmpG15.hdr.Format = LgLcd.NET.LgLcd.LGLCD_BMP_FORMAT_160x43x1;
                bmpG15.pixels = new byte[6880];

                pixels2 = new UInt32[160 * 43];
            }
            catch (Exception ex)
            {
                FSS.AddLog(ex.ToString());
            }
        }

        public void Close()
        {
            // close G15
            FSS.AddLog("Close G15");
            LgLcd.NET.LgLcd.lgLcdClose(opn.device);
            LgLcd.NET.LgLcd.lgLcdDisconnect(conn.connection);
            LgLcd.NET.LgLcd.lgLcdDeInit();
        }

        public void Test()
        {
            // ================ draw DED on G15 =========================================
            DrawingVisual visualDED = new DrawingVisual();

            // Get the drawing context.
            using (DrawingContext dc = visualDED.RenderOpen())
            {
                //dc.DrawRectangle(Brushes.White, new Pen(Brushes.Black,1), new Rect(0,0, 160, 43));

                Typeface typeface = new Typeface(new FontFamily("Courier New"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

                // Draw the content.
                dc.DrawText(new FormattedText("0123456789012345678901234",
                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    typeface, 10, Brushes.Black),
                    new Point(4, 0));

                dc.DrawText(new FormattedText("ABCDEFGHKLMNOPRSTQUVZ0123",
                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    typeface, 50, Brushes.Black),
                    new Point(4, 8));

                dc.DrawText(new FormattedText("0123456789012345678901234",
                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    typeface, 50, Brushes.Black),
                    new Point(4, 16));

                dc.DrawText(new FormattedText("ABCD 1234500   safafa",
                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    typeface, 50, Brushes.Black),
                    new Point(4, 24));

                dc.DrawText(new FormattedText("ABCD 1234500   safafa",
                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    typeface, 50, Brushes.Black),
                    new Point(4, 32));
            }

            RenderTargetBitmap bmp2 = new RenderTargetBitmap(160, 43, 96, 96, PixelFormats.Pbgra32);
            bmp2.Render(visualDED);

            BitmapFrame bf2 = BitmapFrame.Create(bmp2);

            bf2.CopyPixels(new Int32Rect(0, 0, 160, 43), pixels2, 160 * 4, 0);

            //imageDED.Source = bf2;

        }

        /// <summary>
        /// Draw DED
        /// </summary>
        private void DrawDED()
        {
            // .NET drawing objects
            System.Drawing.Bitmap bmpNet;
            System.Drawing.Graphics gfx;

            // Graphics object
            bmpNet = new System.Drawing.Bitmap(160, 43);
            gfx = System.Drawing.Graphics.FromImage(bmpNet);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            // Clear the screen            
            gfx.FillRectangle(System.Drawing.Brushes.White, new System.Drawing.Rectangle(0, 0, 160, 43));

            // Drawing goes here.  We can do any .NET graphics object drawing we want.
            // (fonts or whatever.)
            //DrawDEDLine(gfx, FSS.simconnect.simData.DEDLine1, FSS.simconnect.simData.DEDInvertLine1, 1);
            //DrawDEDLine(gfx, FSS.simconnect.simData.DEDLine2, FSS.simconnect.simData.DEDInvertLine2, 2);
            //DrawDEDLine(gfx, FSS.simconnect.simData.DEDLine3, FSS.simconnect.simData.DEDInvertLine3, 3);
            //DrawDEDLine(gfx, FSS.simconnect.simData.DEDLine4, FSS.simconnect.simData.DEDInvertLine4, 4);
            //DrawDEDLine(gfx, FSS.simconnect.simData.DEDLine5, FSS.simconnect.simData.DEDInvertLine5, 5);


            // Convert our .NET bitmap to the format that LCD wants 
            // (treat anything non-white to be black)
            // This is *not* the most efficient way to do this.
            for (int y = 0; y < 43; y++)
            {
                for (int x = 0; x < 160; x++)
                {
                    System.Drawing.Color trueColor = bmpNet.GetPixel(x, y);
                    byte nColor = (byte)((trueColor.R == 255 && trueColor.G == 255 && trueColor.B == 255) ? 0 : 255);

                    bmpG15.pixels[y * 160 + x] = nColor;
                }
            }

            // Draw the picture.
            LgLcd.NET.LgLcd.lgLcdUpdateBitmap(opn.device, ref bmpG15, LgLcd.NET.LgLcd.LGLCD_PRIORITY_NORMAL);
        }

        /// <summary>
        /// Draw DED line 
        /// </summary>
        /// <param name="gfx"></param>
        /// <param name="line"></param>
        /// <param name="inverted"></param>
        /// <param name="p_3"></param>
        private void DrawDEDLine(System.Drawing.Graphics gfx, string line, string inverted, int row)
        {
            int left, top;
            left = 0;
            top = -4 + (row - 1) * 9;
            char[] invertedChars = inverted.ToCharArray();
            int i = 0;
            foreach (char c in line.ToCharArray())
            {
                //FSS.AddLog("Char:" + (int)c);
                switch ((int)c)
                {
                    case 1:
                        System.Drawing.Point[] points1 = { new System.Drawing.Point(left + 3, top + 7),
                                                            new System.Drawing.Point(left + 6, top + 4),
                                                            new System.Drawing.Point(left + 9, top + 7) };
                        gfx.FillPolygon(System.Drawing.Brushes.Black, points1);
                        System.Drawing.Point[] points2 = { new System.Drawing.Point(left + 4, top + 8),
                                                            new System.Drawing.Point(left + 6, top + 10),
                                                            new System.Drawing.Point(left + 8, top + 8) };
                        gfx.FillPolygon(System.Drawing.Brushes.Black, points2);
                        break;
                    case 4:
                        gfx.FillRectangle(System.Drawing.Brushes.Black, new System.Drawing.Rectangle(left + 1, top + 3, 8, 9));
                        gfx.DrawString("*", dedFont, System.Drawing.Brushes.White, left, top + 1);

                        break;
                    default:
                        if ((int)invertedChars[i] != 50) // regular 
                            gfx.DrawString(c.ToString(), dedFont, System.Drawing.Brushes.Black, left, top);
                        else // inverted
                        {
                            gfx.FillRectangle(System.Drawing.Brushes.Black, new System.Drawing.Rectangle(left + 1, top + 3, 8, 9));
                            gfx.DrawString(c.ToString(), dedFont, System.Drawing.Brushes.White, left, top);
                        }
                        break;
                }
                left += 6;
                i++;
            }

        }

    }
}

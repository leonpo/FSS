using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FighterSimulatorSystem.Interfaces
{
    class LCD2USBManager
    {
        public LCD2USBManager()
        {
            try
            {
                // init to LCD2USB display
                FSS.AddLog("Init LCD2USB display");
                bool bOk = false;
                string startparam = null;

                string res;
                res = Wrapper.DISPLAYDLL_Init((byte)20, (byte)4, startparam, ref bOk);
                FSS.AddLog(res);

                // turn backlight
                Wrapper.DISPLAYDLL_SetBacklight(true);
                Wrapper.DISPLAYDLL_SetBrightness(15);

                // show test
                if (bOk)
                {
                    FSS.AddLog("Init LCD2USB display succeded: " + res);
                    Wrapper.DISPLAYDLL_SetPosition(1, 1);
                    Wrapper.DISPLAYDLL_Write("12345678901234567890");
                    Wrapper.DISPLAYDLL_SetPosition(1, 2);
                    Wrapper.DISPLAYDLL_Write("12345678901234567890");
                    Wrapper.DISPLAYDLL_SetPosition(1, 3);
                    Wrapper.DISPLAYDLL_Write("12345678901234567890");
                    Wrapper.DISPLAYDLL_SetPosition(1, 4);
                    Wrapper.DISPLAYDLL_Write("12345678901234567890");
                }
            }
            catch (Exception ex)
            {
                FSS.AddLog(ex.ToString());
            }
        }

        public void Close()
        {
            // close lcds
            FSS.AddLog("Close LCDs");
            // turn off backlight
            Wrapper.DISPLAYDLL_SetBacklight(false);
            Wrapper.DISPLAYDLL_SetBrightness(0);
            Wrapper.DISPLAYDLL_SetPosition(1, 1);
            Wrapper.DISPLAYDLL_Write("                    ");
            Wrapper.DISPLAYDLL_SetPosition(1, 2);
            Wrapper.DISPLAYDLL_Write("                    ");
            Wrapper.DISPLAYDLL_SetPosition(1, 3);
            Wrapper.DISPLAYDLL_Write("                    ");
            Wrapper.DISPLAYDLL_SetPosition(1, 4);
            Wrapper.DISPLAYDLL_Write("                    ");
            Wrapper.DISPLAYDLL_Done();
        }

        /// <summary>
        /// Draw PFL on LCD2USB 40x4
        /// </summary>
        private void DrawPFL()
        {
            //// Draw PFL on LCD2USB
            //Wrapper.DISPLAYDLL_SetPosition(1, 1);
            //if (FSS.simconnect.simData.PFLLine1.Trim() != "")
            //    Wrapper.DISPLAYDLL_Write(FSS.simconnect.simData.PFLLine1.Substring(3, 20));

            //if (FSS.simconnect.simData.PFLLine1.Trim() == "")
            //{
            //    Wrapper.DISPLAYDLL_SetPosition(1, 1);
            //    Wrapper.DISPLAYDLL_Write(String.Format("{0,3:F0}/{1,3:F0} {2,5:F0} {3,5:F0}",
            //        FSS.simconnect.simData.distanceToBeacon,
            //        FSS.simconnect.simData.bearingToBeacon,
            //        FSS.simconnect.simData.fuelFlow,
            //        FSS.simconnect.simData.total));  // show fuel flow and total if no faults
            //}

            //Wrapper.DISPLAYDLL_SetPosition(1, 2);
            //if (FSS.simconnect.simData.PFLLine2.Trim() != "")
            //    Wrapper.DISPLAYDLL_Write(FSS.simconnect.simData.PFLLine2.Substring(3, 20));
            //else
            //    Wrapper.DISPLAYDLL_Write("                    ");
            //Wrapper.DISPLAYDLL_SetPosition(1, 3);

            //if (FSS.simconnect.simData.PFLLine3.Trim() != "")
            //    Wrapper.DISPLAYDLL_Write(FSS.simconnect.simData.PFLLine3.Substring(3, 20));
            //else
            //    Wrapper.DISPLAYDLL_Write("                    ");
            //Wrapper.DISPLAYDLL_SetPosition(1, 4);
            //if (FSS.simconnect.simData.PFLLine4.Trim() != "")
            //    Wrapper.DISPLAYDLL_Write(FSS.simconnect.simData.PFLLine4.Substring(3, 20));
            //else
            //    Wrapper.DISPLAYDLL_Write("                    ");
        }
    }
}

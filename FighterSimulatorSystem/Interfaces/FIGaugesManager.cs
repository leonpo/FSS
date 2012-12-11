using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Timers;

namespace FighterSimulatorSystem.Interfaces
{
    /// <summary>
    /// Flight Illusiona gauges manager
    /// </summary>
    public class FIGaugesManager
    {
        // serial port
        SerialPort _serialPort;
        private System.Timers.Timer timer;
        bool lights = false;
        int kohlsman = 0;       // last kohlsman read
        int vor_obs = 0;        // last vor obs read
        int heading_bug = 0;    // last heading bug read
        int last_read = 0;      // 0 - alt , 1 - vor , 2 - compass

        enum GAUGES
        {
            ALTIMETER = 101,
            ATTITUDE = 103,
            COMPASS = 105,
            VOR1 = 106,
            AIRSPEED = 255,
        }

        enum NAV_TOFROM
        {
            OFF = 0,
            TO = 1,
            FROM = 2,
        }

        enum COMMANDS
        {
            INITIALIZE = 1,
            SET_VALUE=4,
            SET_NEEDLE1_POS = 5,
            SET_NEEDLE2_POS = 6,
            READ_VALUES = 7,
            SET_LIGHTS = 8,
            SET_FLAGS = 12,
        }

        public FIGaugesManager()
        {
            try
            {
                // Create a new SerialPort object with default settings.
                _serialPort = new SerialPort();

                // Allow the user to set the appropriate properties.
                _serialPort.PortName = "COM4";
                _serialPort.BaudRate = 38400;
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;
                _serialPort.Encoding = Encoding.GetEncoding(28591); // no encoding !!!

                // Set the read/write timeouts
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;

                _serialPort.Open();

                _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);

                // subscribe to SimConnect events
                FSS.simconnect.DataReceived += new EventHandler(DataReceived);

                //// send init command
                //lock (this)
                //{
                //    byte[] cmd;
                //    byte[] buffer = new byte[30];
                //    cmd = ComposeCmd(GAUGES.ALTIMETER, COMMANDS.INITIALIZE, 0);
                //    Array.Copy(cmd, 0, buffer, 0, 6);

                //    cmd = ComposeCmd(GAUGES.AIRSPEED, COMMANDS.INITIALIZE, 0);
                //    Array.Copy(cmd, 0, buffer, 6, 6);

                //    cmd = ComposeCmd(GAUGES.ATTITUDE, COMMANDS.INITIALIZE, 0);
                //    Array.Copy(cmd, 0, buffer, 12, 6);

                //    cmd = ComposeCmd(GAUGES.COMPASS, COMMANDS.INITIALIZE, 0);
                //    Array.Copy(cmd, 0, buffer, 18, 6);

                //    cmd = ComposeCmd(GAUGES.VOR1, COMMANDS.INITIALIZE, 0);
                //    Array.Copy(cmd, 0, buffer, 24, 6);

                //    // send to gauges
                //    _serialPort.Write(buffer, 0, 30);
                //}

                // set reading thread
                // Create a timer with a 250 ms interval.
                timer = new System.Timers.Timer(250);

                // Hook up the Elapsed event for the timer.
                timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

                timer.Interval = 250;
                timer.Enabled = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        // on reading time
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            lock (this)
            {
                // send read request
                byte[] cmd=null;

                if (last_read == 0)
                    // read altitude pressure
                    cmd = ComposeCmd(GAUGES.ALTIMETER, COMMANDS.READ_VALUES, 0);

                if (last_read == 1)
                    // read VOR OBS
                    cmd = ComposeCmd(GAUGES.VOR1, COMMANDS.READ_VALUES, 0);

                if (last_read == 2)
                    // read Compass BUG
                    cmd = ComposeCmd(GAUGES.COMPASS, COMMANDS.READ_VALUES, 0);

                if (last_read++ == 2)
                    last_read = 0;

                // send to gauges
                _serialPort.Write(cmd, 0, 6);
            }
        }

        // update gauges with data from SimConnect
        private void DataReceived(object sender, EventArgs ea)
        {
            byte[] cmd, buffer;
            int val = 0;

            try
            {
                lock (this)
                {
                    // init buffer
                    buffer = new byte[60];

                    // Set altimeter (101 0x65)
                    val = (int)FSS.simconnect.simData["Altimeter_1000_footPtr"];
                    cmd = ComposeCmd(GAUGES.ALTIMETER, COMMANDS.SET_VALUE, val);
                    Array.Copy(cmd, 0, buffer, 0, 6);

                    // set attitude indicator (103 0x67)  
                    //set bank
                    val = CalculatePos(-FSS.simconnect.simData["AHorizon_Bank"] * 180 / 3.14,
                        -180, 180, 0, 3850);
                    cmd = ComposeCmd(GAUGES.ATTITUDE, COMMANDS.SET_VALUE, val);
                    Array.Copy(cmd, 0, buffer, 6, 6);

                    //set pitch
                    val = CalculatePos(-FSS.simconnect.simData["AHorizon_Pitch"] * 180 / 3.14,
                        -25, 25, 0, 660);
                    cmd = ComposeCmd(GAUGES.ATTITUDE, COMMANDS.SET_NEEDLE1_POS, val);
                    Array.Copy(cmd, 0, buffer, 12, 6);

                    // Set compass (105 0x69)
                    // set heading
                    val = CalculatePos(FSS.simconnect.simData["GyroHeading"],
                        0, 360, 0, 4350);
                    cmd = ComposeCmd(GAUGES.COMPASS, COMMANDS.SET_NEEDLE1_POS, val);
                    Array.Copy(cmd, 0, buffer, 18, 6);

                    // set bug
                    //val = CalculatePos(FSS.simconnect.simData.desiredCourse,
                    //    0, 360, 0, 4350);
                    //cmd = ComposeCmd(GAUGES.COMPASS, COMMANDS.SET_NEEDLE2_POS, val);
                    //Array.Copy(cmd, 0, buffer, 24, 6);

                    // Set VOR1 (106 0x70)
                    // set localizer needle
//                    val = CalculatePos(FSS.simconnect.simData.AdiIlsVerPos,
//                        -FSS.simconnect.simData.halfDeviationLimit, FSS.simconnect.simData.halfDeviationLimit, 0, 944);
//                    cmd = ComposeCmd(GAUGES.VOR1, COMMANDS.SET_NEEDLE1_POS, val);
//                    Array.Copy(cmd, 0, buffer, 30, 6);
//
//                    // set GS needle
//                    val = CalculatePos(FSS.simconnect.simData.AdiIlsHorPos,
//                        -FSS.simconnect.simData.halfDeviationLimit, FSS.simconnect.simData.halfDeviationLimit, 0, 944);
//                    cmd = ComposeCmd(GAUGES.VOR1, COMMANDS.SET_NEEDLE2_POS, val);
//                    Array.Copy(cmd, 0, buffer, 36, 6);
//
//                    // set flags
//                    //val = FSS.simconnect.simData.hsiBits; //0 off 1 to 2 from
//                    if ((FSS.simconnect.simData.hsiBits & (int)SimConnectManager.HsiBits.HSI_OFF) != 0)
//                        val = 0;
//                    if ((FSS.simconnect.simData.hsiBits & (int)SimConnectManager.HsiBits.ToTrue) != 0)
//                        val = 1;
//                    else
//                        val = 2;
//
//                    cmd = ComposeCmd(GAUGES.VOR1, COMMANDS.SET_FLAGS, val);
//                    Array.Copy(cmd, 0, buffer, 42, 6);

                    // set airspeed 255 (0xFF)
                    // set mach
//                    val = (int)(40 + (FSS.simconnect.simData.mach - 0.9) * 640);
//                    if (val < 0)
//                        val = 0;
//                    cmd = ComposeCmd(GAUGES.AIRSPEED, COMMANDS.SET_NEEDLE1_POS, val);
//                    Array.Copy(cmd, 0, buffer, 48, 6);

                    // set indicated airspeed
                    if (FSS.simconnect.simData["AirspeedNeedle"] < 100)
                        val = CalculatePos(FSS.simconnect.simData["AirspeedNeedle"],
                            0, 100, 0, 90);
                    if ((FSS.simconnect.simData["AirspeedNeedle"] >= 100) &&
                        (FSS.simconnect.simData["AirspeedNeedle"] < 200))
                        val = CalculatePos(FSS.simconnect.simData["AirspeedNeedle"],
                            100, 200, 90, 355);
                    if ((FSS.simconnect.simData["AirspeedNeedle"] >= 200) &&
                        (FSS.simconnect.simData["AirspeedNeedle"] < 400))
                        val = CalculatePos(FSS.simconnect.simData["AirspeedNeedle"],
                            200, 400, 355, 690);
                    if ((FSS.simconnect.simData["AirspeedNeedle"] >= 400))
                        val = CalculatePos(FSS.simconnect.simData["AirspeedNeedle"],
                            400, 800, 690, 944);
                    cmd = ComposeCmd(GAUGES.AIRSPEED, COMMANDS.SET_NEEDLE2_POS, val);
                    Array.Copy(cmd, 0, buffer, 54, 6);

                    // send to gauges
                    _serialPort.Write(buffer, 0, 60);

                    // handle lights
                    byte[] buffer2 = new byte[30];
                    if (((FSS.simconnect.mainPower > 0) && (!lights)) ||
                        ((FSS.simconnect.mainPower == 0) && lights)) // need to toggle gauges lights
                    {
                        if ((FSS.simconnect.mainPower > 0))
                            val = 192;
                        else
                            val = 0;
                        lights = (FSS.simconnect.mainPower > 0);

                        cmd = ComposeCmd(GAUGES.ALTIMETER, COMMANDS.SET_LIGHTS, val);
                        Array.Copy(cmd, 0, buffer2, 0, 6);

                        cmd = ComposeCmd(GAUGES.AIRSPEED, COMMANDS.SET_LIGHTS, val);
                        Array.Copy(cmd, 0, buffer2, 6, 6);

                        cmd = ComposeCmd(GAUGES.ATTITUDE, COMMANDS.SET_LIGHTS, val);
                        Array.Copy(cmd, 0, buffer2, 12, 6);

                        cmd = ComposeCmd(GAUGES.COMPASS, COMMANDS.SET_LIGHTS, val);
                        Array.Copy(cmd, 0, buffer2, 18, 6);

                        cmd = ComposeCmd(GAUGES.VOR1, COMMANDS.SET_LIGHTS, val);
                        Array.Copy(cmd, 0, buffer2, 24, 6);

                        // send to gauges
                        _serialPort.Write(buffer2, 0, 30);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
                System.Diagnostics.Trace.WriteLine("val:" + val);
            }
        }

        // calculate needle position
        public int CalculatePos(double val, double minVal, double maxVal, double minPos, double maxPos)
        {
            if (val < minVal) val = minVal;
            if (val > maxVal) val = maxVal;

            int pos = (int)(minPos + (val - minVal) * (maxPos - minPos) / (maxVal - minVal));

            return pos;
        }

        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //byte[] buffer = new byte[25];
            //int len = _serialPort.Read(buffer, 0, 25);
            //int val=0, val2=0;
            //if ((buffer[0] == 0) && (len == 25))  // valid read
            //{
            //    val = buffer[6] * 256 + buffer[7];
            //    val2 = buffer[9] * 256 + buffer[10];

            //    //AddLog(" FI Received " + buffer[1]+ " " +  val + " " + val2);

            //    switch ((GAUGES) buffer[1])
            //    {
            //        case GAUGES.ALTIMETER:
            //            if ((kohlsman != val) && (val!=0))
            //            { 
            //                kohlsman = val;  // save last value
            //                //val = (int)((val/100.0) * 33.8639)*16; // inHG to mb
            //                //FSS.simconnect.SetKohlsman(val/100.0f);
            //            }
            //            break;
            //        case GAUGES.VOR1:
            //            vor_obs = val;  // save last read
            //            val = 360 - val * 360 / 1082;
            //            //FSS.AddLog("VOR1 val:" + val + " desiredCourse:" + Math.Round(FSS.simconnect.simData.desiredCourse));
            //            int diff = (val - (int)FSS.simconnect.simData.desiredCourse)/5;
            //            if (diff > 0) // inc HSI course
            //                //for(int i = 0; i< diff;i++)
            //                //    FSS.simconnect.SendSimCommand("SimHsiCourseInc");
            //            if (diff < 0) // dec HSI course
            //                //for(int i = 0; i< -diff;i++)
            //                //    FSS.simconnect.SendSimCommand("SimHsiCourseDec");
            //            break;
            //        case GAUGES.COMPASS:
            //            heading_bug = val2;  // save last read
            //            val2 = val2 * 360 / 4350;
            //            //FSS.AddLog("COMPASS val2:" + val2 + " desiredHeading:" + Math.Round(FSS.simconnect.simData.desiredHeading));
            //            int diff2 = (val2 - (int)FSS.simconnect.simData.desiredHeading) / 5;
            //            if (diff2 > 0) // inc HSI heading
            //                //for(int i = 0; i<diff2;i++)
            //                //    FSS.simconnect.SendSimCommand("SimHsiHeadingInc");
            //            if (diff2< 0) // dec HSI heading
            //                //for(int i = 0; i< -diff2;i++)
            //                //    FSS.simconnect.SendSimCommand("SimHsiHeadingDec ");
            //            break;
            //    }
            //}
        }


        private byte[] ComposeCmd(GAUGES gaugeID, COMMANDS icmd, int ival)
        {
        //--------------------------------------------------------------
        // Commands from control PC
        //
        // Message layout:
        //
        // Byte 1: Flag byte:&h00
        // Byte 2: Address: 1-255; address zero forbidden!
        // Byte 3: Command: Bit 3 always "1": CCCC.1SHL (H&L = databits, S=sign)
        // Byte 4: Data Low: &h00-&hFF;  bit0 always 1 : XXXX.XXX1 -> XXXX.XXXL
        // Byte 5: Data High:&h00-&hFF: bit1 always 1 : XXXX.XX1X -> XXXX.XXHX
        // Byte 6: End byte:&hFF

        // Commands:
        //  0= noop
        //  1= Initialize
        //  2= Set Instrument Address; Value 1-&hff (not zero!),second byte:&HAA
        //  3= Set instrument speed; Value 0-&h7fff
        //  4= Set Instrument direction:lowbyte =0=CW,<>0=CCW
        //  5= New needle position; Value 0-&h7fff
        //  6= New display value: 4 bits per digit 1111222233334444
        //  7= Enquiry instrument info; Reply &h00, Address, Type, Model, Direction, "B"(16 bytes)
        //  8= Switch lights: Low byte= DL00.0000; D=display on/off, L=light on/off
        //  9= Set Instrument Model &Version, Low Byte=Model, High Byte=Version
        //  10=Write an EEPROM byte
        //
        //----------------------------------------------------------------
         
         byte[] xferCmd = new byte[6]; 
         
         // prepare GaugeId
         int int1 = (int)gaugeID;
         xferCmd[1] = (byte) int1;  
         
         // prepare value
         int long1 = ival;
         if (ival < 0)
             long1 = Math.Abs(ival);
         int int2 = (int)(long1 / 256);

         xferCmd[3] = (byte)((long1 & 0xFF) | 0x1);
         xferCmd[4] = (byte)((int2 & 0xFF) | 0x2);
         
         // prepare command
         int int3 = (int)icmd * 16;
         int3 = int3 | (long1 & 0x1);
         int3 = int3 | (int2 & 0x2);
         if  (ival < 0)
             int3 = int3 | 0xC;
         else
             int3 = int3 | 0x8;
         xferCmd[2] = (byte)(int3 & 0xFF);

         // start 0 and end FF
         xferCmd[5] = (byte)(0xFF);
         xferCmd[0] = (byte)(0x0);

         return xferCmd;
        }

        public void Close()
        {
            lock (this)
            {
                if (timer == null)
                    return;
                timer.Stop();
                timer.Enabled = false;
                if (_serialPort != null)
                    _serialPort.Close();
            }
        }
    }
}
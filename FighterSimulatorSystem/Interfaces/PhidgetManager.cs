using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

// phidgets
using Phidgets;         
using Phidgets.Events;
using System.Threading;
using System.Timers;


namespace FighterSimulatorSystem.Interfaces
{
    public class PhidgetManager
    {
        private AdvancedServo advServo;             // Declare an advancedservo object
        private LED led1;                           // LED object

        private System.Timers.Timer timer;

        /// <summary>
        ///  Ctor
        /// </summary>
        public PhidgetManager()
        {
            InitPhidgets();

            // no need, will update on timer because of slow update performance
            // subscribe to SimConnect events
            //FSS.simconnect.DataReceived += new EventHandler(DataReceived);
        }

        private void InitPhidgets()
        {
            try
            {
                FSS.AddLog("Init servo 84196");
                // init servo
                advServo = new AdvancedServo();
                advServo.Attach += new AttachEventHandler(advServo_Attach);
                advServo.Detach += new DetachEventHandler(advServo_Detach);
                advServo.Error += new ErrorEventHandler(advServo_Error);
                advServo.open(84196);
                //advServo.waitForAttachment();

                //FSS.AddLog("Init led 5524");
                //led1 = new LED();
                //led1.open(5524);
                ////led1.waitForAttachment();

                // set updating thread
                // Create a timer with a 250ms interval.
                timer = new System.Timers.Timer(250);

                // Hook up the Elapsed event for the timer.
                timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

                timer.Interval = 250;
                timer.Enabled = true;
            }
            catch (Exception ex)
            {
                FSS.AddLog("Init Phidgets: " + ex.Message);
            }
        }

        public void ClosePhidgets()
        {
            try
            {
                timer.Enabled = false;

                // clear phidgets
                advServo.Attach -= new AttachEventHandler(advServo_Attach);
                advServo.Detach -= new DetachEventHandler(advServo_Detach);
                advServo.Error -= new ErrorEventHandler(advServo_Error);

                // disengage servos
                if (advServo.Attached)
                    foreach (AdvancedServoServo servo in advServo.servos)
                    {
                        servo.Engaged = false;
                    }

                //if (led1.Attached)
                //    for (int i = 0; i < led1.leds.Count; i++)
                //        led1.leds[i] = 0;

                advServo.close();
                //led1.close();

                advServo = null;
                led1 = null;
   
            }
            catch (Exception ex)
            {
                FSS.AddLog("Close Phidgets: " + ex.Message);
            }

        }

        public void SetServoPos(AdvancedServoServo servo, double val, double minVal, double maxVal, double minPos, double maxPos, bool reversed)
        {
            try
            {
                if (val < minVal) val = minVal;
                if (val > maxVal) val = maxVal;

                if (reversed)  // reversed needle position
                    val = maxVal + minVal - val;

                double pos = minPos + (maxVal - val) * (maxPos - minPos) / (maxVal - minVal);
                servo.Position = pos;
            }
            catch (Exception ex)
            {
                FSS.AddLog(ex.ToString());
                FSS.AddLog(String.Format("val:{0} minVal:{1} maxVal:{2} minPos:{3} maxPos:{4} rec:{5}",
                    val, minVal, maxVal, minPos, maxPos, reversed));
             }
        }

        //  PhidgetServo attach event handling code
        void advServo_Attach(object sender, AttachEventArgs e)
        {
            AdvancedServo attached = (AdvancedServo)sender;
            // engage
            //this will enagage all servos - at the same time initializing all servo state
            try
            {
                foreach (AdvancedServoServo servo in attached.servos)
                {
                    servo.Type = ServoServo.ServoType.HITEC_HS322HD;
                    servo.Acceleration = servo.AccelerationMin;
                    //servo.VelocityLimit = 35000;
                    servo.Engaged = true;
                }
            }
            catch (Exception ex)
            {
                FSS.AddLog(ex.ToString());
            }
        }

        // PhidgetServo detach event handling code
        void advServo_Detach(object sender, DetachEventArgs e)
        {
            AdvancedServo detached = (AdvancedServo)sender;
        }

        void advServo_Error(object sender, ErrorEventArgs e)
        {
            FSS.AddLog(e.Description);
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            UpdatePhidgets();
        }
        
        private void UpdatePhidgets()
        {
            try
            {

                if (advServo.Attached)
                {
                    //aoa
                    SetServoPos(advServo.servos[4], 0,
                        -30f, 20f, 0f, 180f, false);

                    //vs
                    SetServoPos(advServo.servos[5], FSS.simconnect.simData["Variometer"],
                        -7000f, 3000f, 0f, 180f, true);

                    //Oil
                    SetServoPos(advServo.servos[3], FSS.simconnect.simData["Oil_Pressure"],
                        20f, 70f, 0f, 180f, true);

                    //Nozzle Position, Manifold for P-51D
                    SetServoPos(advServo.servos[1], FSS.simconnect.simData["Manifold_Pressure"],
                        10f, 85f, 0f, 180f, true);

                    //RPM
                    SetServoPos(advServo.servos[0], FSS.simconnect.simData["Engine_RPM"],
                        2500f, 8000f, 0f, 180f, true);

                    //FTIT
                    SetServoPos(advServo.servos[2], FSS.simconnect.simData["Oil_Temperature"],
                        27f, 87f, 0f, 180f, true);

                    //Fuel
                    SetServoPos(advServo.servos[6], FSS.simconnect.simData["Fuel_Tank_Left"],
                        25f, 320f, 0f, 180f, true);
                }


                //if (led1.Attached)
                //{
                //    // set AOA indexer
                //    if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.AOABelow) == 0)
                //        led1.leds[44] = 0;
                //    else
                //        led1.leds[44] = 100;
                //}


            }
            catch (Exception ex)
            {
                FSS.AddLog("Phidgets update: " + ex.ToString());
            }
        }

        private string Compress(string p)
        {
            // remove leading spaces
            while (p.StartsWith(" ") && p.Length > 20)
                p = p.Remove(0, 1);

            if (p.Length == 20)
                return p;

            string[] elements = p.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder res = new StringBuilder(20);

            foreach (string elem in elements)
            {
                res.Append(elem);
                res.Append(" ");
            }

            return (res.ToString()).Substring(0, Math.Min(20, res.Length));
        }

    }
}

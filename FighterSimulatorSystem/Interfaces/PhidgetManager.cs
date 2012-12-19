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
        // Input Changed event
        public event EventHandler InputChanged;

        private AdvancedServo advServo;             // Declare an advancedservo object
        private TextLCD lcd1, lcd2;                 // create a TextLCD objects
        private InterfaceKit ifk1, ifk2, ifk3;      // create an INterfaceKit objects
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

                // init lcds
                FSS.AddLog("Init lcd 67948");
                lcd1 = new TextLCD();
                lcd1.Attach += new AttachEventHandler(lcd_Attach);
                lcd1.Detach += new DetachEventHandler(lcd_Detach);
                lcd1.Error += new ErrorEventHandler(lcd_Error);
                lcd1.open(67948);
                //lcd1.waitForAttachment();

                FSS.AddLog("Init lcd 67935");
                lcd2 = new TextLCD();
                lcd2.Attach += new AttachEventHandler(lcd_Attach);
                lcd2.Detach += new DetachEventHandler(lcd_Detach);
                lcd2.Error += new ErrorEventHandler(lcd_Error);
                lcd2.open(67935);
                //lcd2.waitForAttachment();

                // init ifks
                FSS.AddLog("Init ifk 67948");
                ifk1 = new InterfaceKit();
                ifk1.Attach += new AttachEventHandler(ifk_Attach);
                ifk1.Detach += new DetachEventHandler(ifk_Detach);
                ifk1.Error += new ErrorEventHandler(ifk_Error);
                ifk1.InputChange += new InputChangeEventHandler(ifk_InputChange);
                ifk1.OutputChange += new OutputChangeEventHandler(ifk_OutputChange);
                ifk1.open(67948);
                //ifk1.waitForAttachment();

                FSS.AddLog("Init ifk 67935");
                ifk2 = new InterfaceKit();
                ifk2.Attach += new AttachEventHandler(ifk_Attach);
                ifk2.Detach += new DetachEventHandler(ifk_Detach);
                ifk2.Error += new ErrorEventHandler(ifk_Error);
                ifk2.InputChange += new InputChangeEventHandler(ifk_InputChange);
                ifk2.OutputChange += new OutputChangeEventHandler(ifk_OutputChange);
                ifk2.open(67935);
                //ifk2.waitForAttachment();

                FSS.AddLog("Init ifk 94600");
                ifk3 = new InterfaceKit();
                ifk3.Attach += new AttachEventHandler(ifk_Attach);
                ifk3.Detach += new DetachEventHandler(ifk_Detach);
                ifk3.Error += new ErrorEventHandler(ifk_Error);
                ifk3.InputChange += new InputChangeEventHandler(ifk_InputChange);
                ifk3.OutputChange += new OutputChangeEventHandler(ifk_OutputChange);
                ifk3.open(94600);
                //ifk3.waitForAttachment();

                FSS.AddLog("Init led 5524");
                led1 = new LED();
                led1.open(5524);
                //led1.waitForAttachment();

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

                lcd1.Attach -= new AttachEventHandler(lcd_Attach);
                lcd1.Detach -= new DetachEventHandler(lcd_Detach);
                lcd1.Error -= new ErrorEventHandler(lcd_Error);

                lcd2.Attach -= new AttachEventHandler(lcd_Attach);
                lcd2.Detach -= new DetachEventHandler(lcd_Detach);
                lcd2.Error -= new ErrorEventHandler(lcd_Error);

                ifk1.Attach -= new AttachEventHandler(ifk_Attach);
                ifk1.Detach -= new DetachEventHandler(ifk_Detach);
                ifk1.Error -= new ErrorEventHandler(ifk_Error);
                ifk1.InputChange -= new InputChangeEventHandler(ifk_InputChange);
                ifk1.OutputChange -= new OutputChangeEventHandler(ifk_OutputChange);

                ifk2.Attach -= new AttachEventHandler(ifk_Attach);
                ifk2.Detach -= new DetachEventHandler(ifk_Detach);
                ifk2.Error -= new ErrorEventHandler(ifk_Error);
                ifk2.InputChange -= new InputChangeEventHandler(ifk_InputChange);
                ifk2.OutputChange -= new OutputChangeEventHandler(ifk_OutputChange);

                ifk3.Attach -= new AttachEventHandler(ifk_Attach);
                ifk3.Detach -= new DetachEventHandler(ifk_Detach);
                ifk3.Error -= new ErrorEventHandler(ifk_Error);
                ifk3.InputChange -= new InputChangeEventHandler(ifk_InputChange);
                ifk3.OutputChange -= new OutputChangeEventHandler(ifk_OutputChange);

                //run any events in the message queue - otherwise close will hang if there are any outstanding events
                //Application.DoEvents();

                // disengage servos
                if (advServo.Attached)
                    foreach (AdvancedServoServo servo in advServo.servos)
                    {
                        servo.Engaged = false;
                    }

                // turn off LCD backlight
                if (lcd1.Attached)
                    lcd1.Backlight = false;
                if (lcd2.Attached)
                    lcd2.Backlight = false;

                // turn off leds
                if (ifk1.Attached)
                    for (int i = 0; i < ifk1.outputs.Count; i++)
                        ifk1.outputs[i] = false;
                if (ifk2.Attached)
                    for (int i = 0; i < ifk2.outputs.Count; i++)
                        ifk2.outputs[i] = false;

                if (ifk3.Attached)
                    for (int i = 0; i < ifk3.outputs.Count; i++)
                        ifk3.outputs[i] = false;

                if (led1.Attached)
                    for (int i = 0; i < led1.leds.Count; i++)
                        led1.leds[i] = 0;

                advServo.close();
                lcd1.close();
                lcd2.close();
                ifk1.close();
                ifk2.close();
                ifk3.close();
                led1.close();

                advServo = null;
                lcd1 = null;
                lcd2 = null;
                ifk1 = null;
                ifk2 = null;
                ifk3 = null;
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
                    servo.Type = ServoServo.ServoType.RAW_us_MODE;
                    servo.Acceleration = servo.AccelerationMin;
                    servo.VelocityLimit = 35000;
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

        void ifk_Attach(object sender, AttachEventArgs e)
        {

        }

        void ifk_Detach(object sender, DetachEventArgs e)
        {
        }

        //error event handler
        void ifk_Error(object sender, ErrorEventArgs e)
        {
            FSS.AddLog(e.Description);
        }

        void lcd_Attach(object sender, AttachEventArgs e)
        {
            TextLCD attached = (TextLCD)sender;
            attached.Backlight = true;

            attached.customCharacters[0].setCustomCharacter(491502, 1030144);       // open top
            attached.customCharacters[1].setCustomCharacter(479, 491502);           // open bottom

            attached.customCharacters[2].setCustomCharacter(915407, 981495);        // closed top 
            attached.customCharacters[3].setCustomCharacter(999163, 515965);        // closed bottom 
        }

        //detach event handler.... we will display the device attach status and clear all the other fields
        void lcd_Detach(object sender, DetachEventArgs e)
        {
            TextLCD detached = (TextLCD)sender;
        }

        //error event handler
        void lcd_Error(object sender, ErrorEventArgs e)
        {
            FSS.AddLog(e.Description);
        }

        //digital input change event handler... here we check or uncheck the corresponding input checkbox based on the index of
        //the digital input that generated the event
        void ifk_InputChange(object sender, InputChangeEventArgs e)
        {
        }

        //digital output change event handler... here we check or uncheck the corresponding output checkbox based on the index of
        //the output that generated the event
        void ifk_OutputChange(object sender, OutputChangeEventArgs e)
        {
        }

        protected void Fire_InputChanged(object sender)
        {
            if (InputChanged != null)
                InputChanged(this, EventArgs.Empty);
        }

        // update leds and servos
        private void DataReceived(object sender, EventArgs ea)
        {
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            UpdatePhidgets();
        }
        
        private void UpdatePhidgets()
        {
            try
            {

                if ((FSS.simconnect.mainPower > 0))
                {
                    if (lcd1.Attached)
                        lcd1.Backlight = true;
                    if (lcd2.Attached)
                        lcd2.Backlight = true;
                }
                else
                {
                    if (lcd1.Attached)
                        lcd1.Backlight = false;
                    if (lcd2.Attached)
                        lcd2.Backlight = false;

                    if (ifk1.Attached)
                        for (int i = 0; i < ifk1.outputs.Count; i++)
                            ifk1.outputs[i] = false;
                    if (ifk2.Attached)
                        for (int i = 0; i < ifk2.outputs.Count; i++)
                            ifk2.outputs[i] = false;
                    if (ifk3.Attached)
                        for (int i = 0; i < ifk3.outputs.Count; i++)
                            ifk3.outputs[i] = false;
                    if (led1.Attached)
                        for (int i = 0; i < led1.leds.Count; i++)
                            led1.leds[i] = 0;
                    return;
                }

                // check if light test is pressed
                if (FSS.simconnect.isTest)
                {
                    for (int i = 0; i < ifk1.outputs.Count; i++)
                        ifk1.outputs[i] = true;
                    for (int i = 0; i < ifk2.outputs.Count; i++)
                        ifk2.outputs[i] = true;
                    for (int i = 0; i < ifk3.outputs.Count; i++)
                        ifk3.outputs[i] = true;
                    for (int i = 0; i < led1.leds.Count; i++)
                        led1.leds[i] = 100;
                    return;
                }


                if (advServo.Attached)
                {
                    //aoa
                    SetServoPos(advServo.servos[0], 0,
                        -25f, 25f, 500f, 2100f, false);

                    //vs
                    SetServoPos(advServo.servos[1], FSS.simconnect.simData["Variometer"],
                        -6000f, 6000f, 720f, 2500f, true);

                    //Oil
                    SetServoPos(advServo.servos[2], FSS.simconnect.simData["Oil_Pressure"],
                        36f, 100f, 0f, 2730f, true);

                    //Nozzle Position, Manifold for P-51D
                    SetServoPos(advServo.servos[3], FSS.simconnect.simData["Manifold_Pressure"],
                        15f, 100f, 0f, 2730f, true);

                    //RPM
                    SetServoPos(advServo.servos[4], FSS.simconnect.simData["Engine_RPM"],
                        60f, 100f, 0f, 2730f, true);

                    ////FTIT
                    //SetServoPos(advServo.servos[5], FSS.simconnect.simData.ftit,
                    //    710f, 1000f, 0f, 2100f, true);
                    //            '     ftit value in shared mem does not fit to instrument value in 2D pit
                    //'     now ftit is dependend on rpm and fits instrument
                    //      If tempRpm < 60 Then
                    //          ftit = 240
                    //      ElseIf ((tempRpm >= 60) And (tempRpm < 70)) Then
                    //          siocIndex(mFtit) = CLng((tempRpm * 36) - 1920)
                    //      ElseIf ((tempRpm >= 70) And (tempRpm <= 93)) Then
                    //          ftit = CLng((tempRpm * 4.7826) + 265)
                    //      ElseIf (tempRpm > 93) Then
                    //          ftit = CLng((tempRpm * 6.82) + 75.91)
                    //      End If
                }


                //// display CMDS
                if (lcd1.Attached)
                {
                    lcd1.rows[0].DisplayString = "GO NO GO   DSPNS RDY";
                    lcd1.rows[1].DisplayString = String.Format("Lo Lo 0     {0:F0}   {1:F0}",
                        0, 0);
                }
 
                // display speedbrake and trims
                if (lcd1.Attached)
                {
                    //string topSB = "", bottomSB = "";
                    //if (FSS.simconnect.simData.speedBrake >= 0.5)  // open more then 50%
                    //{
                    //    topSB = lcd2.customCharacters[0].StringCode + lcd2.customCharacters[0].StringCode + lcd2.customCharacters[0].StringCode;
                    //    bottomSB = lcd2.customCharacters[1].StringCode + lcd2.customCharacters[1].StringCode + lcd2.customCharacters[1].StringCode;
                    //}

                    //if ((FSS.simconnect.simData.speedBrake > 0) && (FSS.simconnect.simData.speedBrake < 0.5))  // half open 
                    //{
                    //    topSB = lcd2.customCharacters[3].StringCode + lcd2.customCharacters[3].StringCode + lcd2.customCharacters[3].StringCode;
                    //    bottomSB = lcd2.customCharacters[0].StringCode + lcd2.customCharacters[0].StringCode + lcd2.customCharacters[0].StringCode;
                    //}


                    //if (FSS.simconnect.simData.speedBrake == 0)     // fully closed
                    //{
                    //    topSB = lcd2.customCharacters[2].StringCode + lcd2.customCharacters[2].StringCode + lcd2.customCharacters[2].StringCode;
                    //    bottomSB = lcd2.customCharacters[3].StringCode + lcd2.customCharacters[3].StringCode + lcd2.customCharacters[3].StringCode;
                    //}


                    //lcd2.rows[0].DisplayString = String.Format("   P   R   Y     {0}",
                    //    topSB);
                    //lcd2.rows[1].DisplayString = String.Format("  {0:F1} {1:F1} {2:F1}    {3}",
                    //    FSS.simconnect.simData.TrimPitch,
                    //    FSS.simconnect.simData.TrimRoll,
                    //    FSS.simconnect.simData.TrimYaw,
                    //    bottomSB);
                }


                // gear lights
                if (ifk3.Attached)
                {
                    ifk3.outputs[0] = (FSS.simconnect.simData["Landing_Gear_Handle"] > 0);
                    ifk3.outputs[2] = (FSS.simconnect.simData["Landing_Gear_Handle"] > 0);
                    ifk3.outputs[1] = (FSS.simconnect.simData["Landing_Gear_Handle"] > 0);
                }


                if (led1.Attached)
                {
                    //// set AOA indexer
                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.AOABelow) == 0)
                    //    led1.leds[44] = 0;
                    //else
                    //    led1.leds[44] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.AOAOn) == 0)
                    //    led1.leds[43] = 0;
                    //else
                    //    led1.leds[43] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.AOAAbove) == 0)
                    //    led1.leds[42] = 0;
                    //else
                    //    led1.leds[42] = 100;

                    //// set NWS indexer
                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.RefuelAR) == 0)
                    //    led1.leds[46] = 0;
                    //else
                    //    led1.leds[46] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.RefuelDSC) == 0)
                    //    led1.leds[47] = 0;
                    //else
                    //    led1.leds[47] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.RefuelRDY) == 0)
                    //    led1.leds[45] = 0;
                    //else
                    //    led1.leds[45] = 100;

                    //// set Caution Light Panel
                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.FltControlSys) == 0)
                    //    led1.leds[0] = 0;
                    //else
                    //    led1.leds[0] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.EngineFault) == 0)
                    //    led1.leds[1] = 0;
                    //else
                    //    led1.leds[1] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.Avionics) == 0)
                    //    led1.leds[2] = 0;
                    //else
                    //    led1.leds[2] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.SEAT_ARM) == 0)
                    //    led1.leds[3] = 0;
                    //else
                    //    led1.leds[3] = 100;

                    //if ((FSS.simconnect.simData.lightBits3 & (int)SimConnectManager.LightBits3.Elec_Fault) == 0) 
                    //    led1.leds[4] = 0;
                    //else
                    //    led1.leds[4] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.SEC) == 0)
                    //    led1.leds[5] = 0;
                    //else
                    //    led1.leds[5] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.Overheat) == 0) // equip hot
                    //    led1.leds[6] = 0;
                    //else
                    //    led1.leds[6] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.NWSFail) == 0)
                    //    led1.leds[7] = 0;
                    //else
                    //    led1.leds[7] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.PROBEHEAT) == 0)
                    //    led1.leds[8] = 0;
                    //else
                    //    led1.leds[8] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.FUEL_OIL_HOT) == 0)
                    //    led1.leds[9] = 0;
                    //else
                    //    led1.leds[9] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.ALT) == 0)
                    //    led1.leds[10] = 0;
                    //else
                    //    led1.leds[10] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.ANTI_SKID) == 0)
                    //    led1.leds[11] = 0;
                    //else
                    //    led1.leds[11] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.CAN) == 0)  // ?? C ADC
                    //    led1.leds[12] = 0;
                    //else
                    //    led1.leds[12] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.IFF) == 0)
                    //    led1.leds[13] = 0;
                    //else
                    //    led1.leds[13] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.Hook) == 0)
                    //    led1.leds[14] = 0;
                    //else
                    //    led1.leds[14] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.CONFIG) == 0)
                    //    led1.leds[15] = 0;
                    //else
                    //    led1.leds[15] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.Overheat) == 0)
                    //    led1.leds[16] = 0;
                    //else
                    //    led1.leds[16] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.OXY_LOW) == 0)
                    //    led1.leds[17] = 0;
                    //else
                    //    led1.leds[17] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits.LEFlaps) == 0)  // ?? EEC
                    //    led1.leds[18] = 0;
                    //else
                    //    led1.leds[18] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.CabinPress) == 0)
                    //    led1.leds[19] = 0;
                    //else
                    //    led1.leds[19] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.AftFuelLow) == 0)
                    //    led1.leds[20] = 0;
                    //else
                    //    led1.leds[20] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.FwdFuelLow) == 0)
                    //    led1.leds[21] = 0;
                    //else
                    //    led1.leds[21] = 100;

                    //// left brow indicators
                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.DUAL) == 0)
                    //    led1.leds[24] = 0;
                    //else
                    //    led1.leds[24] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.TF) == 0)
                    //    led1.leds[25] = 0;
                    //else
                    //    led1.leds[25] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.MasterCaution) == 0)
                    //    led1.leds[26] = 0;
                    //else
                    //    led1.leds[26] = 100;

                    //// right brow indicators
                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.T_L_CFG) == 0)
                    //    led1.leds[27] = 0;
                    //else
                    //    led1.leds[27] = 100;

                    //if (((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.CAN) == 0) &&
                    //    ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.OXY_LOW) == 0))
                    //    led1.leds[28] = 0;
                    //else
                    //    led1.leds[28] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.LEFlaps) == 0)
                    //    led1.leds[29] = 0;
                    //else
                    //    led1.leds[29] = 100;

                    //if ((FSS.simconnect.simData.lightBits & (int)SimConnectManager.LightBits.HYD) == 0)
                    //    led1.leds[30] = 0;
                    //else
                    //    led1.leds[30] = 100;

                    //if ((FSS.simconnect.simData.lightBits & ((int)SimConnectManager.LightBits.ENG_FIRE + (int)SimConnectManager.LightBits.EngineFault)) == 0)
                    //    led1.leds[32] = 0;
                    //else
                    //    led1.leds[32] = 100;

                    //// Threat Warn Prime
                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.HandOff) == 0)
                    //    led1.leds[36] = 0;
                    //else
                    //    led1.leds[36] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.PriMode) == 0)
                    //    led1.leds[37] = 0;
                    //else
                    //    led1.leds[37] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.TgtSep) == 0)
                    //    led1.leds[38] = 0;
                    //else
                    //    led1.leds[38] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.Launch) == 0)
                    //    led1.leds[40] = 0;
                    //else
                    //    led1.leds[40] = 100;

                    //if ((FSS.simconnect.simData.lightBits2 & ((int)SimConnectManager.LightBits2.Naval) + (int)SimConnectManager.LightBits2.Unk) == 0)
                    //    led1.leds[39] = 0;
                    //else
                    //    led1.leds[39] = 100;

                    ////if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.Unk) == 0)
                    ////    led1.leds[41] = 0;
                    ////else
                    ////    led1.leds[41] = 100;
                }

                if (ifk2.Attached)
                {
                    //// Threat Warn Aux
                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.AuxSrch) == 0)
                    //    ifk2.outputs[0] = false;
                    //else
                    //    ifk2.outputs[0] = true;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.AuxAct) == 0)
                    //    ifk2.outputs[1] = false;
                    //else
                    //    ifk2.outputs[1] = true;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.AuxLow) == 0)
                    //    ifk2.outputs[3] = false;
                    //else
                    //    ifk2.outputs[3] = true;

                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.AuxPwr) == 0)
                    //    ifk2.outputs[2] = false;
                    //else
                    //    ifk2.outputs[2] = true;
                }

                if (ifk1.Attached)
                {
                    //// ELEC PANEL
                    //if ((FSS.simconnect.simData.lightBits3 & ((int)SimConnectManager.LightBits3.FlcsPmg + (int)SimConnectManager.LightBits3.MainGen)) == 0)
                    //    ifk1.outputs[7] = false;
                    //else
                    //    ifk1.outputs[7] = true;

                    //if ((FSS.simconnect.simData.lightBits3 & (int)SimConnectManager.LightBits3.StbyGen) == 0)
                    //    ifk1.outputs[0] = false;
                    //else
                    //    ifk1.outputs[0] = true;

                    //if ((FSS.simconnect.simData.lightBits3 & ((int)SimConnectManager.LightBits3.EpuGen + (int)SimConnectManager.LightBits3.EpuPmg)) == 0)
                    //    ifk1.outputs[6] = false;
                    //else
                    //    ifk1.outputs[6] = true;

                    //if ((FSS.simconnect.simData.lightBits3 & (int)SimConnectManager.LightBits3.BatFail) == 0)
                    //    ifk1.outputs[1] = false;
                    //else
                    //    ifk1.outputs[1] = true;

                    //if ((FSS.simconnect.simData.lightBits3 & ((int)SimConnectManager.LightBits3.ToFlcs + (int) SimConnectManager.LightBits3.FlcsRly)) == 0)
                    //    ifk1.outputs[5] = false;
                    //else
                    //    ifk1.outputs[5] = true;

                    //// EPU PANEL
                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.EPUOn) == 0)
                    //    ifk1.outputs[3] = false;
                    //else
                    //    ifk1.outputs[3] = true;

                    //if ((FSS.simconnect.simData.lightBits3 & ((int)SimConnectManager.LightBits3.Hydrazine + (int)SimConnectManager.LightBits3.Air) ) == 0) // ?
                    //    ifk1.outputs[2] = false;
                    //else
                    //    ifk1.outputs[2] = true;

                    //// JET START
                    //if ((FSS.simconnect.simData.lightBits2 & (int)SimConnectManager.LightBits2.JFSOn) == 0) // ?
                    //    ifk1.outputs[4] = false;
                    //else
                    //    ifk1.outputs[4] = true;
                }
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

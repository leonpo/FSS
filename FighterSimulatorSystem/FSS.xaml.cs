using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using FighterSimulatorSystem.Interfaces;

using System.Globalization;
using System.Diagnostics;

namespace FighterSimulatorSystem
{
    /// <summary>
    /// Interaction logic for FSS.xaml
    /// </summary>
    public partial class FSS : Window
    {
        public static SimConnectManager simconnect = null;

        // private members
        private static PhidgetManager phidgets = null;
        private static FIGaugesManager figauges = null;
        private static G15Manager g15 = null;
        private static USBD480Manager d480 = null;
        private static LCD2USBManager lcd = null;

        public FSS()
        {
            InitializeComponent();

            // init trace
            Trace.Listeners.Add(new TextWriterTraceListener("fss.log"));
            Trace.AutoFlush = true;
            AddLog(" ====================== FSS start ================================ ");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (simconnect != null)
            {
                // close simconnect
                simconnect.closeConnection();

                //close phidgets
                if (phidgets != null)
                    phidgets.ClosePhidgets();

                // close serial port
                if (figauges != null)
                    figauges.Close();

                // close g15
                if (g15 != null)
                    g15.Close();

                // close d480
                if (d480 != null)
                    d480.Close();

                // close lcd
                if (lcd != null)
                    lcd.Close();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                simconnect = new SimConnectManager(this);
                simconnect.Connect();

                // init interfaces
                figauges = new FIGaugesManager();

                //phidgets = new PhidgetManager();

                //g15 = new G15Manager();

                //d480 = new USBD480LCDManager();

                //lcd = new LCD2USBManager();

                System.Windows.MessageBox.Show("Connected");
            }
            catch (Exception ex)
            {
                AddLog("Exception in btnConnect: " + ex.ToString());
                MessageBox.Show("Exception in btnConnect: " + ex.ToString());
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            g15.Test();
        }

        public static void AddLog(string message)
        {
            Trace.WriteLine(DateTime.Now + " " + message);
        }

    }
}

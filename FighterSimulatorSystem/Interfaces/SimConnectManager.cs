using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;

using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Threading;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace FighterSimulatorSystem.Interfaces
{
    public class SimConnectManager
    {
        // Data Received event
        public event EventHandler DataReceived;
        Thread bgThread;
        delegate void MyDelegate();
        Window window;

        // local state of simulator
        public bool isTest = false;                        // test is pressed
        public int mainPower = 1;                          // no main power 

        public Dictionary<string, float> simData;          // holds JSON data from the sim

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="window"></param>
        public SimConnectManager(Window window)
        {
            this.window = window;
        }

        public void closeConnection()
        {
            // dispose event handlers
            DataReceived = null;
        }


        public void Connect()
        {
            try
            {
                // create background thread for message processing
                bgThread = new Thread(new System.Threading.ThreadStart(SimMessageThread));
                bgThread.IsBackground = true;
                bgThread.Start();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Unable to connect to Simulator " + ex.Message);
                FSS.AddLog(ex.ToString());
            }
        }

        public void SimMessageThread()
        {
            TcpListener server = null;
            JavaScriptSerializer parser = new JavaScriptSerializer();
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 6000;
                //IPAddress localAddr = IPAddress.Parse("172.16.0.131");

                server = new TcpListener(IPAddress.Any, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[2000];
                String data = null;

                // Enter the listening loop. 
                while(true) 
                {
                     FSS.AddLog("Waiting for a connection... ");

                    // Perform a blocking call to accept requests. 
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();            
                    FSS.AddLog("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client. 
                    while((i = stream.Read(bytes, 0, bytes.Length))!=0) 
                    {   
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        FSS.AddLog("Received:" + data);

                        try
                        {
                            simData = parser.Deserialize<Dictionary<string, float>>(data);

                            //float airspeedNeedle = simData["AirspeedNeedle"];
                        }
                        catch (Exception ex)
                        {
                            FSS.AddLog(ex.ToString());
                        }

                        // process on UI thread
                        window.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new MyDelegate(SimMessageProcess));            
                    }

                    // Shutdown and end connection
                    client.Close();
                }  // wait for another client
            }
            catch(SocketException ex)
            {
                FSS.AddLog(ex.ToString());
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }

        private void SimMessageProcess()
        {
            try
            {
                // trigger DataReceived event
                Fire_DataReceived(this);
            }
            catch (Exception ex)
            {
                FSS.AddLog(ex.ToString());
            }
        }

        protected void Fire_DataReceived(object sender)
        {
            if (DataReceived != null)
                DataReceived(this, EventArgs.Empty);
        }
    }
}

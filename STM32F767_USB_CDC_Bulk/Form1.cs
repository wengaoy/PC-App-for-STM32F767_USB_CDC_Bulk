using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using System.Collections.ObjectModel;
using LibUsbDotNet.LudnMonoLibUsb;
using System.Threading;
using LibUsbDotNet.DeviceNotify;
using LibUsbDotNet.Descriptors;

namespace STM32F767_USB_CDC_Bulk
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public UsbDevice MyUsbDevice;
        public UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x0483, 0xC1B0);
        public UsbEndpointWriter writer;
        UsbEndpointReader reader;// = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01, 64);//ep01

        #region usb function
        public bool WriteData(byte[] buffer)
        {
            ErrorCode ec = ErrorCode.None;

            try
            {
                // open write endpoint 1.
                // UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01, EndpointType.Bulk);
                int bytesWritten;   //number of bytes written
                ec = writer.Write(buffer, 2000, out bytesWritten);//.Write(testbyte, 2000, out 2);

                Console.WriteLine("writing was " + ec + " byte writen: " + bytesWritten);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
                return false;
            }

        }
        public void ReadData(byte[] readBuffer)
        {
            ErrorCode ec = ErrorCode.None;

            try
            {
                // open read endpoint ep03.
                // UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01, 64);//ep01
                while (ec == ErrorCode.None)
                {
                    int bytesRead;

                    // If the device hasn't sent data in the last 100 milliseconds,
                    // a timeout error (ec = IoTimedOut) will occur. 
                    //  ec = reader.Read(readBuffer, 100, out bytesRead);

                    ec = reader.Read(readBuffer, 400, out bytesRead);

                    if (bytesRead == 0) throw new Exception("No more bytes!");


                    // Write that output to the console.
                    // Console.Write(Encoding.Default.GetString(readBuffer, 0, bytesRead));

                    //foreach (byte a in readBuffer)
                    //{
                    //    Console.WriteLine("ReadBuffer: " + a);

                    //}
                    //Console.WriteLine("byteRead: " + bytesRead + "\r\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message + " nothing to read.");
            }
        }

        public int OpenDev(int Dev)
        {
            ErrorCode ec = ErrorCode.None;
            if (Dev == 0)
            {
                try
                {
                    // Find and open the usb device.
                    MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                    // If the device is open and ready
                    if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                    IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                    if (!ReferenceEquals(wholeUsbDevice, null))
                    {
                        // Select config #1
                        wholeUsbDevice.SetConfiguration(1);

                        // Claim interface #0.
                        wholeUsbDevice.ClaimInterface(0);//can do the same interface 0 and 1????
                        ////set Alt interface
                        //wholeUsbDevice.SetAltInterface(1);

                        writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01, EndpointType.Bulk);
                        reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01, 64);
                    }
                    return Dev;

                }
                catch (Exception ex)
                {
                    Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
                    //MessageBox.Show((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
                    // Release interface #0.
                    //wholeUsbDevice.ReleaseInterface(0);
                    return -1;
                }

            }
            else
            {
                Console.WriteLine("Only support single USBIO Device!");
                //MessageBox.Show("Only support single USBIO Device!");
                return -1;
            }


        }
        public void CloseDev()
        {
            if (MyUsbDevice != null)
            {
                if (MyUsbDevice.IsOpen)
                {
                    // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                    // it exposes an IUsbDevice interface. If not (WinUSB) the 
                    // 'wholeUsbDevice' variable will be null indicating this is 
                    // an interface of a device; it does not require or support 
                    // configuration and interface selection.
                    IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                    if (!ReferenceEquals(wholeUsbDevice, null))
                    {
                        // Release interface #0.
                        wholeUsbDevice.ReleaseInterface(0);
                    }

                    MyUsbDevice.Close();
                }
                MyUsbDevice = null;

                // Free usb resources
                UsbDevice.Exit();

            }
        }
        #endregion



        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnWrite_ReadTest_Click(object sender, EventArgs e)
        {
            byte[] dataW = new byte[64];
            byte[] dataR = new byte[64];

            for (byte i = 0; i < 64; i++)
            {
                dataW[i] = i;
            }

            txtinfo.AppendText("Sending byte 0-63\r\n");

            WriteData(dataW);
            Thread.Sleep(500);
            ReadData(dataR);

            //CloseDev();

            txtinfo.AppendText("Reading ... \r\n");
            foreach (byte i in dataR)
            {
                txtinfo.AppendText(i.ToString() + " ");
            }
            txtinfo.AppendText("done\r\n");
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (OpenDev(0) == 0)
            {
                lblUSBstatus.Text = "STM32 USBIO found!";         
            }
            else
            {
                lblUSBstatus.Text = "STM32 USBIO not found!";
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseDev();
        }
    }
}

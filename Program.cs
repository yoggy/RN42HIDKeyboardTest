//
//  RN42HIDKeyboardTest - keyboard emulation test program for RN-42
//
//    see also...
//        RN-42 module (Microchip)
//        http://www.microchip.com/wwwproducts/en/RN42
//
//        RN-42 Bluetooth Evaluation Kit
//        http://www.microchip.com/DevelopmentTools/ProductDetails.aspx?PartNO=rn-42-ek
//
//        RN-41-EK & RN-42-EK Evaluation Kit User’s Guide
//        http://ww1.microchip.com/downloads/en/DeviceDoc/rn-4142-ek-ug-1.0.pdf
//
//        Bluetooth HID Profile
//        http://cdn.sparkfun.com/datasheets/Wireless/Bluetooth/RN-HID-User%20Guide-1.1r.pdf
//        
using System.Management;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO.Ports;
using System.Threading;
using System;

namespace RN42HIDKeyboardTest
{
    class Program
    {
        static SerialPort serial = new SerialPort();

        static void SendData(byte[] buf)
        {
            serial.Write(buf, 0, buf.Length);

            while (true)
            {
                if (serial.BytesToWrite == 0) break;
            }
        }

        // see also... (page8)http://cdn.sparkfun.com/datasheets/Wireless/Bluetooth/RN-HID-User%20Guide-1.1r.pdf
        static void SendKeyData(int key)
        {
            byte[] buf = new byte[11];
            buf[0] = 0xFD;
            buf[1] = 0x09; // length
            buf[2] = 0x01; // descriptor
            buf[3] = 0x00; // modifier key
            buf[4] = 0x00; // reserved
            buf[5] = (byte)key; // key 1 : ???
            buf[6] = 0x00; // key 2 : none
            buf[7] = 0x00; // key 3 : none
            buf[9] = 0x00; // key 4 : none
            buf[9] = 0x00; // key 5 : none
            buf[10] = 0x00; // key 6 : none

            SendData(buf);
        }

        // see also... (page11)http://cdn.sparkfun.com/datasheets/Wireless/Bluetooth/RN-HID-User%20Guide-1.1r.pdf
        static void SendConsumerKeyData(int d1, int d2)
        {
            byte[] buf = new byte[5];
            buf[0] = 0xFD;
            buf[1] = 0x03; // length
            buf[2] = 0x03; // descriptor?
            buf[3] = (byte)d1; // ???
            buf[4] = (byte)d2; // ???

            SendData(buf);
        }

        static void PushKey(int key)
        {
            SendKeyData(key); // press
            Thread.Sleep(50);
            SendKeyData(0); // release
        }

        static void PushConsuperKey(int val)
        {
            int d1 = (val & 0x000000ff);
            int d2 = (val & 0x0000ff00) >> 8;

            SendConsumerKeyData(d1, d2);
            Thread.Sleep(50);
            SendConsumerKeyData(0, 0);
        }

        static string [] GetComPortNames()
        {
            var list = new List<string>();

            ManagementClass win32_pnpentity = new ManagementClass("Win32_PnPEntity");
            ManagementObjectCollection col = win32_pnpentity.GetInstances();

            Regex reg = new Regex(".+\\((?<port>COM\\d+)\\)");

            foreach (ManagementObject obj in col)
            {
                // name : "USB Serial Port(COM??)"
                string name = (string)obj.GetPropertyValue("name");
                if (name != null && name.Contains("(COM"))
                {
                    // "USB Serial Port(COM??)" -> COM??
                    Match m = reg.Match(name);
                    string port = m.Groups["port"].Value;

                    // description : "USB Serial Port"
                    string desc = (string)obj.GetPropertyValue("Description");

                    // result string : "COM?? (USB Serial Port)"
                    list.Add(port);
                }
            }

            if (list.Count == 0)
            {
                return null;
            }

            ComPortComparer comp = new ComPortComparer();
            list.Sort(comp);

            return list.ToArray();
        }

        static void Main(string[] args)
        {
            serial.PortName = GetComPortNames()[0];
            serial.BaudRate = 115200;
            serial.DataBits = 8;
            serial.Parity = Parity.None;
            serial.StopBits = StopBits.One;
            serial.Open();

            for (int i = 0; i < 127; ++i)
            {
                Console.WriteLine(string.Format("PushKey : key={0}", i));
                PushKey(i);
                Thread.Sleep(100);
            }

            serial.Close();
        }
    }
}


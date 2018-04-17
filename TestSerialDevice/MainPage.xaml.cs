using System;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SerialDeviceLib;


namespace TestSerialDevice
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page,ISerialDeviceConfig
    {

        SerialDeviceInput _serialDevice = new SerialDeviceLib.SerialDeviceInput();
        DeviceInformation deviceInformation;
        public MainPage()
        {
            this.InitializeComponent();
            _serialDeviceConfig();
        }
        public void _serialDeviceConfig()
        {
            _serialDevice.ComPortReceived += _serialDevice_ComPortReceived; ;
            _serialDevice.GetAvailablePorts_Dis();
            _serialDevice.MessageReceived += _serialDevice_MessageReceived;
            _serialDevice.ConnectionStatusChanged += _serialDevice_ConnectionStatusChanged;
        }
        public void _serialDevice_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {
           if(args.ConnectStatus==true)
            {
                status.Text = "串口已打开";
            }
            else
            {
                status.Text = "串口已关闭";
            }
        }

        public void _serialDevice_ComPortReceived(object sender, ComPortWasFoundEventArgs args)
        {

            for (int i = 0; i < args.DeviceInformationCollection.Count; i++)
            {
                status.Text += args.DeviceInformationCollection[i].Id.ToString() + "\n";
            }
            deviceInformation = args.DeviceInformationCollection[1];
            status.Text = "串口："+deviceInformation.Id.ToString();

        }

        public  void _serialDevice_MessageReceived(object sender, MessageReceivedEventArgs args)
        {
                        //Console.WriteLine("Received message: {0}", BitConverter.ToString(args.Data));
            Console.WriteLine(args.Data.ToString());
            // On every message received we send an ACK message back to the device

            rcvdText.Text = args.Data.ToString()+_serialDevice.HexStringToAscii(args.Data.ToString()).ToString();
            
            status.Text = "接收数据：" + args.Data.ToString();
        }

        public void _serDeviceConnect()
        {
            _serialDevice.SetPort(deviceInformation.Id);
            _serialDevice.Connect();
        }
        private void comPortInput_Click(object sender, RoutedEventArgs e)
        {
            _serDeviceConnect();
            status.Text = _serialDevice.SerialDeviceInputSatus;
        }

        private void closeDevice_Click(object sender, RoutedEventArgs e)
        {
            _serialDevice.CloseDevice();
            status.Text = "串口已关闭";
        }

        private void sendTextButton_Click(object sender, RoutedEventArgs e)
        {
            _serialDevice.SendMessage_StringAsText(sendText.Text);
            _serialDevice.SendMessage_HexStringAsHexBytes(sendText.Text);
            status.Text = "发送数据："+ sendText.Text;
        }

    }
}

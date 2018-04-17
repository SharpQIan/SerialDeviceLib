using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialDeviceLib
{
    public interface ISerialDeviceConfig
    {
        /// <summary>
        /// 初始化函数
        ///  _serialDevice.ComPortReceived += _serialDevice_ComPortReceived;
        ///  _serialDevice.GetAvailablePorts_Dis();
        ///  _serialDevice.MessageReceived += _serialDevice_MessageReceived;
        ///  _serialDevice.ConnectionStatusChanged += _serialDevice_ConnectionStatusChanged;
        /// </summary>
        void _serialDeviceConfig();
        /// <summary>
        /// 串口状态改变事件
        ///    if (args.ConnectStatus == true)
        ///    {
        ///        status.Text = "串口已打开";
        ///    }
        ///    else
        ///    {
        ///        status.Text = "串口已关闭";
        ///    }
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">args.ConnectStatus串口状态</param>
        void _serialDevice_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args);
        /// <summary>
        /// 可用串口读取事件处理函数 
        /// args.DeviceInformationCollection.Count[i].ID为可用串口号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">args.DeviceInformationCollection.Count[i].ID为可用串口号</param>
        /// 
        void _serialDevice_ComPortReceived(object sender, ComPortWasFoundEventArgs args);
        /// <summary>
        /// 数据接收事件处理函数
        /// args.Data为数据对应16进制字符数组转换的字符串
        ///  转换为对应字符_serialDevice.HexStringToAscii(args.Data.ToString()).ToString()
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">args.Data为接收到的byteArray字符串</param>
        void _serialDevice_MessageReceived(object sender, MessageReceivedEventArgs args);
        /// <summary>
        /// 串口硬件连接函数
        /// 设置串口_serialDevice.SetPort(deviceInformation.Id);
        /// 连接串口 _serialDevice.Connect();
        /// </summary>
        void _serDeviceConnect();
        
        ///设置例子
        //SerialDeviceInput _serialDevice = new SerialDeviceLib.SerialDeviceInput();
        //DeviceInformation deviceInformation;
        //private void _serialDeviceConfig()
        //{
        //    _serialDevice.ComPortReceived += _serialDevice_ComPortReceived; ;
        //    _serialDevice.GetAvailablePorts_Dis();
        //    _serialDevice.MessageReceived += _serialDevice_MessageReceived;
        //    _serialDevice.ConnectionStatusChanged += _serialDevice_ConnectionStatusChanged;
        //}
        //private void _serialDevice_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        //{
        //    if (args.ConnectStatus == true)
        //    {
        //        status.Text = "串口已打开";
        //    }
        //    else
        //    {
        //        status.Text = "串口已关闭";
        //    }
        //}

        //private void _serialDevice_ComPortReceived(object sender, ComPortWasFoundEventArgs args)
        //{

        //    for (int i = 0; i < args.DeviceInformationCollection.Count; i++)
        //    {
        //        status.Text += args.DeviceInformationCollection[i].Id.ToString() + "\n";
        //    }
        //    deviceInformation = args.DeviceInformationCollection[1];
        //    status.Text = "串口：" + deviceInformation.Id.ToString();

        //}

        //private void _serialDevice_MessageReceived(object sender, MessageReceivedEventArgs args)
        //{

        //    rcvdText.Text = args.Data.ToString() + _serialDevice.HexStringToAscii(args.Data.ToString()).ToString();

        //    status.Text = "接收数据：" + args.Data.ToString();
        //}

        //private void _serDeviceConnect()
        //{
        //    _serialDevice.SetPort(deviceInformation.Id);
        //    _serialDevice.Connect();
        //}

    }
}

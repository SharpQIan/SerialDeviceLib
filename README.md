# SerialDeviceLib
UWP Serial port library , that can be used with virtual usb port. Send or recived the data and set text mode.
At the same time,I also provide an interface,so that you can use it more easily!
###SerialDeviceLib库介绍

SerialDeviceLib库使用与UWP程序中的串口通信，可用于windows平台，win10 IOT等系统的串口通信中。
该库采用事件的方式，便于针对不同应用编写不同的执行代码。主要的事件包括：

* **ComPortWasFoundEventArgs**：发现可用串口事件，当检测到可用串口硬件时返回.
* **ConnectionStatusChangedEventArgs**：连接状态改变事件，返回串口当前打开还是关闭.
* **MessageReceivedEventArgs**：数据接收事件，接收到串口数据是发生.

### SerialDeviceLib库主要方法

* **SetPort函数**：设置串口参数

```c#
public void SetPort(string portname, uint baudrate = 115200, SerialParity parity = SerialParity.None, ushort DataBits=8, SerialStopBitCount stopbits = SerialStopBitCount.One);
```

*  **Connect()函数**：完成串口连接，建立监听

```C#
 public async void Connect();
```

-  **CloseDevice()函数**：关闭串口设备

```c#
 public void CloseDevice();
```

* SendMessage_xx：串口发生数据函数_xx不同，发送数据方式不同

```c#
        /// <summary>
        /// 文本方式发送字符串
        /// </summary>
        public async void SendMessage_StringAsText(string sendStr)；	  
        
        /// <summary>
        /// 输入16进制字符串按照16进制字节数组发送
        /// </summary>
        /// <param name="sendStr">16进制字符串</param>
        public async void SendMessage_HexStringAsHexBytes(string sendStr)；

        /// <summary>
        /// byte[]数组转为对应字符串发送
        /// </summary>
        /// <param name="sendStr"></param>
        public async void SendMessage_BytesAsHexString(byte[] sendStr)
            
            
        /// <summary>
        /// byte[]数组直接发送
        /// </summary>
        /// <param name="sendBytes"></param>
        public async void SendMessage_BytesAsBytes(byte[] sendBytes)
            



```

* 常用转换方法

```C#
//十六进制字符串转换为ASCII 
public string HexStringToAscii(string hexstring);
//字节数组转换十六进制字符串
public string ByteArrayToHexString(byte[] data);
// 十六进制字符串转换字节数组
public byte[] HexStringToByteArray(string s);
```



### SerialDeviceLib库使用举例

* **MainPage.xaml.cs**

```c#
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
        }

        private void sendTextButton_Click(object sender, RoutedEventArgs e)
        {
        	//发送字符串
            _serialDevice.SendMessage_StringAsText(sendText.Text);
            //16进制字符串转对应字节数组发送
            _serialDevice.SendMessage_HexStringAsHexBytes(sendText.Text);
            
            status.Text = "发送数据："+ sendText.Text;
        }

    }
}
```

* MainPage.xaml

```xml
<Page
    x:Class="TestSerialDevice.MainPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="using:TestSerialDevice"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d">

    <Page.Resources>
        <CollectionViewSource x:Name="DeviceListSource"/>
    </Page.Resources>

    <Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="Auto"/>
        </Grid.ColumnDefinitions>

        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <TextBlock Grid.Row="0" Grid.ColumnSpan="2" x:Name="pageTitle" Text="UART Sample" Style="{StaticResource HeaderTextBlockStyle}" 
                        IsHitTestVisible="false" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="10,10,0,0"/>

        <StackPanel Grid.Row="1" Grid.Column="0" HorizontalAlignment="Center" VerticalAlignment="Center" Orientation="Horizontal" Margin="70,10,0,0">
            <TextBlock Text="Select Device:" HorizontalAlignment="Left" VerticalAlignment="Top"/>
        </StackPanel>

        <StackPanel Grid.Row="2" Grid.Column="0" HorizontalAlignment="Center" VerticalAlignment="Top" Orientation="Horizontal" Margin="70,0,0,0" Width="400" Height="80">
            <ListBox x:Name="ConnectDevices" ScrollViewer.HorizontalScrollMode="Enabled" ScrollViewer.HorizontalScrollBarVisibility="Visible" ItemsSource="{Binding Source={StaticResource DeviceListSource}}" Width="400" Height="80" Background="Gray">
                <ListBox.ItemTemplate>
                    <DataTemplate>
                        <TextBlock Text="{Binding Id}" />
                    </DataTemplate>
                </ListBox.ItemTemplate>
            </ListBox>
        </StackPanel>

        <StackPanel Grid.Row="2" Grid.Column="1" HorizontalAlignment="Left" VerticalAlignment="Center" Orientation="Vertical" Margin="30,0,0,0" Height="84" Width="94">
            <Button Name="comPortInput" Content="Connect" Click="comPortInput_Click" HorizontalAlignment="Right" Margin="0,0,19,0"/>
            <Button Name="closeDevice" Margin="0,20,0,0" Content="Disconnect" Click="closeDevice_Click"/>
        </StackPanel>

        <StackPanel Grid.Row="3" Grid.Column="0" HorizontalAlignment="Center" VerticalAlignment="Center" Orientation="Horizontal" Margin="70,10,0,0">
            <TextBlock Text="Write Data:" HorizontalAlignment="Left" VerticalAlignment="Top"/>
        </StackPanel>

        <StackPanel Grid.Row="4" Grid.Column="0" HorizontalAlignment="Center" VerticalAlignment="Top" Orientation="Horizontal" Margin="70,0,0,0" Width="300" Height="80">
            <TextBox Name="sendText" Width="300" Height="80"/>
        </StackPanel>

        <StackPanel Grid.Row="4" Grid.Column="1" HorizontalAlignment="Left" VerticalAlignment="Center" Orientation="Horizontal" Margin="30,0,0,0">
            <Button Name="sendTextButton" Content="Write" Click="sendTextButton_Click"/>
        </StackPanel>

        <StackPanel Grid.Row="5" Grid.Column="0" HorizontalAlignment="Center" VerticalAlignment="Center" Orientation="Horizontal" Margin="70,10,0,0">
            <TextBlock Text="Read Data:" HorizontalAlignment="Left" VerticalAlignment="Top"/>
        </StackPanel>

        <StackPanel Grid.Row="6" Grid.Column="0" HorizontalAlignment="Center" VerticalAlignment="Center" Orientation="Horizontal" Margin="70,0,0,0" Width="300" Height="80">
            <TextBox Name="rcvdText" Width="300" Height="80"/>
        </StackPanel>

        <StackPanel Grid.Row="7" Grid.Column="0" Grid.ColumnSpan="2" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="70,20,0,0" Width="460" Height="40">
            <TextBox
                x:Name="status" TextWrapping="Wrap" IsReadOnly="True" Width="460" Height="250" HorizontalAlignment="Left" VerticalAlignment="Top" 
                ScrollViewer.HorizontalScrollBarVisibility="Disabled" ScrollViewer.VerticalScrollBarVisibility="Auto" BorderBrush="White"/>
        </StackPanel>

    </Grid>
</Page>

```






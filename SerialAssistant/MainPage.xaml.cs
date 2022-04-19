using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SerialAssistant
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //设备串口信息
        DeviceInformationCollection serialDeviceInfos;
        //串口
        SerialDevice serialDevice;
        //串口ID
        String serialId;

        bool SerialLock = false;

        string[] baud = new string[] { "4800", "9600", "14400", "19200", "38400" ,
            "57600","115200","128000","256000","460800","512000","750000","921600","1500000"};

        string[] databit = new string[] { "5", "6", "7", "8" };

        string[] checkbit = new string[] { "Even", "Mark", "None", "Odd", "Space" };

        string[] stopbit = new string[] { "1", "1.5", "2" };

        ObservableCollection<FontFamily> fonts0 = new ObservableCollection<FontFamily>();
        ObservableCollection<FontFamily> fonts1 = new ObservableCollection<FontFamily>();
        ObservableCollection<FontFamily> fonts2 = new ObservableCollection<FontFamily>();
        ObservableCollection<FontFamily> fonts3 = new ObservableCollection<FontFamily>();
        ObservableCollection<FontFamily> fonts4 = new ObservableCollection<FontFamily>();
        ObservableCollection<FontFamily> fonts5 = new ObservableCollection<FontFamily>();

        public MainPage()
        {
            this.InitializeComponent();
            //修改背景色
            Open.Background = new SolidColorBrush(Color.FromArgb(100, 90, 90, 90));
            //波特率
            foreach (string a in baud)
            {
                fonts1.Add(new FontFamily(a));
            }
            BaudRate.SelectedItem = fonts1[6];

            //数据位

            foreach (string a in databit)
            {
                fonts2.Add(new FontFamily(a));
            }
            DataBits.SelectedItem = fonts2[3];
            //校验位
            foreach (string a in checkbit)
            {
                fonts3.Add(new FontFamily(a));
            }
            CheckBit.SelectedItem = fonts3[2];
            //停止位
            foreach (string a in stopbit)
            {
                fonts4.Add(new FontFamily(a));
            }
            StopBit.SelectedItem = fonts4[0];

            //  Debug.WriteLine("波特率"+DataBits);

            _ = StartAsync();

            ShowMessge("初始化成功");
        }


        //窗口底部提示消息
        private async Task ShowMessge(String str)
        {

            Message.Text = str;
            //        Task.Delay(2000).Wait();
            //      Message.Text = "";

        }

        //扫描串口
        private async Task StartAsync()
        {

            serialDeviceInfos = null;
            try
            {
                serialDeviceInfos = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());
            }
            catch (Exception ex) { }

            //清除列表
            int count = SerialPort.Items.Count();
            for (int i = 0; i < count; i++)
            {
                fonts0.RemoveAt(0);
            }

            foreach (DeviceInformation serialDeviceInfo in serialDeviceInfos)
            {
                //  Debug.WriteLine("扫描串口" + serialDeviceInfo.Name);
                fonts0.Add(new FontFamily(serialDeviceInfo.Name));
            }

        }

        //毛玻璃
        private void InitializeFrostedGlass(UIElement glassHost)
        {
            Visual hostVisual = ElementCompositionPreview.GetElementVisual(glassHost);
            Compositor compositor = hostVisual.Compositor;
            var backdropBrush = compositor.CreateHostBackdropBrush();
            var glassVisual = compositor.CreateSpriteVisual();
            glassVisual.Brush = backdropBrush;
            ElementCompositionPreview.SetElementChildVisual(glassHost, glassVisual);
            var bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
            bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);
            glassVisual.StartAnimation("Size", bindSizeAnimation);
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        //打开串口回调
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("打开/关闭串口");
            _ = Open_Close();
        }

        //打开关闭串口
        private async Task Open_Close()
        {

            if (SerialLock == false)
            {
                // Debug.WriteLine(BaudRate.);


                if (serialDeviceInfos == null)
                {
                    ShowMessge("没有串口");
                    return;
                }
                else
                {
                    try
                    {
                        //打开相应id的串口
                        serialDevice = await SerialDevice.FromIdAsync(serialDeviceInfos[SerialPort.SelectedIndex].Id);
                    }
                    catch
                    {
                        // ShowMessge(serialDeviceInfos[SerialPort.SelectedIndex].Name+"串口打开失败");                
                        return;
                    }

                    Debug.WriteLine(serialDeviceInfos[SerialPort.SelectedIndex].Name);
                    Debug.WriteLine("serialDevice:" + serialDevice);


                    if (serialDevice != null)
                    {
                        ShowMessge("串口已打开");
                        SerialLock = true;
                        serialId = serialDeviceInfos[SerialPort.SelectedIndex].Id;
                        Open.Content = "关闭串口";

                        Open.Background = new SolidColorBrush(Color.FromArgb(100, 0, 135, 200));


                        serialDevice.BaudRate = uint.Parse(baud[BaudRate.SelectedIndex]);
                        serialDevice.DataBits = ushort.Parse(databit[DataBits.SelectedIndex]);

                        switch (CheckBit.SelectedIndex)
                        {

                            case 0: serialDevice.Parity = SerialParity.Even; break;
                            case 1: serialDevice.Parity = SerialParity.Mark; break;
                            case 2: serialDevice.Parity = SerialParity.None; break;
                            case 3: serialDevice.Parity = SerialParity.Odd; break;
                            case 4: serialDevice.Parity = SerialParity.Space; break;
                        }
                        switch (StopBit.SelectedIndex)
                        {

                            case 0: serialDevice.StopBits = SerialStopBitCount.One; break;
                            case 1: serialDevice.StopBits = SerialStopBitCount.OnePointFive; break;
                            case 2: serialDevice.StopBits = SerialStopBitCount.Two; break;
                        }

                        Listen();

                    }
                    else
                    {
                        ShowMessge("打开串口出现错误");
                    }
                }
            }
            else
            {
                serialDevice.Dispose();
                serialDevice = null;
                SerialLock = false;
                Open.Background = new SolidColorBrush(Color.FromArgb(100, 90, 90, 90));
                Open.Content = "打开串口";
                ShowMessge("串口已关闭");
            }

        }

        //接收线程
        public async Task Listen()
        {

            if (serialDevice == null)
            {
                return;
            }
            while (true)
            {
                try
                {
                    uint length = 1;
                    IBuffer buffer = new Windows.Storage.Streams.Buffer(length);
                    await serialDevice.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.ReadAhead);
                    ReceivingBox.Text += RetrieveStringFromUtf8IBuffer(buffer);
                    ReceivingBox.Select(ReceivingBox.Text.Length, 0);
                    TextBoxScrollToEnd(ReceivingBox);
                }
                catch (Exception ex)
                {
                    break;
                }
            }

        }

        //文本滑动定位最后一行
        private void TextBoxScrollToEnd(TextBox textBox)
        {
            var grid = (Grid)VisualTreeHelper.GetChild(textBox, 0);
            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            {
                object obj = VisualTreeHelper.GetChild(grid, i);
                if (!(obj is ScrollViewer)) continue;
                ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
                break;
            }
        }

        //Ibuffer 转换
        public string RetrieveStringFromUtf8IBuffer(Windows.Storage.Streams.IBuffer theBuffer)
        {
            using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(theBuffer))
            {
                dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                return dataReader.ReadString(theBuffer.Length);
            }
        }
        //发送消息回调
        private void Send_msg(object sender, RoutedEventArgs e)
        {

            if (SendBox.Text == String.Empty)
            {
                ShowMessge("请输入要发送的内容！");
                return;
            }
            //   Debug.WriteLine(SendBox.Text);
            if (SerialLock == true)
            {
                //   Debug.WriteLine("发送");
                _ = send();
                ShowMessge("已发送");

                //  IBuffer buffer = CryptographicBuffer.ConvertStringToBinary("hello world!", BinaryStringEncoding.Utf8);
                //       str = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffer);

                //  await serialDevice.OutputStream.WriteAsync(buffer);
            }
            else
            {

                ShowMessge("串口未打开");
            }
        }
        //发送消息
        private async Task send()
        {
            //       str = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffer);
            try
            {
                IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(SendBox.Text, BinaryStringEncoding.Utf8);
                await serialDevice.OutputStream.WriteAsync(buffer);
            }
            catch (Exception ex)
            { }

        }
        //串口修改回调
        private async void serial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //  Debug.WriteLine("Selected Item Text: " + BaudRate.SelectedItem.ToString() + "  " +
            //               "Index: " + BaudRate.SelectedIndex.ToString());

            //串口关闭  下拉菜单没有选择  串口Id相同时返回
            if (SerialLock == false || SerialPort.SelectedIndex == -1 || serialId == serialDeviceInfos[SerialPort.SelectedIndex].Id)
            {
                return;
            }
            Debug.WriteLine(SerialPort.SelectedIndex);
            try
            {
                // serialDevice = await SerialDevice.FromIdAsync(serialDeviceInfos[SerialPort.SelectedIndex].Id);
                Open_Close();
                Open_Close();
            }
            catch (Exception)
            {
            }

        }
        //波特率修改回调
        private void baud_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SerialLock == false)
            {
                return;
            }
            //  Debug.WriteLine("Selected Item Text: " + BaudRate.SelectedItem.ToString() + "  " +
            //               "Index: " + BaudRate.SelectedIndex.ToString());

            Debug.WriteLine(baud[BaudRate.SelectedIndex]);
            serialDevice.BaudRate = uint.Parse(baud[BaudRate.SelectedIndex]);
        }
        //数据位修改
        private void databit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SerialLock == false)
            {
                return;
            }
            //  Debug.WriteLine("Selected Item Text: " + BaudRate.SelectedItem.ToString() + "  " +
            //               "Index: " + BaudRate.SelectedIndex.ToString());

            Debug.WriteLine(databit[DataBits.SelectedIndex]);
            serialDevice.DataBits = ushort.Parse(databit[DataBits.SelectedIndex]);
        }
        //检验位修改
        private void checkbit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SerialLock == false)
            {
                return;
            }
            //  Debug.WriteLine("Selected Item Text: " + BaudRate.SelectedItem.ToString() + "  " +
            //               "Index: " + BaudRate.SelectedIndex.ToString());

            Debug.WriteLine(checkbit[CheckBit.SelectedIndex]);

            switch (CheckBit.SelectedIndex)
            {

                case 0: this.serialDevice.Parity = SerialParity.Even; break;
                case 1: this.serialDevice.Parity = SerialParity.Mark; break;
                case 2: this.serialDevice.Parity = SerialParity.None; break;
                case 3: this.serialDevice.Parity = SerialParity.Odd; break;
                case 4: this.serialDevice.Parity = SerialParity.Space; break;
            }
        }
        //停止位修改
        private void stopbit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SerialLock == false)
            {
                return;
            }
            //  Debug.WriteLine("Selected Item Text: " + BaudRate.SelectedItem.ToString() + "  " +
            //               "Index: " + BaudRate.SelectedIndex.ToString());

            Debug.WriteLine(stopbit[StopBit.SelectedIndex]);
            switch (StopBit.SelectedIndex)
            {

                case 0: this.serialDevice.StopBits = SerialStopBitCount.One; break;
                case 1: this.serialDevice.StopBits = SerialStopBitCount.OnePointFive; break;
                case 2: this.serialDevice.StopBits = SerialStopBitCount.Two; break;
            }
        }
        //下拉事件
        private void SerialPort_DropDownOpened(object sender, object e)
        {
            // ShowMessge("串口刷新");
            _ = StartAsync();
        }

    }


}

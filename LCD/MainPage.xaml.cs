using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Microsoft.IoT.Lightning.Providers;
using Windows.Devices;
using Windows.Devices.I2c;
using System.Threading.Tasks;
using System.Text;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace LCD
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private I2cDevice LCD;
        private byte LCD_Addr = 0x3e;

        public MainPage()
        {
            this.InitializeComponent();

            Loaded += MainPage_Loaded;

        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await InitLCD();

            await DispLCD();
        }

        private async Task DispLCD()
        {
            //LCDクリア
            await WriteCmd(0x01, 100);

            //１行目を指定
            await WriteCmd(0x80, 1);
            await WriteDisp("RPi3");

            //２行目を指定
            await WriteCmd(0xc0, 1);
            await WriteDisp("ApplePi");
        }

        private async Task InitLCD()
        {
            if (LightningProvider.IsLightningEnabled)
            {
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
            }

            var i2c = await I2cController.GetDefaultAsync();

            if (i2c == null)
            {
                LCD = null;
                return;
            }

            LCD = i2c.GetDevice(new I2cConnectionSettings(LCD_Addr));

            //LCD初期化
            await Task.Delay(50);
            await WriteCmd(0x38, 1);
            await WriteCmd(0x39, 1);
            await WriteCmd(0x14, 1);
            await WriteCmd(0x71, 1);
            await WriteCmd(0x56, 1);
            await WriteCmd(0x6c, 250);
            await WriteCmd(0x38, 1);
            await WriteCmd(0x0c, 1);
            await WriteCmd(0x01, 200);
        }

        /// <summary>
        /// LCD設定コマンド
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private async Task WriteCmd(byte cmd, int time)
        {
            LCD.Write(new byte[] { 0, cmd });
            await Task.Delay(time);
        }

        /// <summary>
        /// LCD表示コマンド
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private async Task WriteDisp(string msg)
        {
            byte[] bytemsg = Encoding.ASCII.GetBytes(msg);

            for (int i = 0; i <= msg.Length - 2; i++)
            {
                LCD.Write(new byte[] { 0x40, bytemsg[i] });
            }

            LCD.Write(new byte[] { 0x40, bytemsg[msg.Length - 1] });

            await Task.Delay(1);
        }
    }
}

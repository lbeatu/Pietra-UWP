using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Networking;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Boş Sayfa öğe şablonu https://go.microsoft.com/fwlink/?LinkId=234238 adresinde açıklanmaktadır

namespace App6
{

    public sealed partial class SensorPage : Page    {
        // Bluetooth socket to send message
        private StreamSocket _socket;
        private Accelerometer _accelerometer;       
        // writer to wite on the socket
        public DataWriter _writer;
        public string sendata;
       public Double Xeksen,Yeksen;       
        List<DeviceInfo> devices = new List<DeviceInfo>();
        public DeviceInfo SelectedDevice;

        public SensorPage()
        {
            this.InitializeComponent();
           
            _accelerometer = Accelerometer.GetDefault();         
            
            InitializeBtComponentAsync();
        }

        private async Task InitializeBtComponentAsync()
        {
            deviceNames.Items.Clear();
            try
            {
                // find all the Paired devices
                PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
                var pairedDevices = await PeerFinder.FindAllPeersAsync();

                // Covert devices to ViewModel
                foreach (var device in pairedDevices)
                {
                    devices.Add(new DeviceInfo()
                    {
                        DisplayName = device.DisplayName,
                        HostName = device.HostName.RawName
                    });
                }
                deviceNames.ItemsSource = devices;
               
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147023729)
                {
                   ErrorMessage.Text="BT KAPALI";
                }
                throw;
            }
            return ;          }    
      
        public bool IsConnected
        {
            get
            {
                return (_writer != null);
            }
        }

      

        /// <summary>
        /// Connect to the device given host name
        /// </summary>
        /// <param name="deviceHostName">Raw host name of the device</param>
        /// <returns>True if connected successfully. Else False</returns>
        public async Task<bool> ConnectAsync(string deviceHostName)
        {
           // dispose of any existing socket
           /* if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }*/
          
            try
            {
                // create hostname
                HostName host = new HostName(deviceHostName);

                // create new socket and attempt to connect
                _socket = new StreamSocket();

                // if connect fails, go to Bluetooth manager settings
                // connect it manually and disconnect
                // then try again

                await _socket.ConnectAsync(host, "1");              
                // create a writer based on the socket              

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);              
                throw;
            }
        }                  
              
        private async void Button_Click(object sender, RoutedEventArgs e)
        {   if (devices.Count ==-1 || deviceNames.SelectedItem==null)
                ErrorMessage.Text = "Cihaz Seç";
            else
            
            try
            {
                int deger = deviceNames.SelectedIndex;
                ErrorMessage.Text = "Bağlanıyor...";
                SelectedDevice = devices[deger];
                await ConnectAsync(SelectedDevice.HostName);
                ErrorMessage.Text = IsConnected ? "Bağlantı Başarılı" : "Bağlantı Hatası";
               
            }
            catch (Exception ex)
            {
                // catch any exception and display it
                ErrorMessage.Text = ex.Message;
            }
            finally
            {
                ErrorMessage.Text = "Bağlandı";
             
                if (_accelerometer != null)
                {
                    // Establish the report interval
                    uint minReportInterval = _accelerometer.MinimumReportInterval;
                    uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                    _accelerometer.ReportInterval = reportInterval;

                    // Assign an event handler for the reading-changed event
                    _accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);

                }
                ConnectBtn.Visibility = Visibility.Collapsed;
                deviceNames.Visibility= Visibility.Collapsed;
                ErrorMessage.Visibility= Visibility.Collapsed;
                DisBtn.Visibility = Visibility.Visible;
                // Frame.Navigate(typeof(AcceMeter));

            }
        }

        private async void Write(string str )//Send data
        {
            
                using (DataWriter w = new DataWriter(_socket.OutputStream))
                {


                w.WriteString(str);
                    await w.StoreAsync();
                    await w.FlushAsync();
                    w.DetachStream();
                }        
           
        }
    
    private void DisBtn_Click(object sender, RoutedEventArgs e)
        {

            Write("M");//Durdur;
            ErrorMessage.Text = "Koparıldı.";
            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }

            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
            
           ConnectBtn.Visibility = Visibility.Visible;
            deviceNames.Visibility = Visibility.Visible;
            ErrorMessage.Visibility = Visibility.Visible;
            DisBtn.Visibility = Visibility.Collapsed;
        }

        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {

            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Low, async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200));
                AccelerometerReading reading = e.Reading;

                DisplayInformation displayInfo = DisplayInformation.GetForCurrentView();
                switch (displayInfo.CurrentOrientation)
                {

                    case DisplayOrientations.Landscape:
                        PointerX.Value = 100 * (e.Reading.AccelerationX);
                        PointerY.Value = 100 * (e.Reading.AccelerationY);
                        PointerZ.Value = 100 * (e.Reading.AccelerationZ);
                        Yeksen = (100 * reading.AccelerationY);
                        Xeksen = 100 * reading.AccelerationX;
                        //   Yeksen = string.Format("{0,5:0.00}", reading.AccelerationY);

                        break;
                    case DisplayOrientations.Portrait:
                        PointerX.Value = 100 * (e.Reading.AccelerationY);
                        PointerY.Value = -1 * 100 * (e.Reading.AccelerationX);
                        PointerZ.Value = 100 * (e.Reading.AccelerationZ);
                        Yeksen = -1 * 100 * (e.Reading.AccelerationX);
                        Xeksen = 100 * (e.Reading.AccelerationY);
                        break;
                    case DisplayOrientations.LandscapeFlipped:
                        PointerX.Value = -1 * 100 * (e.Reading.AccelerationX);
                        PointerY.Value = 1 * 100 * (e.Reading.AccelerationY);
                        PointerZ.Value = 100 * (e.Reading.AccelerationZ);
                        Yeksen = (100 * reading.AccelerationY);
                        Xeksen = -100 * reading.AccelerationX;                     
                        break;
                    case DisplayOrientations.PortraitFlipped:
                        PointerX.Value = -1 * 100 * (e.Reading.AccelerationY);
                        PointerY.Value = 100 * (e.Reading.AccelerationX);
                        PointerZ.Value = 100 * (e.Reading.AccelerationZ);
                        Yeksen = (100 * reading.AccelerationY);
                        Xeksen = -100 * reading.AccelerationX;
                        break;
                }
                //ileri sag-sol 1.KADEME
                if (20 < Xeksen && Xeksen < 50)
                {
                    if (-50 < Yeksen && Yeksen < -20)
                        sendata = "A";//30 derece sol
                    if (-100 < Yeksen && Yeksen < -60)
                        sendata = "B";//tam sol
                    if (20 < Yeksen && Yeksen < 45)
                        sendata = "C";//30 derece sağ
                    if (60 < Yeksen && Yeksen < 100)
                        sendata = "D";//tam sağ
                }
                //2.KADEME
                if (50 < Xeksen && Xeksen < 100)
                {
                    if (-50 < Yeksen && Yeksen < -20)
                        sendata = "E";//30 derece sol
                    if (-100 < Yeksen && Yeksen < -60)
                        sendata = "F";//tam sol
                    if (20 < Yeksen && Yeksen < 45)
                        sendata = "G";//30 derece sağ
                    if (60 < Yeksen && Yeksen < 100)
                        sendata = "H";//tam sağ
                }

                ///geri sağ-sol
           

                if (-100 < Xeksen && Xeksen < -20)
                {
                    if (-50 < Yeksen && Yeksen < -20)
                        sendata = "X";//30 derece sol
                    if (-100 < Yeksen && Yeksen < -60)
                        sendata = "Y";//tam sol
                    if (20 < Yeksen && Yeksen < 45)
                        sendata = "Z";//30 derece sağ
                    if (60 < Yeksen && Yeksen < 100)
                        sendata = "W";//tam sağ
                    if (-20 < Yeksen && Yeksen < 20)
                        sendata = "K";
                }

                if (-20 < Yeksen && Yeksen < 20)
                {
                    if (20 < Xeksen && Xeksen < 50  )
                    {
                        sendata = "O";//tam Düz 1.KADEME
                    }
                    if (50 < Xeksen && Xeksen < 100)
                    {
                        sendata = "P";//tam Düz 2.KADEME
                    }
                    if (-20 < Xeksen && Xeksen < 20)
                    {
                        sendata = "M";//Dur
                    }
                }           
                             
                ReadData.Text = sendata;             
                Write(sendata);
                
            });
        }   
     
    }
}

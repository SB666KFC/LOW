using System;
using System.Windows;
using System.Windows.Forms;
using WinForms = System.Windows.Forms;
using WPFMessageBox = System.Windows.MessageBox;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace VisionSystem
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window  // 明确指定继承自System.Windows.Window
    {
        private VideoCapture camera;
        private bool isLive = false;
        private Mat frame;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            // 初始化相机和其他组件
            frame = new Mat();
            UpdateStatus("系统初始化完成");
        }

        private void ConnectCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                camera = new VideoCapture(0); // 使用默认相机
                if (camera.IsOpened())
                {
                    UpdateStatus("相机连接成功");
                    txtCameraStatus.Text = "相机已连接";
                    btnGrab.IsEnabled = true;
                    btnLive.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show($"相机连接失败: {ex.Message}");
            }
        }

        private void Grab_Click(object sender, RoutedEventArgs e)
        {
            if (camera != null && camera.IsOpened())
            {
                camera.Read(frame);
                if (!frame.Empty())
                {
                    imgDisplay.Source = frame.ToBitmapSource();
                    UpdateStatus("图像采集成功");
                }
            }
        }

        private async void Live_Click(object sender, RoutedEventArgs e)
        {
            if (camera != null && camera.IsOpened())
            {
                isLive = true;
                btnLive.IsEnabled = false;
                btnStop.IsEnabled = true;

                while (isLive)
                {
                    camera.Read(frame);
                    if (!frame.Empty())
                    {
                        imgDisplay.Source = frame.ToBitmapSource();
                    }
                    await System.Threading.Tasks.Task.Delay(30); // 约30FPS
                }
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            isLive = false;
            btnLive.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        private void Measure_Click(object sender, RoutedEventArgs e)
        {
            if (!frame.Empty())
            {
                // 执行测量
                ProcessImage();
            }
        }

        private void ProcessImage()
        {
            try
            {
                // 图像处理和测量逻辑
                Mat grayImage = new Mat();
                Cv2.CvtColor(frame, grayImage, ColorConversionCodes.BGR2GRAY);

                if (chkEdgeDetection.IsChecked == true)
                {
                    Mat edges = new Mat();
                    Cv2.Canny(grayImage, edges, 100, 200);
                    imgDisplay.Source = edges.ToBitmapSource();
                }

                // 添加测量结果
                UpdateResults();
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show($"处理失败: {ex.Message}");
            }
        }

        private void UpdateResults()
        {
            // 更新测量结果
            var results = new System.Collections.Generic.List<MeasurementResult>();
            results.Add(new MeasurementResult { Item = "尺寸", Value = "10.5", Unit = "mm" });
            results.Add(new MeasurementResult { Item = "角度", Value = "90.0", Unit = "度" });
            dgResults.ItemsSource = results;
        }

        private void UpdateStatus(string message)
        {
            txtStatus.Text = message;
        }

        protected override void OnClosed(EventArgs e)
        {
            isLive = false;
            camera?.Dispose();
            frame?.Dispose();
            base.OnClosed(e);
        }
    }

    public class MeasurementResult
    {
        public string Item { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
    }

}

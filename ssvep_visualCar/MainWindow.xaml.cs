using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ssvep_visualCar
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private double FREQUENCY1 = 6;    // 控制键频率
        private double FREQUENCY2 = 7;    // 左伪键频率
        private double FREQUENCY3 = 8;    // 右伪键频率

        private int switchCount = 0;        // 控制键闪烁计数
        private int leftCount = 0;          // 左伪键闪烁计数
        private int rightCount = 0;         // 右伪键闪烁计数
        private bool cntnSgnl = false;       // 暂时停止符
        private bool stopSgnl = true;       // 结束符
        private bool switchEnableSgnl = true;   //
        private bool leftEnableSgnl = true;     //
        private bool rightEnableSgnl = true;    //

        string startTime;

        // 由于需要控制闪烁频率，设置委托用于线程间通信。
        public delegate void flashHandle(Image image);
        public delegate void flashHandleOnOff(Image image, double frequency);
        public delegate void showStatusHandle(string str);


        public MainWindow()
        {
            InitializeComponent();
            Thread mainThread = new Thread(new ThreadStart(mainLoop));
            mainThread.SetApartmentState(ApartmentState.MTA);   // 主线程创建，进入多线程单元
            mainThread.Start();
            ShowStatus("thread start");
        }

        private void mainLoop()
        {
            /*
            this.Dispatcher.Invoke(new flashHandle(flashOn), imageSwitch);
            Thread.Sleep(1000/FREQUENCY1);
            this.Dispatcher.Invoke(new flashHandle(flashOff), imageSwitch);
            Thread.Sleep(1000/FREQUENCY1);

            this.Dispatcher.Invoke(new flashHandleOnOff(flash), imageSwitch, FREQUENCY1);
            this.Dispatcher.Invoke(new flashHandleOnOff(flash), imageLeft, FREQUENCY2);
            this.Dispatcher.Invoke(new flashHandleOnOff(flash), imageRight, FREQUENCY3);
            flash(imageSwitch, FREQUENCY1);
            flash(imageLeft, FREQUENCY2);
            flash(imageRight, FREQUENCY3);    */

            //System.Windows.Forms.SaveFileDialog sf = new System.Windows.Forms.SaveFileDialog();
            //sf.Filter = "time(*.txt)|*.txt";
            //sf.AddExtension = true;
            //sf.Title = "time";
            //FileStream fs = new FileStream(sf.FileName, FileMode.Append); 

            Thread switchThread = new Thread(new ThreadStart(switchLoop));  // 控制键进程
            switchThread.SetApartmentState(ApartmentState.STA);
            Thread leftThread = new Thread(new ThreadStart(leftLoop));      // 左伪键进程
            leftThread.SetApartmentState(ApartmentState.STA);
            Thread rightThread = new Thread(new ThreadStart(rightLoop));    // 右伪键进程
            rightThread.SetApartmentState(ApartmentState.STA);

            startTime = DateTime.Now.ToString();       // 获取开始时间

            switchThread.Start();
            leftThread.Start(); 
            rightThread.Start();
            /*            
            while(true)
            {
                this.Dispatcher.Invoke(ShowStatus);
            }           */
        }

        private void switchLoop()   // 控制键循环
        {
            while (stopSgnl)
            {
                if (switchEnableSgnl)
                    while (cntnSgnl)
                    {
                        flash(imageSwitch, FREQUENCY1);
                        switchCount++;
                    }
            }  
        }

        private void leftLoop()     // 左伪键循环
        {
            while(stopSgnl)
            {
                if (leftEnableSgnl)
                    while (cntnSgnl)
                    {
                        flash(imageLeft, FREQUENCY2);
                        leftCount++;
                    }
            } 
        }

        private void rightLoop()    // 右伪键循环
        {
            while (stopSgnl)
            {
                if (rightEnableSgnl)
                    while (cntnSgnl)
                    {
                        flash(imageRight, FREQUENCY3);
                        rightCount++;
                    }
            }
        }

        // 显示闪烁计数，字符缺省情况
        private void ShowStatus()
        {
            switchblock.Text = "ctrl:" + switchCount;
            leftblock.Text = "left:" + leftCount;
            rightblock.Text = "rght:" + rightCount;
        }

        //显示字符，闪烁计数
        private void ShowStatus(string str)
        {
            statusStrip.Text = str;
            switchblock.Text = "ctrl:" + switchCount;
            leftblock.Text = "left:" + leftCount;
            rightblock.Text = "rght:" + rightCount;
        }

        //辅助判断用，button 用于控制三个键的集体亮，并计数清零。
        private void button_Click(object sender, RoutedEventArgs e)
        {
            //string time = DateTime.Now.ToString();       // 获取当前时间
            //File.AppendAllText(@"C:\Users\recom\Desktop\time.txt", "end:" + time + "\r\n");
            flashOff(imageSwitch);
            flashOff(imageLeft);
            flashOff(imageRight);

            double.TryParse(switchFrqncyInput.GetLineText(0).ToString(), out FREQUENCY1);
            double.TryParse(leftFrqncyInput.GetLineText(0).ToString(), out FREQUENCY2);
            double.TryParse(rightFrqncyInput.GetLineText(0).ToString(), out FREQUENCY3);

            /*
            int countDown = 3;
            for (int i=countDown; i>0; i--)
            {
                countdownBlock.Text = i.ToString();
                Thread.Sleep(1000);
            }
            countdownBlock.Text = "";
            */
            cntnSgnl = true;

            startTime = DateTime.Now.ToString();       // 获取开始时间
            switchCount = leftCount = rightCount = 0;
        }

        // 根据频率控制对应键闪烁。
        private void flash(Image image, double frequency)
        {
            //flashOff(image);
            this.Dispatcher.Invoke(new flashHandle(flashOn), image);
            Thread.Sleep((int)(1000 / frequency));
            //flashOn(image);
            this.Dispatcher.Invoke(new flashHandle(flashOff), image);
            Thread.Sleep((int)(1000 / frequency));
        }

        // 键亮
        private void flashOn(Image image)
        {
            image.Visibility = Visibility.Visible;
            ShowStatus("on");
        }

        // 键暗
        private void flashOff(Image image)
        {
            image.Visibility = Visibility.Collapsed;
            ShowStatus("off");
        }

        // stop 键，暂停闪烁，记录结果。
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            cntnSgnl = false;

            string time = DateTime.Now.ToString();       // 获取当前时间
            string msg = " ctrl:" + FREQUENCY1 + " left:" + FREQUENCY2 + " right:" + FREQUENCY3 + " ";
            if (switchEnableSgnl) msg += "1"; else msg += "0";
            if (leftEnableSgnl) msg += "1"; else msg += "0";
            if (rightEnableSgnl) msg += "1"; else msg += "0";
            File.AppendAllText(@"C:\Users\recom\Desktop\time.txt", "start:" + startTime + " end:" + time + msg + "\r\n");
        }

        // 设置控制键是否闪烁
        private void buttonSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (switchEnableSgnl)
            {
                switchEnableSgnl = false;
                switchEnableBlock.Text = "Unable";
            }
            else
            {
                switchEnableSgnl = true;
                switchEnableBlock.Text = "Enable";
            }
        }

        // 设置左伪键是否闪烁
        private void leftEnable_Click(object sender, RoutedEventArgs e)
        {
            if (leftEnableSgnl)
            {
                leftEnableSgnl = false;
                leftEnableBlock.Text = "Unable";
            }
            else
            {
                leftEnableSgnl = true;
                leftEnableBlock.Text = "Enable";
            }
        }

        // 设置右伪键是否闪烁
        private void rightEnable_Click(object sender, RoutedEventArgs e)
        {
            if (rightEnableSgnl)
            {
                rightEnableSgnl = false;
                rightEnableBlock.Text = "Unable";
            }
            else
            {
                rightEnableSgnl = true;
                rightEnableBlock.Text = "Enable";
            }
        }

        // 结束所有进程跳出循环，使前面的 start 和 stop 失效。
        private void endBttn_Click(object sender, RoutedEventArgs e)
        {
            stopSgnl = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

namespace locationserver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> arguments = new List<string>();
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private Server myserver = new Server();
        
        public string outter = null;

        public MainWindow()
        {
            InitializeComponent();
            //consol.Text;
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            worker.RunWorkerAsync();
            worker.DoWork += worker_DoWork;
            consol.Text += "Server started\r\n";
            start.IsEnabled = false;
            saveLog.IsEnabled = false;
            SaveDb.IsEnabled = false;
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e) //start server
        {
            myserver.Main(arguments.ToArray());
        }

        private void sendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

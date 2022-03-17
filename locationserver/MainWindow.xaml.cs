using Microsoft.Win32;
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
        private BackgroundWorker worker = new BackgroundWorker();
        private Server myserver = new Server();
        private string LogPath = null;
        private string DBPath = null;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += bgw_Complete;
            worker.RunWorkerAsync();
            
            
            consol.Text += "Server started\r\n";
            start.IsEnabled = false;
            saveLog.IsEnabled = false;
            SaveDb.IsEnabled = false;
            stop.IsEnabled = true;
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void worker_DoWork(object sender, DoWorkEventArgs e) //start server
        {
            string lg;
            myserver.UIMode = true;
            myserver.Main(arguments.ToArray());
            while (true)
            {
                myserver.connection = myserver.listener.AcceptSocket();
                Server.Handler RequestHandler = new Server.Handler();
                //RequestHandler.logPath = myserver.logPath;
                //RequestHandler.dbPath = myserver.dbPath;
                RequestHandler.doRequest(myserver.connection, out lg, myserver.personLocation,LogPath,DBPath);
                this.Dispatcher.Invoke(() => {consol.Text += "New Connection\r\n";});
                this.Dispatcher.Invoke(() => {consol.Text += lg + "\r\n";});
                this.Dispatcher.Invoke(() => {consol.Text += $"[Disconnected]\r\n"; });
            }
        }

        void bgw_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled) MessageBox.Show("Worker cancelled");
        }

        private void sendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void saveLog_Click(object sender, RoutedEventArgs e)
        {
            if (saveLog.IsChecked==true)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text file (*.txt)|*.txt";
                saveFileDialog.ShowDialog();
                string path = saveFileDialog.ToString();
                path = path.Remove(0, 51);
                LogPath = path;
            }
            else
            {
                saveLog.IsChecked = false;
                LogPath = null;
            }
        }

        private void SaveDb_Click(object sender, RoutedEventArgs e)
        {
            if (SaveDb.IsChecked == true)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text file (*.txt)|*.txt";
                saveFileDialog.ShowDialog();
                string path = saveFileDialog.ToString();
                path = path.Remove(0, 51);
                DBPath = path;
                //arguments.Add("-f");
                //arguments.Add(path);
            }
            else
            {
                saveLog.IsChecked = false;
                DBPath = null;
                //arguments.RemoveAt(1 + arguments.IndexOf("-f"));
                //arguments.RemoveAt(arguments.IndexOf("-f"));
            }
        }
    }
}

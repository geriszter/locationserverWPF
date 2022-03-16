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
        
        public string outter = null;

        public MainWindow()
        {
            InitializeComponent();
            myserver._response = "";
            this.DataContext = myserver;
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
            myserver.Main(arguments.ToArray());
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

                arguments.Add("-l");
                arguments.Add(path);
            }
            else
            {
                saveLog.IsChecked = false;
                arguments.RemoveAt(1 + arguments.IndexOf("-l"));
                arguments.RemoveAt(arguments.IndexOf("-l"));
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

                arguments.Add("-f");
                arguments.Add(path);
            }
            else
            {
                saveLog.IsChecked = false;
                arguments.RemoveAt(1 + arguments.IndexOf("-f"));
                arguments.RemoveAt(arguments.IndexOf("-f"));
            }
        }
    }
}

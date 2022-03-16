using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly BackgroundWorker worker = new BackgroundWorker();
        public Thread server;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            Server myserver = new Server();
            List<string> arguments = new List<string>();
            //myserver.Main(arguments.ToArray());
            server =new Thread(() => myserver.Main(arguments.ToArray()));
            server.Start();

            worker.RunWorkerAsync();
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) //recieve data
        {

        }
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e) // send data
        {

        }
    }
}

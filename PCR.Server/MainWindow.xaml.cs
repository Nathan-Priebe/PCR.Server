using Microsoft.Owin.Hosting;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using NetFwTypeLib;

namespace PCRServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private bool _serverRunning;
        private IDisposable _webApp;
        private Task _t;
        public MainWindow()
        {
            InitializeComponent();
            var ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("icon.ico");
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };

            var clientIpAdddress = IPTextBox.Text;

            _t = new Task(delegate () { StartOwinServer(clientIpAdddress); });
            _t.Start();

            try
            {
                _t.Wait();
            }
            catch(Exception ex)
            {
                ErrorOutput.Visibility = Visibility.Visible;
                ErrorOutput.Content = ex.InnerException?.Message;
                MessageBox.Show(ex.InnerException?.Message, "Error starting server",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Common.LogError(ex.Message + Environment.NewLine + ex.InnerException?.Message);
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void StartOwinServer(string clientIpAdddress)
        {
            var baseAddress = GetDeviceIP();
            if (string.IsNullOrEmpty(baseAddress) && string.IsNullOrEmpty(clientIpAdddress))
            {
                _serverRunning = false;
                throw new Exception("Unable to retrieve device IP address");
            }

            if (string.IsNullOrEmpty(baseAddress))
            {
                baseAddress = clientIpAdddress;
            }

            try
            {
                OpenFireWallPort();
            }
            catch (Exception ex)
            {
                ErrorOutput.Visibility = Visibility.Visible;
                ErrorOutput.Content = ex.InnerException?.Message;
                MessageBox.Show(ex.InnerException?.Message, "Error starting server",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Common.LogError(ex.Message + Environment.NewLine + ex.InnerException?.Message);
            }
            

            //this.IPTextBox.((MethodInvoker) delegate() { this.IPTextBox.Text = baseAddress });

            //IPTextBox.Text = baseAddress;
            //IPTextBox.IsEnabled = false;

            baseAddress = "http://" + baseAddress + ":4222";
            using (_webApp = WebApp.Start<Startup>(baseAddress))
            {
                while (true)
                {
                        
                }
            }
        }

        private void OpenFireWallPort()
        {
            var tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            var fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);
            var currentProfiles = fwPolicy2.CurrentProfileTypes;

            // Let's create a new rule
            var inboundRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            inboundRule.Enabled = true;
            //Allow through firewall
            inboundRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            //Using protocol TCP
            inboundRule.Protocol = 6; // TCP
            inboundRule.LocalPorts = "4222";
            //Name of rule
            inboundRule.Name = "PCR.Server Port";
            inboundRule.Profiles = currentProfiles;

            var firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallPolicy.Rules.Add(inboundRule);
        }

        private string GetDeviceIP()
        {
            IPHostEntry host;
            var localIp = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIp = ip.ToString();
                }
            }
            return localIp;
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var clientIpAdddress = IPTextBox.Text;
                _t = new Task(delegate () { StartOwinServer(clientIpAdddress); });
                _t.Start();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Unable to start server at this time, please try again later", "Error starting server",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Common.LogError("Unable to start server at this time, please try again later" + Environment.NewLine + exception.Message);
            }
            
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPTextBox.IsEnabled = true;
                _webApp.Dispose();
                _t.Dispose();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Unable to stop server at this time, please try again later", "Error stopping server",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Common.LogError("Unable to stop server at this time, please try again later" + Environment.NewLine + exception.Message);
            }
        }
    }
}

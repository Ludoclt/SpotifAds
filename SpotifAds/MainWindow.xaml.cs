using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace SpotifAds
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Settings settings = new Settings();
        private ProxyHandler proxyHandler = new ProxyHandler();

        private RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

        public MainWindow()
        {
            string userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (!Assembly.GetExecutingAssembly().Location.Contains(userDirectory))
            {
                AlertWindow alertWindow = new AlertWindow();
                alertWindow.AlertTitle.Text = "Directory Alert";
                alertWindow.AlertContent.Text = "SpotifAds will not work correctly if it is not installed in the user folder, please reinstall it in the default folder";
                alertWindow.ShowDialog();

                App.Current.Shutdown();
            }

            String processName = Process.GetCurrentProcess().ProcessName;

            if (Process.GetProcesses().Count(p => p.ProcessName == processName) > 2)
            {
                App.Current.Shutdown();
            }

            else if (Process.GetProcesses().Count(p => p.ProcessName == processName) > 1)
            {
                AlertWindow alertWindow = new AlertWindow();
                alertWindow.AlertTitle.Text = "Execution Alert";
                alertWindow.AlertContent.Text = "SpotifAds is already running, please check in the taskbar before running the app";
                alertWindow.ShowDialog();

                App.Current.Shutdown();
            }

            InitializeComponent();
            settings = settings.Read();
            Visibility = settings.WindowVisibility;
            ProxyPort.Text = settings.Port;
            ProxySwitch.IsChecked = settings.ProxyEnabled;
            StartupSwitch.IsChecked = settings.LaunchAtStartup;
            TraySwitch.IsChecked = settings.MinimizeWhenClosed;
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (TraySwitch.IsChecked ?? true)
            {
                WindowState = WindowState.Minimized;
                Hide();
                settings.WindowVisibility = Visibility.Hidden;
                settings.Save();
            }
            else
            {
                settings.WindowVisibility = Visibility.Visible;
                CloseApp();
            }
        }

        private void CloseApp(object? sender = null, RoutedEventArgs? e = null)
        {
            settings.Save();
            Close();
        }

        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private async void UserEnableProxy(object sender, RoutedEventArgs e)
        {
            if (sender.GetType().Name == "MenuItem")
            {
                ProxySwitch.IsChecked = true;
            }
            else
            {
                ProxyMenuItem.IsChecked = true;

                if (!File.Exists(Assembly.GetExecutingAssembly().Location.Replace("SpotifAds.dll", "rootCert.pfx")))
                {
                    AlertWindow alertWindow = new AlertWindow();
                    alertWindow.AlertTitle.Text = "Certificate Alert";
                    alertWindow.AlertContent.Text = "Please accept the following certificate in order to make SpotifAds work";
                    alertWindow.ShowDialog();
                }

                await proxyHandler.EnableProxy(Int32.Parse(settings.Port));
            }
        }

        private async void UserDisableProxy(object sender, RoutedEventArgs e)
        {
            if (sender.GetType().Name == "MenuItem")
            {
                ProxySwitch.IsChecked = false;
            }
            else
            {
                ProxyMenuItem.IsChecked= false;
                await proxyHandler.DisableProxy();
            }
        }

        private void UserEnableAutoStart(object sender, RoutedEventArgs e)
        {
            if (sender.GetType().Name == "MenuItem")
            {
                StartupSwitch.IsChecked = true;
            }
            else
            {
                StartupMenuItem.IsChecked = true;
                string location = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
                registryKey.SetValue(Assembly.GetExecutingAssembly().GetName().Name, location);
            }
        }

        private void UserDisableAutoStart(object sender, RoutedEventArgs e)
        {
            if (sender.GetType().Name == "MenuItem")
            {
                StartupSwitch.IsChecked = false;
            }
            else
            {
                StartupMenuItem.IsChecked = false;
                registryKey.DeleteValue(Assembly.GetExecutingAssembly().GetName().Name);
            }
        }

        private void OnPortChangedEventHandler(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (ProxyPort.Text == "")
            {
                settings.Port = "2468";
            }
            else
            {
                settings.Port = ProxyPort.Text;
            }
            settings.Save();
        }

        private void ProxySwitchEventHandler(object sender, RoutedEventArgs e)
        {
            settings.ProxyEnabled = (bool)ProxySwitch.IsChecked;
            settings.Save();
        }

        private void StartupSwitchEventHandler(object sender, RoutedEventArgs e)
        {
            settings.LaunchAtStartup = (bool)StartupSwitch.IsChecked;
            settings.Save();
        }

        private void TraySwitchEventHandler(object sender, RoutedEventArgs e)
        {
            settings.MinimizeWhenClosed = (bool)TraySwitch.IsChecked;
            settings.Save();
        }
    }

    public class ShowUpCommand : ICommand
    {
        private Window mainWindow = Application.Current.MainWindow;
        private static Settings settings = new Settings();

        public bool CanExecute(object parameter)
        {
            if (mainWindow.Visibility == Visibility.Hidden)
            {
                return true;
            }
            return false;
        }

        public void Execute(object parameter)
        {
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            settings = settings.Read();
            settings.WindowVisibility = Visibility.Visible;
            settings.Save();
        }

        public event EventHandler CanExecuteChanged;
    }
}

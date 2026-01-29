using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Taiko.pages;

namespace Taiko
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var uri = new Uri("pack://application:,,,/asset/title_screen.png", UriKind.Absolute);
            TitleImage.Source = new BitmapImage(uri);
            MainFrame.Navigated += MainFrame_Navigated;
        }
        

        private void MainFrame_Navigated(object? sender, NavigationEventArgs e)
        {
            // If Content is null, we are on the Title Screen -> Show the text
            // Otherwise, we are on a Page -> Hide the text
            if (e.Content == null)
            {
                ActionText.Visibility = Visibility.Visible;
            }
            else
            {
                ActionText.Visibility = Visibility.Collapsed;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus the window to ensure it receives key events
            this.Focus();
            Keyboard.Focus(this);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Only navigate on keypress when still on the title screen (ActionText visible)
            if (ActionText.Visibility == Visibility.Visible)
            {
                NavigateToControl();
                e.Handled = true;
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Only navigate on first click when still on the title screen (ActionText visible)
            if (ActionText.Visibility == Visibility.Visible)
            {
                NavigateToControl();
                e.Handled = true;
            }
        }

        private void NavigateToControl()
        {
            // Use the named Frame instance to navigate. Relative URI to the page.
            MainFrame.Navigate(new Controlpage());
        }
    }
}
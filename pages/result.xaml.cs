using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Taiko.pages
{
    /// <summary>
    /// Interaction logic for result.xaml
    /// </summary>
    public partial class result : Page
    {
        public bool GamePlayState { get; set; }= true;
        public result(string difficulty)
        {
            Debug.WriteLine("Result page initialized with difficulty: " + GamePlay.DiffDict[difficulty]);

            InitializeComponent();
            Loaded += Page_Loaded;
            Storyboard hop = (Storyboard)this.FindResource("CharacterHop");
            if (GamePlay.totalscore > 100)
            {
                Lose_lil_man.Visibility = Visibility.Collapsed;
                Win_lil_man.Visibility = Visibility.Visible;
                hop.Begin(Win_lil_man);
            }
            else {
                Win_lil_man.Visibility = Visibility.Collapsed;
                Lose_lil_man.Visibility = Visibility.Visible;
            }

            if (difficulty == "Ez") { 
                Difficulty.Source = new BitmapImage(new Uri("pack://application:,,,/asset/ez_result.png", UriKind.Absolute));
            }
            else if (difficulty == "Nm")
            {
                Difficulty.Source = new BitmapImage(new Uri("pack://application:,,,/asset/nm_result.png", UriKind.Absolute));
            }
            else if (difficulty == "Hd")
            {
                Difficulty.Source = new BitmapImage(new Uri("pack://application:,,,/asset/hd_result.png", UriKind.Absolute));
            }
            else if (difficulty == "Ex")
            {
                Difficulty.Source = new BitmapImage(new Uri("pack://application:,,,/asset/ex_result.png", UriKind.Absolute));
            }

            if (Controlpage.isMultiplayer == false)
            {
                ScoreText.Text += GamePlay.totalscore.ToString();
                PerfectText.Text += GamePlay.perfect.ToString();
                GoodText.Text += GamePlay.good.ToString();
                BadText.Text += GamePlay.miss.ToString();
            }
            else { 
                SinglePlayer.Visibility = Visibility.Collapsed;
                MultiPlayer.Visibility = Visibility.Visible;
                P1ScoreText.Text += GamePlay.totalscore.ToString();
                P2ScoreText.Text += GamePlay.totalscore2.ToString();
            }
                this.Focusable = true;
            Loaded += (s, e) =>
            {
                this.Focus();
                Keyboard.Focus(this);
            };
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TriggerBackgroundFall();
        }

        private void TriggerBackgroundFall()
        {
            Storyboard sb = (Storyboard)this.FindResource("BgScroll");
            sb.Begin();
        }


        private void NavToMain(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as Taiko.MainWindow;

            if (mainWindow != null)
            {
                // Setting this to null now automatically triggers the 
                // MainFrame_Navigated logic to show the ActionText
                mainWindow.MainFrame.Content = null;
                Controlpage.isMultiplayer = false;
                while (mainWindow.MainFrame.CanGoBack)
                {
                    mainWindow.MainFrame.RemoveBackEntry();
                }

                mainWindow.Focus();
            }
            e.Handled = true;
        }
    }
}

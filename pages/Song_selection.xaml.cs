using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    public partial class Song_selection : Page
    {
        private Storyboard? _bgStoryboard;
        private List<Button> DifficultyButtons => new List<Button> {EzBut, NmBut, HdBut, ExBut};

        public Song_selection()
        {
            InitializeComponent();
            Loaded += Song_selection_Loaded;
            Unloaded += Page_Unloaded;
            IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void Song_selection_Loaded(object? sender, RoutedEventArgs e)
        {
            _bgStoryboard = (Storyboard?)Resources["BgScroll"];
            if (_bgStoryboard != null)
            {
                // Begin with controllable = true so we can Pause/Resume/Stop later
                Debug.WriteLine("Storyboard beginning");
                _bgStoryboard.Begin(this, true);
            }

            /*var verticalText = WithLineBreaks("Dream");
            var tb = new TextBlock
            {
                Text = verticalText,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.NoWrap
            };
            SongButton.Content = tb;*/
        }

        private void Page_Unloaded(object? sender, RoutedEventArgs e)
        {
            if (_bgStoryboard != null)
            {
                Debug.WriteLine("Page Unloaded, Storyboard stopped");
                _bgStoryboard.Stop(this);
            }
        }

        private void Page_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (_bgStoryboard == null) {
                Debug.WriteLine("Page isvisible change, storyboard null");
                return; 
            }

            if (IsVisible)
            {
                Debug.WriteLine("Resume");
                _bgStoryboard.Resume(this);
            }
            else
            {
                Debug.WriteLine("Pause");
                _bgStoryboard.Pause(this);
            }
        }


        private void NavToGamePlay(Button btn, RoutedEventArgs e)
        {
            string difficulty = btn.Name[0..2];
            Debug.WriteLine("Difficulty: "+ difficulty);
            Debug.WriteLine("RoutedEventArgs: " + e);
            var nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new GamePlay(difficulty));
            e.Handled = true;
        }
        public bool IsDifficultyPanelActivated { get; set; } = false;
        public bool IsDifficultyButtonActivated { get; set; } = false;
        private void expand(object sender, RoutedEventArgs e) {
            var image = (Image)SongButton.Template.FindName("ButtonImage", SongButton);
            Debug.WriteLine(image.Source);
            if (SongButton.Width==60){
                Select_song.Visibility = Visibility.Collapsed;
                DiffGrid.Visibility = Visibility.Visible;
                Selection.Source = new BitmapImage(new Uri(@"/asset/select_diff.png", UriKind.Relative));
                IsDifficultyButtonActivated = true;
                foreach (var btn in DifficultyButtons)
                {
                    btn.IsEnabled = true;
                }
                Debug.WriteLine("Expanded Song Button");
            }
            e.Handled = true;
        }


        private void DifficultyButton_Click(object sender, RoutedEventArgs e) {
            NavToGamePlay(sender as Button, e);
        }
    }
}
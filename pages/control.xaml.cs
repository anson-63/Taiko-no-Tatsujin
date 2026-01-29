using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Taiko.pages
{
    public partial class Controlpage : Page
    {
        public static Key RedLeft { get; set; } = Key.A;
        public static Key RedRight { get; set; } = Key.S;
        public static Key BlueLeft { get; set; } = Key.D;
        public static Key BlueRight { get; set; } = Key.F;
        public static int scaler { get; set; } = 1;

        public static bool isMultiplayer { get; set; } = false;
        public static Key P2RedLeft { get; set; } = Key.J;
        public static Key P2RedRight { get; set; } = Key.K;
        public static Key P2BlueLeft { get; set; } = Key.H;
        public static Key P2BlueRight { get; set; } = Key.L;
        public Controlpage()
        {
            InitializeComponent();
            Loaded += Control_Loaded;
        }


        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var packUri = new Uri("pack://application:,,,/asset/settings.png", UriKind.Absolute);
                // Check embedded resource stream
                var streamInfo = Application.GetResourceStream(packUri);
                Debug.WriteLine(streamInfo == null ? "Resource stream: NOT FOUND" : "Resource stream: FOUND");

                // Try to set the image (this will throw if resource isn't found)
                settingImage.Source = new BitmapImage(packUri);
                Debug.WriteLine("settingImage.Source set successfully.");
                OkButton.Click += OK;
                Debug.WriteLine("OkButton handler attached programmatically");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading image: " + ex);
            }
        }

        private void OK(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Controlpage Onclick triggered");
            NavigateToSongSelection();
            e.Handled = true;
        }

        private void NavigateToSongSelection()
        {
            Debug.WriteLine("Navigating via NavigationService");
            var nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Song_selection());
            return;
        }
        private void Multiplier(object sender, SelectionChangedEventArgs e)
        {
            if (MultiplierCombo?.SelectedItem is ComboBoxItem item)
            {
                string val = item.Content.ToString(); // e.g., "2x"

                // Remove the 'x' and convert to a double
                // FIX: Updated to assign to the static property directly
                if (int.TryParse(val.Replace("x", ""), out int result))
                {
                    scaler = result;
                }
            }
            Debug.WriteLine($"Scaler set to: {scaler}");
        }
        private void MultiplayerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Player2Setting.Visibility = Visibility.Visible;
            isMultiplayer = true;
            e.Handled = true;
        }

        private void MultiplayerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Player2Setting.Visibility = Visibility.Hidden;
            isMultiplayer = false;
            e.Handled = true;
        }

        private void key_config(object sender, RoutedEventArgs e)
        {
            var btn = sender as RadioButton; // Or ToggleButton
            if (btn == null) return;

            // 1. Ensure the button can receive keyboard input
            btn.Focus();

            // 2. Find the TextBlock inside the button's content to give feedback
            // Your structure is: RadioButton -> Grid -> [Image, TextBlock]
            var grid = btn.Content as Grid;
            var textBlock = grid?.Children.OfType<TextBlock>().FirstOrDefault();

            if (textBlock == null) return;

            string originalKey = textBlock.Text;
            textBlock.Text = "..."; // Visual cue that we are waiting for a key

            // 3. Create a temporary event handler to capture the NEXT key press
            KeyEventHandler handler = null;
            handler = (s, args) =>
            {
                // Capture the key
                Key pressedKey = args.Key;

                // Handle the "Alt" key (which WPF labels as 'System')
                if (pressedKey == Key.System) pressedKey = args.SystemKey;

                // 4. Update the UI
                textBlock.Text = pressedKey.ToString();

                //5. Update global
                if (btn.Name == "Red_Left") RedLeft = pressedKey;
                else if (btn.Name == "Red_Right") RedRight = pressedKey;
                else if (btn.Name == "Blue_Left") BlueLeft = pressedKey;
                else if (btn.Name == "Blue_Right") BlueRight = pressedKey;

                // 6. Cleanup: stop listening and uncheck the button
                btn.PreviewKeyDown -= handler;
                btn.IsChecked = false;
                args.Handled = true;

                Debug.WriteLine($"Key for {btn.Name} set to: {pressedKey}");
                Debug.WriteLine($"Current Config - RedLeft: {RedLeft}, RedRight: {RedRight}, BlueLeft: {BlueLeft}, BlueRight: {BlueRight}");
            };

            // Attach the listener
            // We use PreviewKeyDown to catch keys like Tab or Arrows before the system uses them
            btn.PreviewKeyDown += handler;
        }
    }
}
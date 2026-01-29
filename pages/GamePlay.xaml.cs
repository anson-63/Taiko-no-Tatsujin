using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Taiko.pages
{
    public partial class GamePlay : Page
    {
        public static Dictionary<string, string> DiffDict = new Dictionary<string, string>()
        {
            {"Ez", "Easy"}, {"Nm", "Normal"}, {"Hd", "Hard"}, {"Ex", "Extreme"}
        };

        // P1 public stats (existing)
        public static int combo = 0, maxcombo = 0, miss = 0, perfect = 0, good = 0, totalscore = 0;

        // P2 public stats (added)
        public static int combo2 = 0, maxcombo2 = 0, miss2 = 0, perfect2 = 0, good2 = 0, totalscore2 = 0;

        private static readonly Dictionary<string, string> DiffToCourse = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Ez", "easy" }, { "Nm", "normal" }, { "Hd", "hard" }, { "Ex", "oni" }
        };

        private readonly string difficultyKey, parserCourse;
        private string difficultystring = "";
        private bool isGameEnd = false;

        private MediaPlayer bgmPlayer = new MediaPlayer();
        private MediaPlayer donSfx = new MediaPlayer(), katSfx = new MediaPlayer();
        private MediaPlayer donSfxP2 = new MediaPlayer(), katSfxP2 = new MediaPlayer();

        private TjaParser tjaParser = new TjaParser();
        private List<TjaNote> notes = new List<TjaNote>();

        // Active visuals per player
        private List<FrameworkElement> activeNotesP1 = new List<FrameworkElement>();
        private List<FrameworkElement> activeNotesP2 = new List<FrameworkElement>();

        private int nextNoteIndex = 0;
        private Stopwatch gameClock = new Stopwatch();
        private double clockOffset;

        private double NoteSpeed; // Initialized in StartGame
        private const double JudgmentX = 200;
        private const double PerfectWindow = 0.05;
        private const double MissWindow = 0.18;

        public GamePlay(string difficulty)
        {
            difficultyKey = difficulty ?? "Ex";
            parserCourse = DiffToCourse.ContainsKey(difficultyKey) ? DiffToCourse[difficultyKey] : "oni";
            difficultystring = difficulty;
            InitializeComponent();

            this.Focusable = true;
            this.Loaded += (s, e) => {
                this.Focus();
                var sb = this.Resources["BgScroll"] as Storyboard;
                sb?.Begin();
                Debug.WriteLine("GamePlay loaded with P2?: " + Controlpage.isMultiplayer);
                // Show/hide P2 lane
                P2.Visibility = Controlpage.isMultiplayer ? Visibility.Visible : Visibility.Collapsed;

                StartGame();
            };
            this.KeyDown += GamePlay_KeyDown;
            this.KeyUp += GamePlay_KeyUp;
        }

        private void StartGame()
        {
            P2.Visibility = Controlpage.isMultiplayer ? Visibility.Visible : Visibility.Collapsed;
            // Update Speed based on latest scaler
            NoteSpeed = 450 * Controlpage.scaler;

            // Reset P1
            combo = 0; maxcombo = 0; perfect = 0; good = 0; miss = 0; totalscore = 0;

            // Reset P2
            combo2 = 0; maxcombo2 = 0; perfect2 = 0; good2 = 0; miss2 = 0; totalscore2 = 0;

            nextNoteIndex = 0; activeNotesP1.Clear(); activeNotesP2.Clear();

            // Clear canvases
            ClearCanvasImages(NoteCanvas);
            ClearCanvasImages(NoteCanvasP2);

            var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            bgmPlayer.Open(new Uri(Path.Combine(baseDir, "asset", "dream.mp3")));
            donSfx.Open(new Uri(Path.Combine(baseDir, "asset", "don.wav")));
            katSfx.Open(new Uri(Path.Combine(baseDir, "asset", "kat.wav")));
            // P2 SFX
            donSfxP2.Open(new Uri(Path.Combine(baseDir, "asset", "don.wav")));
            katSfxP2.Open(new Uri(Path.Combine(baseDir, "asset", "kat.wav")));

            tjaParser.Parse(Path.Combine(baseDir, "asset", "dream.tja"), parserCourse);
            notes.Clear();
            notes.AddRange(tjaParser.Notes);
            clockOffset = tjaParser.Offset;

            bgmPlayer.MediaOpened += (s, e) => {
                gameClock.Restart();
                bgmPlayer.Play();
                CompositionTarget.Rendering += OnRendering;
            };
        }

        private void ClearCanvasImages(Canvas canvas)
        {
            if (canvas == null) return;
            var toRemove = new List<UIElement>();
            foreach (UIElement child in canvas.Children) { if (child is Image) toRemove.Add(child); }
            foreach (var item in toRemove) canvas.Children.Remove(item);
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            if (!gameClock.IsRunning || isGameEnd) return;

            double currentTime = gameClock.Elapsed.TotalSeconds + clockOffset;
            // Use each canvas width independently (fallback to 1280 if zero)
            double canvasWidthP1 = NoteCanvas.ActualWidth > 0 ? NoteCanvas.ActualWidth : 1280;
            double canvasWidthP2 = NoteCanvasP2.ActualWidth > 0 ? NoteCanvasP2.ActualWidth : 1280;
            double travelTimeP1 = (canvasWidthP1 - JudgmentX) / NoteSpeed;
            double travelTimeP2 = (canvasWidthP2 - JudgmentX) / NoteSpeed;

            // 1. Spawn notes for both players (use same index/notes so they are synchronized)
            while (nextNoteIndex < notes.Count)
            {
                var note = notes[nextNoteIndex];
                // Spawn when either lane needs it (usually same)
                if (note.Time > currentTime + Math.Min(travelTimeP1, travelTimeP2)) break;

                // P1 note
                var visualP1 = CreateNoteVisual(note.Type);
                visualP1.Tag = note;
                NoteCanvas.Children.Add(visualP1);
                activeNotesP1.Add(visualP1);

                // P2 note (only if lane is visible)
                if (P2.Visibility == Visibility.Visible)
                {
                    var visualP2 = CreateNoteVisual(note.Type);
                    visualP2.Tag = note;
                    NoteCanvasP2.Children.Add(visualP2);
                    activeNotesP2.Add(visualP2);
                    Grid.SetRowSpan(P1, 1);

                }

                nextNoteIndex++;
            }

            // 2. Move and Clean up notes
            UpdateAndCullLane(activeNotesP1, NoteCanvas, currentTime, isP2: false);
            if (P2.Visibility == Visibility.Visible)
                UpdateAndCullLane(activeNotesP2, NoteCanvasP2, currentTime, isP2: true);

            // 3. End condition
            if (nextNoteIndex >= notes.Count && activeNotesP1.Count == 0 && (P2.Visibility != Visibility.Visible || activeNotesP2.Count == 0))
            {
                double lastNoteTime = notes.Count > 0 ? notes[notes.Count - 1].Time : 0;
                if (currentTime > lastNoteTime + 3.0)
                {
                    NavToResult();
                }
            }
        }

        private void UpdateAndCullLane(List<FrameworkElement> activeNotes, Canvas canvas, double currentTime, bool isP2)
        {
            for (int i = activeNotes.Count - 1; i >= 0; i--)
            {
                var elem = activeNotes[i];
                var data = (TjaNote)elem.Tag;
                double timeDiff = data.Time - currentTime;

                Canvas.SetLeft(elem, JudgmentX + (timeDiff * NoteSpeed) - (elem.Width / 2));
                Canvas.SetTop(elem, 120 - (elem.Height / 2));

                if (timeDiff < -MissWindow)
                {
                    if (!isP2) HandleMissP1();
                    else HandleMissP2();
                    RemoveNote(elem, activeNotes, canvas);
                }
            }
        }

        private async void GamePlay_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return;

            // P1 key indicators
            if (e.Key == Controlpage.RedLeft) red_left.Visibility = Visibility.Visible;
            if (e.Key == Controlpage.RedRight) red_right.Visibility = Visibility.Visible;
            if (e.Key == Controlpage.BlueLeft) blue_left.Visibility = Visibility.Visible;
            if (e.Key == Controlpage.BlueRight) blue_right.Visibility = Visibility.Visible;

            // P2 key indicators
            if (P2.Visibility == Visibility.Visible)
            {
                if (e.Key == Controlpage.P2RedLeft) red_leftP2.Visibility = Visibility.Visible;
                if (e.Key == Controlpage.P2RedRight) red_rightP2.Visibility = Visibility.Visible;
                if (e.Key == Controlpage.P2BlueLeft) blue_leftP2.Visibility = Visibility.Visible;
                if (e.Key == Controlpage.P2BlueRight) blue_rightP2.Visibility = Visibility.Visible;
            }

            // Determine which player this keypress belongs to (can be both if keys overlap, but your defaults do not)
            bool isP1Don = (e.Key == Controlpage.RedLeft || e.Key == Controlpage.RedRight);
            bool isP1Kat = (e.Key == Controlpage.BlueLeft || e.Key == Controlpage.BlueRight);
            bool isP1 = isP1Don || isP1Kat;

            bool isP2Don = (e.Key == Controlpage.P2RedLeft || e.Key == Controlpage.P2RedRight);
            bool isP2Kat = (e.Key == Controlpage.P2BlueLeft || e.Key == Controlpage.P2BlueRight);
            bool isP2 = (P2.Visibility == Visibility.Visible) && (isP2Don || isP2Kat);

            double currentTime = gameClock.Elapsed.TotalSeconds + clockOffset;

            // Handle P1 input
            if (isP1)
            {
                await TryHitLaneAsync(activeNotesP1, isDon: isP1Don, currentTime: currentTime, isP2: false);
            }

            // Handle P2 input
            if (isP2)
            {
                await TryHitLaneAsync(activeNotesP2, isDon: isP2Don, currentTime: currentTime, isP2: true);
            }
        }

        private async Task TryHitLaneAsync(List<FrameworkElement> activeNotes, bool isDon, double currentTime, bool isP2)
        {
            for (int i = 0; i < activeNotes.Count; i++)
            {
                var elem = activeNotes[i];
                if (elem.Opacity == 0) continue; // Skip notes already being processed

                var note = (TjaNote)elem.Tag;
                double diff = Math.Abs(note.Time - currentTime);

                if (diff <= MissWindow)
                {
                    // Match color type
                    bool isKat = !isDon;
                    bool match = (isDon && (note.Type == 1 || note.Type == 3)) || (isKat && (note.Type == 2 || note.Type == 4));
                    if (!match) continue;

                    bool hitBig = false;

                    // For big notes (type 3/4) require both sides pressed
                    if (note.Type == 3 || note.Type == 4)
                    {
                        await Task.Delay(30);

                        if (!isP2)
                        {
                            if (note.Type == 3)
                                hitBig = Keyboard.IsKeyDown(Controlpage.RedLeft) && Keyboard.IsKeyDown(Controlpage.RedRight);
                            else
                                hitBig = Keyboard.IsKeyDown(Controlpage.BlueLeft) && Keyboard.IsKeyDown(Controlpage.BlueRight);
                        }
                        else
                        {
                            if (note.Type == 3)
                                hitBig = Keyboard.IsKeyDown(Controlpage.P2RedLeft) && Keyboard.IsKeyDown(Controlpage.P2RedRight);
                            else
                                hitBig = Keyboard.IsKeyDown(Controlpage.P2BlueLeft) && Keyboard.IsKeyDown(Controlpage.P2BlueRight);
                        }

                        if (!hitBig) return; // require both keys simultaneously
                    }

                    // Play SFX and register hit
                    PlaySfx(isDon, isP2);
                    elem.Opacity = 0; // Hide immediately so it looks like a hit

                    string rating = diff <= PerfectWindow ? "良" : "可";

                    if (!isP2) HandleHitP1(rating, hitBig);
                    else HandleHitP2(rating, hitBig);

                    // Remove from this lane only
                    RemoveNote(elem, activeNotes, isP2 ? NoteCanvasP2 : NoteCanvas);
                    break;
                }
            }
        }

        private void GamePlay_KeyUp(object sender, KeyEventArgs e)
        {
            // P1 indicators
            if (e.Key == Controlpage.RedLeft) red_left.Visibility = Visibility.Hidden;
            if (e.Key == Controlpage.RedRight) red_right.Visibility = Visibility.Hidden;
            if (e.Key == Controlpage.BlueLeft) blue_left.Visibility = Visibility.Hidden;
            if (e.Key == Controlpage.BlueRight) blue_right.Visibility = Visibility.Hidden;

            // P2 indicators
            if (P2.Visibility == Visibility.Visible)
            {
                if (e.Key == Controlpage.P2RedLeft) red_leftP2.Visibility = Visibility.Hidden;
                if (e.Key == Controlpage.P2RedRight) red_rightP2.Visibility = Visibility.Hidden;
                if (e.Key == Controlpage.P2BlueLeft) blue_leftP2.Visibility = Visibility.Hidden;
                if (e.Key == Controlpage.P2BlueRight) blue_rightP2.Visibility = Visibility.Hidden;
            }
        }

        // P1 scoring/visuals
        private void HandleHitP1(string rating, bool hitBig)
        {
            combo++;
            if (combo > maxcombo) maxcombo = combo;
            ComboCountText.Visibility = Visibility.Visible;
            ComboCountText.Text = combo.ToString();

            int mult = hitBig ? 2 : 1;
            if (rating.StartsWith("良")) { perfect++; totalscore += (int)(100 * Controlpage.scaler * mult); }
            else { good++; totalscore += (int)(50 * Controlpage.scaler * mult); }

            var anim = new DoubleAnimation(1.3, 1.0, TimeSpan.FromMilliseconds(50));
            ComboScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            ComboScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            ShowRatingText(rating, NoteCanvas);
        }

        private void HandleMissP1()
        {
            combo = 0; ComboCountText.Visibility = Visibility.Collapsed;
            miss++;
            ShowRatingText("不可", NoteCanvas);
            totalscore -= (int)(50 / Controlpage.scaler);
        }

        // P2 scoring/visuals
        private void HandleHitP2(string rating, bool hitBig)
        {
            combo2++;
            if (combo2 > maxcombo2) maxcombo2 = combo2;
            ComboCountTextP2.Visibility = Visibility.Visible;
            ComboCountTextP2.Text = combo2.ToString();

            int mult = hitBig ? 2 : 1;
            if (rating.StartsWith("良")) { perfect2++; totalscore2 += (int)(100 * Controlpage.scaler * mult); }
            else { good2++; totalscore2 += (int)(50 * Controlpage.scaler * mult); }

            var anim = new DoubleAnimation(1.3, 1.0, TimeSpan.FromMilliseconds(50));
            ComboScaleP2.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            ComboScaleP2.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            ShowRatingText(rating, NoteCanvasP2);
        }

        private void HandleMissP2()
        {
            combo2 = 0; ComboCountTextP2.Visibility = Visibility.Collapsed;
            miss2++;
            ShowRatingText("不可", NoteCanvasP2);
            totalscore2 -= (int)(50 / Controlpage.scaler);
        }

        private void ShowRatingText(string text, Canvas canvas)
        {
            TextBlock tb = new TextBlock { Text = text, FontSize = 42, Foreground = Brushes.White, FontWeight = FontWeights.Bold };
            Canvas.SetLeft(tb, JudgmentX - 25); Canvas.SetTop(tb, 40);
            canvas.Children.Add(tb);
            var move = new DoubleAnimation(40, 0, TimeSpan.FromMilliseconds(200));
            var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            fade.Completed += (s, e) => canvas.Children.Remove(tb);
            tb.BeginAnimation(Canvas.TopProperty, move);
            tb.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        private FrameworkElement CreateNoteVisual(int type)
        {
            string img = type switch { 1 => "don_left.png", 3 => "don_right.png", 2 => "ka_left.png", 4 => "ka_right.png", _ => "don_left.png" };
            double size = (type >= 3) ? 105 : 68;
            Image noteImage = new Image { Width = size, Height = size, Source = new BitmapImage(new Uri("pack://application:,,,/asset/" + img)) };
            Panel.SetZIndex(noteImage, 2);
            return noteImage;
        }
        private void RemoveNote(FrameworkElement elem, List<FrameworkElement> activeList, Canvas canvas)
        {
            if (canvas.Children.Contains(elem)) canvas.Children.Remove(elem);
            activeList.Remove(elem);
        }

        private void PlaySfx(bool isDon, bool isP2)
        {
            var sfx = isP2 ? (isDon ? donSfxP2 : katSfxP2) : (isDon ? donSfx : katSfx);
            sfx.Stop(); sfx.Position = TimeSpan.Zero; sfx.Play();
        }

        private async void NavToResult()
        {
            if (isGameEnd) return;
            isGameEnd = true;

            CompositionTarget.Rendering -= OnRendering;

            await Task.Delay(1000);

            gameClock.Stop();
            bgmPlayer.Stop();

            this.NavigationService?.Navigate(new result(difficultystring));
        }
    }
}
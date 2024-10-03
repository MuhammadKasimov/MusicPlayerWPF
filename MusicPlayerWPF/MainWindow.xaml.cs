using System.IO;
using System;
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
using System.Windows.Threading;
using System.Linq;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace MusicPlayerWPF
{

    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private bool isPlaying = false;
        private bool isDraging = false;
        FileInfo lastFile;
        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            LoadLastMusic();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isDraging)
            {
                MusicMinutSlider.Value = MusicMediaEl.Position.TotalSeconds;

                if (MusicMediaEl.NaturalDuration.HasTimeSpan)
                {
                    MinuteSpend.Text = $"{MusicMediaEl.Position.Minutes}:{MusicMediaEl.Position.Seconds}";
                }
            }
        }

        private void LoadLastMusic()
        {
            var files = new DirectoryInfo("Musics").GetFiles();

            lastFile = files.OrderBy(f => f.Name).LastOrDefault();

            if (lastFile != null)
            {
                MusicMediaEl.Source = new Uri(lastFile.FullName, UriKind.RelativeOrAbsolute);
                MusicMediaEl.Play();
                MusicMediaEl.Pause();
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                MusicMediaEl.Pause();
                PlayOrPauseIcon.Kind = PackIconKind.Play;
                isPlaying = false;
            }
            else
            {
                MusicMediaEl.Play();
                PlayOrPauseIcon.Kind = PackIconKind.Pause;
                isPlaying = true;
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            var files = new DirectoryInfo("Musics").GetFiles();

            var leftFiles = files.OrderBy(f => f.Name).TakeWhile(m => m.Name != lastFile.Name).ToArray();

            if (leftFiles.Count() > 0)
            {
                lastFile = leftFiles.LastOrDefault();
            }
            else
            {
                lastFile = files.LastOrDefault();
            }
            if (lastFile != null)
            {
                MusicMediaEl.Source = new Uri(lastFile.FullName, UriKind.RelativeOrAbsolute);
                MusicMediaEl.Play();
                MusicMediaEl.Pause();
            }
            PlayOrPauseIcon.Kind = PackIconKind.Play;
            isPlaying = false;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var files = new DirectoryInfo("Musics").GetFiles().OrderBy(f => f.Name).ToArray();

            var leftFiles = files.SkipWhile(m => m.Name != lastFile.Name).ToArray();

            if (leftFiles.Count() > 1)
            {
                lastFile = leftFiles[1];
            }
            else
            {
                lastFile = files[0];
            }
            if (lastFile != null)
            {
                MusicMediaEl.Source = new Uri(lastFile.FullName, UriKind.RelativeOrAbsolute);
                MusicMediaEl.Play();
                MusicMediaEl.Pause();
            }
            PlayOrPauseIcon.Kind = PackIconKind.Play;
            isPlaying = false;
        }

        private void MusicMediaEl_MediaEnded(object sender, RoutedEventArgs e)
        {
            MusicMediaEl.Stop();
            PlayOrPauseIcon.Kind = PackIconKind.Play;
            isPlaying = false;
            NextButton_Click(sender, e);
            PlayButton_Click(sender,e);
        }

        private void MusicMediaEl_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (MusicMediaEl.NaturalDuration.HasTimeSpan)
                MusicMinutSlider.Maximum = MusicMediaEl.NaturalDuration.TimeSpan.TotalSeconds;
            _timer.Start();
            if (MusicMediaEl.NaturalDuration.HasTimeSpan)
                MinuteLeft.Text = $"{MusicMediaEl.NaturalDuration.TimeSpan.Minutes}:{MusicMediaEl.NaturalDuration.TimeSpan.Seconds}";
            GetData();
        }

        private void MusicMinutSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDraging = true;
        }

        private void MusicMinutSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDraging = false;
            MusicMediaEl.Position = new TimeSpan(0, 0, 0, (int)MusicMinutSlider.Value);
        }

        private void GetData()
        {
            try
            {
                // Load the file
                var file = TagLib.File.Create(lastFile.FullName);

                // Get song details
                string songTitle = file.Tag.Title;
                string artist = file.Tag.FirstPerformer;

                // For images, you'll typically find album art
                // Here's an example of retrieving the first picture (album art)
                var pictures = file.Tag.Pictures;
                byte[] imageData = null;
                if (pictures.Length >= 1)
                {
                    imageData = pictures[0].Data.Data;
                }

                // Displaying the information
                SongNameTxt.Text = songTitle;
                SingerNameTxt.Text = artist;

                if (imageData != null)
                {
                    // Saving the image to a file (optional)
                    MemoryStream memory = new MemoryStream(imageData);
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = memory;
                    image.EndInit();

                    MusicBackgroundImage.Source = image;
                    MusicMainImage.ImageSource = image;
                }
                else
                {
                    MemoryStream memory = new MemoryStream(File.ReadAllBytes("default.png"));
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = memory;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    MusicMainImage.ImageSource = bitmap;
                    MusicBackgroundImage.Source = bitmap;
                }
                var color = GetColor();
                MainBorder.Background = new SolidColorBrush(color);
                IsColorBright(color);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private void IsColorBright(Color c)
        {
            // Calculate perceived brightness based on RGB components
            int brightness = (int)(c.R * 0.30 + c.G * 0.59 + c.B * 0.11);
            if (brightness < 127)
            {
                SongNameTxt.Foreground = new SolidColorBrush(Colors.White);
                SingerNameTxt.Foreground = new SolidColorBrush(Colors.White);
                MinuteLeft.Foreground = new SolidColorBrush(Colors.White);
                MinuteSpend.Foreground = new SolidColorBrush(Colors.White);
                SkipNext.Foreground = new SolidColorBrush(Color.FromRgb(203, 185, 187));
                SkipPrevious.Foreground = new SolidColorBrush(Color.FromRgb(203, 185, 187));

            }
            else 
            {
                SongNameTxt.Foreground = new SolidColorBrush(Colors.Black);
                SingerNameTxt.Foreground = new SolidColorBrush(Colors.Black);
                MinuteLeft.Foreground = new SolidColorBrush(Colors.Black);
                MinuteSpend.Foreground = new SolidColorBrush(Colors.Black);
                SkipNext.Foreground = new SolidColorBrush(Color.FromRgb(41, 41, 41));
                SkipPrevious.Foreground = new SolidColorBrush(Color.FromRgb(41, 41, 41));
            }
        }
        private Color GetColor()
        {
            // Convert the position to integer values
            int x = (int)MusicMainImage.AlignmentX;
            int y = (int)MusicMainImage.AlignmentY;

            // Get the image source from the Image control
            BitmapSource bitmapSource = (BitmapSource)MusicMainImage.ImageSource;

            // Check if the mouse click is within the bounds of the image
            if (x >= 0 && x < bitmapSource.PixelWidth && y >= 0 && y < bitmapSource.PixelHeight)
            {
                // Define the size of the pixels array
                int bytesPerPixel = (bitmapSource.Format.BitsPerPixel + 7) / 8;
                int stride = bytesPerPixel * bitmapSource.PixelWidth;
                byte[] pixels = new byte[bitmapSource.PixelHeight * stride];

                // Copy the image data to the pixels array
                bitmapSource.CopyPixels(pixels, stride, 0);

                // Calculate the index of the clicked pixel
                int index = y * stride + x * bytesPerPixel;

                // Get the color of the clicked pixel
                Color color = Color.FromRgb(
                    pixels[index + 2], // Red
                    pixels[index + 1], // Green
                    pixels[index]);    // Blue

                return color;
            }
            return Color.FromRgb(0,0,0);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
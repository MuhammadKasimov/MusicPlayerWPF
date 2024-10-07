using MaterialDesignThemes.Wpf;
using MusicPlayerWPF.Boxes;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MusicPlayerWPF
{

    public partial class MainWindow : Window
    {
        #region Fields
        private DispatcherTimer _timer;
        private bool isPlaying = false;

        private bool isDraging = false;
        FileInfo lastFile;
        private string directory = "Musics";
        private string[] fileFormats = [".mp3", ".flac", ".mp4", ".wav", ".wma", ".aac"];
        private string configFile = string.Empty;
        #endregion
        
        #region Ctor
        public MainWindow()
        {
            InitializeComponent();
            configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Roaming/MusicPlayerSettings/lastfolder.txt");
        }
        #endregion
        
        #region Slider Events
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isDraging)
            {
                if (isPlaying)
                    MusicMediaEl.Play();
                if (MusicMediaEl.NaturalDuration.HasTimeSpan)
                    MusicMinutSlider.Value = MusicMediaEl.Position.TotalSeconds;
                    MinuteSpend.Text = $"{MusicMediaEl.Position.Minutes}:{MusicMediaEl.Position.Seconds}";
            }
            else
                 MusicMediaEl.Pause();
        }

        private void MusicMinutSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDraging = true;
        }

        private void MusicMinutSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDraging = false;
        }

        private void MusicMinutSlider_DragEnter(object sender, DragEventArgs e)
        {
            isDraging = true;
        }

        private void MusicMinutSlider_DragLeave(object sender, DragEventArgs e)
        {
            isDraging = false;
        }
        private void MusicMinutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MusicMediaEl.Position = TimeSpan.FromSeconds(MusicMinutSlider.Value);
        }
        #endregion

        #region Load Data

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;

            var folder = configFile.Replace("lastfolder.txt", string.Empty);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                File.WriteAllText(configFile, "Musics");
            }
            directory = File.ReadAllText(configFile);
            LoadLastMusic();
        }
        private void GetData()
        {
            try
            {
                LoadMusicInfo();
                SetComponentsColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void LoadLastMusic()
        {
            if (Directory.Exists(directory))
            {
                var files = new DirectoryInfo(directory).GetFiles().Where(f => fileFormats.Contains(f.Extension.ToLower()));

                lastFile = files.OrderBy(f => f.Name).LastOrDefault();

                if (lastFile != null)
                {
                    LoadMusicInfo();
                }
                else
                {
                    PlayButton.IsEnabled = false;
                    NextButton.IsEnabled = false;
                    PreviousButton.IsEnabled = false;
                    SongNameTxt.Text = string.Empty;
                    SingerNameTxt.Text = string.Empty;

                    LoadDefaultImage();
                    MainFrame.Content = new ErrorMessageBox("No music found in this folder", 3);
                }
            }
            else
            {
                LoadDefaultImage();
                MainFrame.Content = new ErrorMessageBox("No music found in this folder", 3);
            }
            SetComponentsColor();
        }
        private void LoadMusicInfo()
        {
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
                LoadImage(imageData);
            }
            else
            {
                LoadDefaultImage();
            }
            MusicMediaEl.Source = new Uri(lastFile.FullName, UriKind.RelativeOrAbsolute);
            MusicMediaEl.Play();
            MusicMediaEl.Pause();
            PlayButton.IsEnabled = true;
            NextButton.IsEnabled = true;
            PreviousButton.IsEnabled = true;
        }
        private void LoadDefaultImage()
        {
            var info = Assembly.GetExecutingAssembly().GetName();
            var name = info.Name;
            using var memory = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream($"{name}.default.png")!;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = memory;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            MusicBackgroundImage.Source = bitmap;
            MusicMainImage.ImageSource = bitmap;
        }
        private void LoadImage(byte[] imageData)
        {
            MemoryStream memory = new MemoryStream(imageData);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = memory;
            image.EndInit();

            MusicBackgroundImage.Source = image;
            MusicMainImage.ImageSource = image;
        }
        #endregion

        #region Action Handlers

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select a folder";
                dialog.ShowNewFolderButton = true;

                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string folderPath = dialog.SelectedPath;
                    directory = folderPath;
                    File.WriteAllText(configFile, folderPath);
                    LoadLastMusic();
                }
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
            var files = new DirectoryInfo(directory).GetFiles();

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
            var files = new System.Collections.Generic.List<FileInfo>();

            files = new DirectoryInfo(directory).GetFiles().OrderBy(f => f.Name).ToList();

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
            PlayButton_Click(sender, e);
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
        #endregion

        #region Ui Colors Changer
        private void SetComponentsColor()
        {
            var color = GetColor();
            MainBorder.Background = new SolidColorBrush(color);
            IsColorBright(color);
        }
        private void IsColorBright(System.Windows.Media.Color c)
        {
            // Calculate perceived brightness based on RGB components
            int brightness = (int)(c.R * 0.30 + c.G * 0.59 + c.B * 0.11);
            if (brightness < 127)
            {
                SongNameTxt.Foreground = new SolidColorBrush(Colors.White);
                SingerNameTxt.Foreground = new SolidColorBrush(Colors.White);
                MinuteLeft.Foreground = new SolidColorBrush(Colors.White);
                MinuteSpend.Foreground = new SolidColorBrush(Colors.White);
                SkipNext.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(203, 185, 187));
                SkipPrevious.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(203, 185, 187));
                CloseThickIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(203, 185, 187));
                FolderIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(203, 185, 187));
                PlayOrPauseIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(203, 185, 187));
                MusicMinutSlider.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(203, 185, 187));
            }
            else
            {
                SongNameTxt.Foreground = new SolidColorBrush(Colors.Black);
                SingerNameTxt.Foreground = new SolidColorBrush(Colors.Black);
                MinuteLeft.Foreground = new SolidColorBrush(Colors.Black);
                MinuteSpend.Foreground = new SolidColorBrush(Colors.Black);
                SkipNext.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 41, 41));
                SkipPrevious.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 41, 41));
                CloseThickIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 41, 41));
                FolderIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 41, 41));
                PlayOrPauseIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 41, 41));
                MusicMinutSlider.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 41, 41));
            }
        }

        private System.Windows.Media.Color GetColor()
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
                System.Windows.Media.Color color = System.Windows.Media.Color.FromRgb(
                    pixels[index + 2], // Red
                    pixels[index + 1], // Green
                    pixels[index]);    // Blue

                return color;
            }
            return System.Windows.Media.Color.FromRgb(0, 0, 0);
        }
        #endregion
    }
}
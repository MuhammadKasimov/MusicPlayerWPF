using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MusicPlayerWPF.Boxes
{
    /// <summary>
    /// Interaction logic for ErrorMessageBox.xaml
    /// </summary>
    public partial class ErrorMessageBox : Window
    {
        private DispatcherTimer timer;

        public ErrorMessageBox(Window parent, string message, int secondsToClose)
        {
            InitializeComponent();
            ErrorText.Text = message;

            // Set message box position relative to parent window (top center)
            if (parent != null)
            {
                this.Owner = parent; // Set owner for proper Z-ordering
                this.WindowStartupLocation = WindowStartupLocation.Manual; // Manual positioning
                this.Left = parent.Left + (parent.Width - this.Width) / 2; // Center horizontally
                this.Top = parent.Top + 20; // Near the top of the parent window (20px from top)
            }

            // Set up the timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(secondsToClose);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            // Fade out animation
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(1)));
            fadeOut.Completed += FadeOut_Completed; // Close window after animation
            this.BeginAnimation(Window.OpacityProperty, fadeOut);
        }

        private void FadeOut_Completed(object sender, EventArgs e)
        {
            this.Close(); // Close the window after fade-out animation completes
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace MusicPlayerWPF.Boxes
{
    /// <summary>
    /// Interaction logic for ErrorMessageBox.xaml
    /// </summary>
    public partial class ErrorMessageBox : UserControl
    {
        private DispatcherTimer timer;

        public ErrorMessageBox(string message, int secondsToClose)
        {
            InitializeComponent();
            ErrorText.Text = message;

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
            fadeOut.Completed += FadeOut_Completed; // Close after animation
            this.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void FadeOut_Completed(object sender, EventArgs e)
        {
            // Remove the control from the parent
            var parent = this.Parent as Panel;
            parent?.Children.Remove(this); // Assuming the control is in a panel
        }
    }
}

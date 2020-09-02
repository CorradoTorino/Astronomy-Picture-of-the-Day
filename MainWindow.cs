// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AstronomyPictureOfTheDay
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AstronomyPictureOfTheDayResponse astronomyPictureOfTheDay = null;

        private readonly Downloader downloader = new Downloader();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void DatePicker_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            DebugUtils.WriteLine("Entering DatePicker_OnSelectedDateChanged");
            this.DisplayTransitionScreen();
            
            try
            {
                this.astronomyPictureOfTheDay = await this.downloader.DownloadDefinitionForAstronomyPictureOfTheDayAsync(this.DatePicker.SelectedDate);
                DebugUtils.WriteLine("Continue after DownloadDefinitionForAstronomyPictureOfTheDay");

                await this.downloader.DownloadImage(this.astronomyPictureOfTheDay, this.DownloadingProgressBar);
                DebugUtils.WriteLine("Continue after DownloadImage");

                this.UpdateUi(
                    this.astronomyPictureOfTheDay.title,
                    this.astronomyPictureOfTheDay.explanation,
                    $".\\Samples\\APOD_{DatePicker.SelectedDate:yyyy-MM-dd}.jpg");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null
                    ? $".{Environment.NewLine}{ex.InnerException.Message}"
                    : "";
                
                this.UpdateUi(
                    "Ops.. Something went wrong..",
                    $"{ex.Message}{innerMessage}",
                    "ErrorOccured.jpg");
            }
        }
        
        private void UpdateUi(string title, string explanation, string imagePath)
        {
            this.TitleTextBox.Text = title;
            this.ExplanationTextBox.Text = explanation;
            this.AstronomyImage.Source = CreateBitmapImage(new Uri(imagePath, UriKind.Relative));

            this.FinalizeTransition();
        }

        private void FinalizeTransition()
        {
            this.DatePicker.IsEnabled = true;
            this.DownloadingProgressBar.Visibility = Visibility.Hidden;
            this.AstronomyImage.Visibility = Visibility.Visible;
        }

        private void DisplayTransitionScreen()
        {
            this.DatePicker.IsEnabled = false;
            this.TitleTextBox.Text = "";
            this.ExplanationTextBox.Text = $"Downloading the astronomy picture of the day for {DatePicker.SelectedDate:yyyy-MM-dd}";
            this.AstronomyImage.Visibility = Visibility.Hidden;
            this.DownloadingProgressBar.Visibility = Visibility.Visible;
            this.DownloadingProgressBar.Value = 0;
        }
        
        private static BitmapImage CreateBitmapImage(Uri uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
    }
}
// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AstronomyPictureOfTheDay.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AstronomyPictureOfTheDayResponse astronomyPictureOfTheDay = null;

        private readonly Downloader downloader = new Downloader();

        private List<DateTime> unsupportedDays;
            
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void DatePicker_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            DebugUtils.WriteLine("Entering DatePicker_OnSelectedDateChanged");

            var selectedDate = this.DatePicker.SelectedDate.GetValueOrDefault(DateTime.Now);

            this.DisplayTransitionScreen();
            
            try
            {
                this.astronomyPictureOfTheDay = 
                    this.downloader.DownloadDefinitionForAstronomyPictureOfTheDay(selectedDate);
                DebugUtils.WriteLine("Continue after DownloadDefinitionForAstronomyPictureOfTheDay");

                await this.downloader.DownloadImage(this.astronomyPictureOfTheDay);
                DebugUtils.WriteLine("Continue after DownloadImage");

                this.UpdateUi(
                    this.astronomyPictureOfTheDay.title,
                    this.astronomyPictureOfTheDay.explanation,
                    $".\\Samples\\APOD_{selectedDate:yyyy-MM-dd}.jpg");
            }
            catch (OperationCanceledException)
            {
                this.UpdateUi(
                    "Operation Cancelled successfully.",
                    $"The operation was cancelled as requested.",
                    "StopWaiting.jpg");
            }
            catch (NotSupportedException)
            {
                this.unsupportedDays.Add(selectedDate);
                this.UpdateUi(
                    "Not supported media format.",
                    $"This day contain a not supported media format. This version load only image.",
                    "NotSupported.jpg");
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

            this.setBlackoutDays();
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
            this.CancelButton.Visibility = Visibility.Hidden;
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
            this.CancelButton.Visibility = Visibility.Visible;
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

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void DatePicker_OnCalendarOpened(object sender, RoutedEventArgs e)
        {
            if (this.unsupportedDays != null)
            {
                return;
            }
            
            this.DatePicker.DisplayDateEnd = DateTime.Now;
            this.unsupportedDays = new List<DateTime>();
        }

        private void setBlackoutDays()
        {
            foreach (var day in this.unsupportedDays)
            {
                if (this.DatePicker.SelectedDate.GetValueOrDefault(DateTime.MinValue) != day)
                {
                    this.DatePicker.BlackoutDates.Add(new CalendarDateRange(day, day));
                }
            }
        }
    }
}
// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
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

        private readonly string NasaApiKey;
        
        public MainWindow()
        {
            InitializeComponent();
            this.NasaApiKey = Environment.GetEnvironmentVariable("NASA_API_KEY", EnvironmentVariableTarget.Machine) ?? "DEMO_KEY";
        }

        private async void DatePicker_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            this.WriteDebugLine("Entering DatePicker_OnSelectedDateChanged");
            this.DisplayTransitionScreen();
            
            try
            {
                this.astronomyPictureOfTheDay = await this.DownloadDefinitionForAstronomyPictureOfTheDay();
                this.WriteDebugLine("Continue after DownloadDefinitionForAstronomyPictureOfTheDay");

                await this.DownloadImage();
                this.WriteDebugLine("Continue after DownloadImage");

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

        private async Task<AstronomyPictureOfTheDayResponse> DownloadDefinitionForAstronomyPictureOfTheDay()
        {
            this.WriteDebugLine("Entering DownloadDefinitionForAstronomyPictureOfTheDay"); 
            
            var file = $".\\Samples\\APOD_{DatePicker.SelectedDate:yyyy-MM-dd}.json";

            if (!File.Exists(file))
            {
                using var client = new WebClient();

                var address = $"https://api.nasa.gov/planetary/apod?date={DatePicker.SelectedDate:yyyy-MM-dd}&api_key={this.NasaApiKey}";
                await client.DownloadFileTaskAsync(address, file).ConfigureAwait(false);
                this.WriteDebugLine($"Continue after DownloadFileTaskAsync {file}");
            }

            var apodResponseAsString = await File.ReadAllTextAsync(file).ConfigureAwait(false);
            this.WriteDebugLine("Continue after File.ReadAllTextAsync");

            return JsonSerializer.Deserialize<AstronomyPictureOfTheDayResponse>(apodResponseAsString);
        }
        
        private async Task DownloadImage()
        {
            this.WriteDebugLine("Entering DownloadImage");

            if (this.astronomyPictureOfTheDay.media_type != "image")
            {
                throw new NotSupportedException($"Not Supported media type: {this.astronomyPictureOfTheDay.media_type}");
            }

            var url = this.astronomyPictureOfTheDay.hdurl ?? this.astronomyPictureOfTheDay.url;

            var file = $".\\Samples\\APOD_{DatePicker.SelectedDate:yyyy-MM-dd}.jpg";
            if (!File.Exists(file))
            {
                using var client = new WebClient();
                client.DownloadProgressChanged += (s, e) =>
                {
                    this.DownloadingProgressBar.Value = e.ProgressPercentage;
                };

                await client.DownloadFileTaskAsync(url, file).ConfigureAwait(false);
                this.WriteDebugLine($"Continue after DownloadFileTaskAsync {file}");
            }
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

        private void WriteDebugLine(string message ="")
        {
            var methodBase = new StackTrace().GetFrame(1).GetMethod();

            Debug.WriteLine($"[{DateTime.Now:yyyy.MM.dd HH:mm:ss:ffff}] - " +
                            $"[{methodBase.Name}] - " +
                            $"[{Thread.CurrentThread.ManagedThreadId}] - " +
                            $"{message}");
        }
    }
}
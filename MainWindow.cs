// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net;
using System.Text.Json;
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
            this.DisplayTransitionScreen();
            
            try
            {
                this.astronomyPictureOfTheDay = await this.DownloadDefinitionForAstronomyPictureOfTheDay();
                await this.DownloadImage();

                this.UpdateUi(
                    this.astronomyPictureOfTheDay.title,
                    this.astronomyPictureOfTheDay.explanation,
                    $".\\Samples\\APOD_{DatePicker.SelectedDate:yyyy-MM-dd}.jpg");
            }
            catch (Exception ex)
            {
                this.UpdateUi(
                    "Ops.. Something went wrong..",
                    ex.Message,
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
            var file = $".\\Samples\\APOD_{DatePicker.SelectedDate:yyyy-MM-dd}.json";

            if (!File.Exists(file))
            {
                using var client = new WebClient();

                var address = $"https://api.nasa.gov/planetary/apod?date={DatePicker.SelectedDate:yyyy-MM-dd}&api_key={this.NasaApiKey}";
                await client.DownloadFileTaskAsync(address, file);
            }

            var apodResponseAsString = File.ReadAllText(file);

            return JsonSerializer.Deserialize<AstronomyPictureOfTheDayResponse>(apodResponseAsString);
        }
        
        private async Task DownloadImage()
        {
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
                await client.DownloadFileTaskAsync(url, file);
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
    }
}
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
            this.NasaApiKey = Environment.GetEnvironmentVariable("NASA_API_KEY") ?? "DEMO_KEY";
        }

        private async void DatePicker_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            this.DisplayTransitionScreen();
            try
            {
                await this.DisplayAstronomyPictureOfTheDay();
            }
            catch (Exception ex)
            {
                this.DisplayErrorScreen(ex);
            }
        }

        private void DisplayErrorScreen(Exception exception)
        {
            this.TitleTextBox.Text = "Ops.. Something went wrong..";
            this.ExplanationTextBox.Text = exception.Message;

            var uri = new Uri("ErrorOccured.jpg", UriKind.Relative);
            this.AstronomyImage.Source = CreateBitmapImage(uri);

            this.DatePicker.IsEnabled = true;
            this.DownloadingProgress.IsActive = false;
        }

        private async Task DisplayAstronomyPictureOfTheDay()
        {
            this.astronomyPictureOfTheDay = await this.LoadAstronomyPictureOfTheDay();
            var image = await this.LoadImage();

            TitleTextBox.Text = this.astronomyPictureOfTheDay.title;
            ExplanationTextBox.Text = this.astronomyPictureOfTheDay.explanation;
            AstronomyImage.Source = image;

            this.DatePicker.IsEnabled = true;
            this.DownloadingProgress.IsActive = false;
        }

        private void DisplayTransitionScreen()
        {
            this.DatePicker.IsEnabled = false;
            this.TitleTextBox.Text = "";
            this.ExplanationTextBox.Text = $"Downloading the astronomy picture of the day for {DatePicker.SelectedDate:yyyy-MM-dd}";
            this.AstronomyImage.IsEnabled = false;
            this.DownloadingProgress.IsActive = true;
        }

        private async Task<AstronomyPictureOfTheDayResponse> LoadAstronomyPictureOfTheDay()
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
        
        private async Task<BitmapImage> LoadImage()
        {
            if (this.astronomyPictureOfTheDay.media_type != "image")
            {
                throw new NotSupportedException($"Not Supported media type: {this.astronomyPictureOfTheDay.media_type}");
            }

            var url = this.astronomyPictureOfTheDay.hdurl ?? this.astronomyPictureOfTheDay.url;

            var imageFile = $".\\Samples\\{Path.GetFileName(url)}";
            if (!File.Exists(imageFile))
            {
                using var client = new WebClient();
                await client.DownloadFileTaskAsync(url, imageFile);
            }

            var uri = new Uri(imageFile, UriKind.Relative);

            return CreateBitmapImage(uri);
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
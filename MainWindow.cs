// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
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
        
        public MainWindow()
        {
            InitializeComponent();
            this.DisplayAstronomyPictureOfTheDay();
        }

        private void DatePicker_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.DisplayAstronomyPictureOfTheDay();
            }
            catch (Exception ex)
            {
                this.DisplayErrorScreen(ex);
            }
        }

        private void DisplayErrorScreen(Exception exception)
        {
            if (TitleTextBox != null)
                TitleTextBox.Text = "Ops.. Something went wrong..";
            
            if (ExplanationTextBox != null)
                ExplanationTextBox.Text = exception.Message;

            if (ImageViewer1 != null)
            {
                var uri = new Uri("ErrorOccured.jpg", UriKind.Relative);

                ImageViewer1.Source = CreateBitmapImage(uri);
            }
        }

        private void DisplayAstronomyPictureOfTheDay()
        {
            this.astronomyPictureOfTheDay = this.LoadAstronomyPictureOfTheDay();

            if (TitleTextBox != null)
                TitleTextBox.Text = this.astronomyPictureOfTheDay.title;

            if (ExplanationTextBox != null)
                ExplanationTextBox.Text = this.astronomyPictureOfTheDay.explanation;

            if (ImageViewer1 != null)
            {
                ImageViewer1.Source = this.LoadImage();
            }
        }

        private AstronomyPictureOfTheDayResponse LoadAstronomyPictureOfTheDay()
        {
            var file = $".\\Samples\\APOD_{DatePicker.SelectedDate:yyyy-MM-dd}.json";

            if (!File.Exists(file))
            {
                using var client = new WebClient();

                var address = $"https://api.nasa.gov/planetary/apod?date={DatePicker.SelectedDate:yyyy-MM-dd}&api_key=DEMO_KEY";
                client.DownloadFile(address, file);
            }

            var apodResponseAsString = File.ReadAllText(file);

            return JsonSerializer.Deserialize<AstronomyPictureOfTheDayResponse>(apodResponseAsString);
        }
        
        private BitmapImage LoadImage()
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
                client.DownloadFile(url, imageFile);
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
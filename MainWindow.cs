// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text.Json;
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
        private Payload Adof = null;

        public MainWindow()
        {
            InitializeComponent();
            this.UpdateAdof();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.UpdateAdof();
        }

        private void UpdateAdof()
        {
            this.Adof = this.GetAdof();

            if(TitleTextBox!=null)
                TitleTextBox.Text = this.Adof.title;

            if(ExplanationTextBox!=null)
                ExplanationTextBox.Text = this.Adof.explanation;

            if(ImageViewer1!=null)
                ImageViewer1.Source = this.GetImage();

        }

        private Payload GetAdof()
        {
            return JsonSerializer.Deserialize<Payload>(File.ReadAllText($".\\Samples\\APOD_{DatePicker.SelectedDate:yyyy-MM-dd}.json"));
        }
        
        private BitmapImage GetImage()
        {
            var selectedFileName = $".\\Samples\\{Path.GetFileName(this.Adof.url)}";
            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(selectedFileName, UriKind.Relative);
            bitmap.EndInit();
            return bitmap;
        }

        private void DatePicker_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            this.UpdateAdof();
        }
    }
}
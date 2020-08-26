// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;


namespace HelloWorld
{
    public class Payload
    {
        public string copyright { get; set; }

        public DateTime date { get; set; }

        public string explanation { get; set; }

        public string hdurl { get; set; }

        public string media_type { get; set; }

        public string service_version { get; set; }

        public string title { get; set; }

        public string url { get; set; }
    }

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DateTime date = new DateTime(2020, 08, 24);

        private Payload Adof = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Adof = this.GetAdof();
            TitleTextBox.Text = this.Adof.title;
            ExplanationTextBox.Text = this.Adof.explanation;
            ImageViewer1.Source = this.GetImage();
        }

        private Payload GetAdof()
        {
            return JsonSerializer.Deserialize<Payload>(File.ReadAllText($".\\Samples\\APOD_{this.date:yyyy-MM-dd}.json"));
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
    }
}
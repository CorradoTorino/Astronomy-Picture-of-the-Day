using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AstronomyPictureOfTheDay.UI
{
    public class Downloader
    {
        private readonly string NasaApiKey;

        public Downloader()
        {
            this.NasaApiKey = Environment.GetEnvironmentVariable("NASA_API_KEY", EnvironmentVariableTarget.Machine) ??
                              "DEMO_KEY";
        }

        public AstronomyPictureOfTheDayResponse DownloadDefinitionForAstronomyPictureOfTheDay(DateTime? dateToDownload)
        {
            DebugUtils.WriteLine("Entering DownloadDefinitionForAstronomyPictureOfTheDay");

            var file = $".\\Samples\\APOD_{dateToDownload:yyyy-MM-dd}.json";

            if (!File.Exists(file))
            {
                using var client = new WebClient();
                var address = $"https://api.nasa.gov/planetary/apod?date={dateToDownload:yyyy-MM-dd}&api_key={this.NasaApiKey}";
                client.DownloadFile(address, file);
                DebugUtils.WriteLine($"Continue after DownloadFileTask {file}");
            }

            var apodResponseAsString = File.ReadAllText(file);
            DebugUtils.WriteLine("Continue after File.ReadAllText");

            return JsonSerializer.Deserialize<AstronomyPictureOfTheDayResponse>(apodResponseAsString);
        }
        
        public void DownloadImage(AstronomyPictureOfTheDayResponse astronomyPictureOfTheDay)
        {
            DebugUtils.WriteLine("Entering DownloadImage");

            if (astronomyPictureOfTheDay.media_type != "image")
            {
                throw new NotSupportedException($"Not Supported media type: {astronomyPictureOfTheDay.media_type}");
            }

            var url = astronomyPictureOfTheDay.hdurl ?? astronomyPictureOfTheDay.url;

            var file = $".\\Samples\\APOD_{astronomyPictureOfTheDay.date:yyyy-MM-dd}.jpg";
            if (!File.Exists(file))
            {
                using var client = new WebClient();
                client.DownloadFile(url, file);
                DebugUtils.WriteLine($"Continue after DownloadFileTaskAsync {file}");
            }
        }

    }
}
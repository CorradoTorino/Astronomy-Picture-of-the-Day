﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AstronomyPictureOfTheDay
{
    public class Downloader
    {
        private readonly string NasaApiKey;

        public Downloader()
        {
            this.NasaApiKey = Environment.GetEnvironmentVariable("NASA_API_KEY", EnvironmentVariableTarget.Machine) ??
                              "DEMO_KEY";
        }

        public async Task<AstronomyPictureOfTheDayResponse> DownloadDefinitionForAstronomyPictureOfTheDay(
            DateTime? dateToDownload, CancellationToken cancellationToken)
        {
            DebugUtils.WriteLine("Entering DownloadDefinitionForAstronomyPictureOfTheDay");

            var file = $".\\Samples\\APOD_{dateToDownload:yyyy-MM-dd}.json";

            if (!File.Exists(file))
            {
                using var client = new HttpClient();
                var address = $"https://api.nasa.gov/planetary/apod?date={dateToDownload:yyyy-MM-dd}&api_key={this.NasaApiKey}";
                await client.DownloadFileAsync(address, file, cancellationToken).ConfigureAwait(false);
                DebugUtils.WriteLine($"Continue after DownloadFileTaskAsync {file}");
            }

            var apodResponseAsString = await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false);
            DebugUtils.WriteLine("Continue after File.ReadAllTextAsync");

            return JsonSerializer.Deserialize<AstronomyPictureOfTheDayResponse>(apodResponseAsString);
        }



        public async Task DownloadImage(AstronomyPictureOfTheDayResponse astronomyPictureOfTheDay,
            ProgressBar downloadingProgressBar, CancellationToken cancellationToken)
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
                var progress = new Progress<double>(percent => { downloadingProgressBar.Value = percent; });

                using var client = new HttpClient();
                await client.DownloadFileAsync(url, file, progress, cancellationToken);
                DebugUtils.WriteLine($"Continue after DownloadFileTaskAsync {file}");
            }
        }

    }
}
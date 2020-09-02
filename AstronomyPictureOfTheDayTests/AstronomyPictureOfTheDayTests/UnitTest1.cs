using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AstronomyPictureOfTheDay;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstronomyPictureOfTheDayTests
{
    public enum DownloadStyle
    {
        SequentiallySynchronously = 0,
        SequentiallyAsynchronously = 1,
        InParallelSynchronously = 2,
        InParallelAsynchronously = 3,
    }

    [TestClass]
    public class DownloaderTests
    {
        private const int dayToDownload = 30;

        [TestInitialize]
        public void TestInitialize()
        {
            this.DeleteDownloadFolder();
        }

        private void DeleteDownloadFolder()
        {
            foreach (var file in new DirectoryInfo(".\\Samples\\").GetFiles())
            {
                file.Delete();
            }
        }

        [TestMethod]
        public async Task BenchmarkDownloader()
        {
            var sut = new Downloader();
            const long maxDownloadTime = 3000;

            var apodFiles = new DirectoryInfo(".\\Samples\\").GetFiles();

            foreach (var style in Enum.GetValues(typeof(DownloadStyle)))
            {
                for (int days = 0; days < apodFiles.Length; days++)
                {
                    long downloadTime = 0;
                    switch (style)
                    {
                        case DownloadStyle.SequentiallySynchronously:
                            downloadTime = this.DownloadLastDaysSynchronously(sut,
                                DateTime.Now.AddDays(-1),
                                days);
                            break;

                        case DownloadStyle.SequentiallyAsynchronously:
                            downloadTime = await this.DownloadLastDaysAsynchronouslySequentially(sut,
                                DateTime.Now.AddDays(-1),
                                days);
                            break;

                        case DownloadStyle.InParallelSynchronously:
                            downloadTime = this.DownloadLastDaysInParallel(sut,
                                DateTime.Now.AddDays(-1),
                                days);
                            break;

                        case DownloadStyle.InParallelAsynchronously:
                            downloadTime = await this.DownloadInParallelAsynchronously(sut,
                                DateTime.Now.AddDays(-1),
                                days);
                            break;

                        default:
                            downloadTime = long.MaxValue;
                            break;
                    }

                    Console.WriteLine($@"{style},{days},{downloadTime}");
                    days++;
                }
            }
        }

        [TestMethod]
        [Ignore]
        public void TestDownloadLastDaysSynchronously()
        {
            var sut = new Downloader();
            this.DownloadLastDaysSynchronously(sut, DateTime.Now, dayToDownload);
        }

        [TestMethod]
        public void TestDownloadLastDaysInParallel()
        {
            var sut = new Downloader();
            this.DownloadLastDaysInParallel(sut, DateTime.Now.AddDays(-1), dayToDownload);
        }

        [TestMethod]
        [Ignore]
        public async Task TestDownloadLastDaysAsyncSequentially()
        {
            var sut = new Downloader();
            await this.DownloadLastDaysAsynchronouslySequentially(sut, DateTime.Now, dayToDownload);
        }

        [TestMethod]
        public async Task TestDownloadLastDaysAsyncInParallel()
        {
            var sut = new Downloader();
            await this.DownloadInParallelAsynchronously(sut, new DateTime(2020, 8, 30), dayToDownload);
        }

        private long DownloadLastDaysSynchronously(Downloader downloader, DateTime lastDayToDownload, int numberOfDaysToDownload)
        {
            var days = Enumerable.Range(0, numberOfDaysToDownload)
                .Select(i => lastDayToDownload.AddDays(-i))
                .ToArray();

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var day in days)
            {
                try
                {
                    downloader.DownloadDefinitionForAstronomyPictureOfTheDay(day);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                
            }
            stopWatch.Stop();

            Console.WriteLine($@"Definitions downloaded in {stopWatch.ElapsedMilliseconds}.");
            return stopWatch.ElapsedMilliseconds;
        }

        private long DownloadLastDaysInParallel(Downloader downloader, DateTime lastDayToDownload, int numberOfDaysToDownload)
        {
            var days = Enumerable.Range(0, numberOfDaysToDownload)
                .Select(i => lastDayToDownload.AddDays(-i))
                .ToArray();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Parallel.ForEach(days, (day) => { downloader.DownloadDefinitionForAstronomyPictureOfTheDay(day); });

            stopWatch.Stop();

            Console.WriteLine($@"Definitions downloaded in {stopWatch.ElapsedMilliseconds}.");

            return stopWatch.ElapsedMilliseconds;
        }

        private async Task<long> DownloadLastDaysAsynchronouslySequentially(Downloader downloader, DateTime lastDayToDownload, int numberOfDaysToDownload)
        {
            var days = Enumerable.Range(0, numberOfDaysToDownload)
                .Select(i => lastDayToDownload.AddDays(-i))
                .ToArray();

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var day in days)
            {
                try
                {
                    await downloader.DownloadDefinitionForAstronomyPictureOfTheDayAsync(day);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                
            }
            stopWatch.Stop();

            Console.WriteLine($@"Definitions downloaded in {stopWatch.ElapsedMilliseconds}.");

            return stopWatch.ElapsedMilliseconds;
        }

        private async Task<long> DownloadInParallelAsynchronously(Downloader downloader, DateTime lastDayToDownload, int numberOfDaysToDownload)
        {
            var days = Enumerable.Range(0, numberOfDaysToDownload)
                .Select(i => lastDayToDownload.AddDays(-i))
                .ToArray();
            
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var tasks = days
                .Select((day) => downloader.DownloadDefinitionForAstronomyPictureOfTheDayAsync(day))
                .ToArray();

            var task = Task.WhenAll(tasks);

            try
            {
                await task;
            }
            catch (Exception)
            {
                foreach (var e in task.Exception.Flatten().InnerExceptions)
                {
                    Console.WriteLine(e.Message);
                }
            }

            stopWatch.Stop();

            Console.WriteLine($@"Definitions downloaded in {stopWatch.ElapsedMilliseconds}.");

            return stopWatch.ElapsedMilliseconds;
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AstronomyPictureOfTheDay
{
    public static class HttpClientExtensions
    {
        public static async Task DownloadFileAsync(this HttpClient client,
            string url,
            string fileName,
            IProgress<double> progress = null,
            CancellationToken token = default(CancellationToken))
        {
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"The request returned with HTTP status code {response.StatusCode}");
            }

            var total = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = total != -1 && progress != null;

            try
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    var totalRead = 0L;
                    var buffer = new byte[4096];
                    var isMoreToRead = true;

                    do
                    {
                        token.ThrowIfCancellationRequested();

                        var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            var data = new byte[read];
                            buffer.ToList().CopyTo(0, data, 0, read);

                            fs.Write(data, 0, data.Length);
                            totalRead += read;

                            if (canReportProgress)
                            {
                                progress.Report((totalRead * 1d) / (total * 1d) * 100);
                            }
                        }
                    } while (isMoreToRead);
                }
            }
            catch (OperationCanceledException)
            {
                File.Delete(fileName);
                throw;
            }
        }

        public static Task DownloadFileAsync(this HttpClient client,
            string url,
            string fileName,
            CancellationToken token = default(CancellationToken))
        {
            return DownloadFileAsync(client, url, fileName, null, token);
        }

        public static Task DownloadFileAsync(this HttpClient client,
            string url,
            string fileName)
        {
            return DownloadFileAsync(client, url, fileName, null, default(CancellationToken));
        }
    }
}
using System;
using System.Diagnostics;
using System.Threading;

namespace AstronomyPictureOfTheDay
{
    public static class DebugUtils
    {
        public static void WriteLine(string message = "")
        {
            var methodBase = new StackTrace().GetFrame(1).GetMethod();

            Debug.WriteLine($"[{DateTime.Now:yyyy.MM.dd HH:mm:ss:ffff}] - " +
                            $"[{methodBase.Name}] - " +
                            $"[{Thread.CurrentThread.ManagedThreadId}] - " +
                            $"{message}");
        }
    }
}
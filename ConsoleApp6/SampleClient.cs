#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp6
{
    internal class SampleClient : IDisposable
    {
        private CancellationTokenSource? cts;
        private Task? requestTask;

        public void Start(string url)
        {
            this.cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            this.requestTask = Task.Run(
                async () =>
                {
                    using var source = new ActivitySource(nameof(SampleClient));
                    using var client = new HttpClient();

                    var count = 1;
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var content = new StringContent($"client message: {DateTime.Now}", Encoding.UTF8);

                        using (var activity = source.StartActivity("client POST:" + url, ActivityKind.Client))
                        {
                            count++;

                            activity?.AddEvent(new ActivityEvent("PostAsync:Started"));
                            using var response = await client.PostAsync(url, content, cancellationToken)
                                .ConfigureAwait(false);
                            activity?.AddEvent(new ActivityEvent("PostAsync:Ended"));

                            activity?.SetTag("http.status_code", $"{response.StatusCode:D}");

                            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                            activity?.SetTag("response.content", responseContent);
                            activity?.SetTag("response.length",
                                responseContent.Length.ToString(CultureInfo.InvariantCulture));
                            activity?.SetTag("current.count", count);

                            foreach (var (key, value) in response.Headers)
                            {
                                switch (value)
                                {
                                    case IEnumerable<object> enumerable:
                                        activity?.SetTag($"http.header.{key}", string.Join(",", enumerable));
                                        break;
                                    default:
                                        activity?.SetTag($"http.header.{key}", value.ToString());
                                        break;
                                }
                            }
                        }

                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
                        }
                        catch (TaskCanceledException)
                        {
                            return;
                        }
                    }
                },
                cancellationToken);
        }

        public void Dispose()
        {
            if (cts == null) return;
            
            cts.Cancel();
            requestTask?.Wait();
            requestTask?.Dispose();
            cts.Dispose();
        }
    }
}
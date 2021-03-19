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
        private CancellationTokenSource cts;
        private Task requestTask;

        public void Start(string url)
        {
            this.cts = new CancellationTokenSource();
            var cancellationToken = this.cts.Token;

            this.requestTask = Task.Run(
                (Func<Task?>) (async () =>
                {
                    using var source = new ActivitySource(nameof(SampleClient));
                    using var client = new HttpClient();

                    var count = 1;
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var content = new StringContent($"client message: {DateTime.Now}", Encoding.UTF8);

                        using (var activity = source.StartActivity("POST:" + url, ActivityKind.Client))
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

                            foreach (var header in response.Headers)
                            {
                                if (header.Value is IEnumerable<object> enumerable)
                                {
                                    activity?.SetTag($"http.header.{header.Key}", string.Join(",", enumerable));
                                }
                                else
                                {
                                    activity?.SetTag($"http.header.{header.Key}", header.Value.ToString());
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
                }),
                cancellationToken);
        }

        public void Dispose()
        {
            if (this.cts != null)
            {
                this.cts.Cancel();
                this.requestTask.Wait();
                this.requestTask.Dispose();
                this.cts.Dispose();
            }
        }
    }
}
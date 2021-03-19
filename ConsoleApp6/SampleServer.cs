using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp6
{
    internal class SampleServer : IDisposable
    {
        private readonly HttpListener listener = new();

        public void Start(string url)
        {
            listener.Prefixes.Add(url);
            listener.Start();

            Task.Run(() =>
            {
                using var source = new ActivitySource(nameof(SampleServer));

                while (listener.IsListening)
                {
                    try
                    {
                        var context = listener.GetContext();

                        using var activity = source.StartActivity(
                            $"Server: {context.Request.HttpMethod}:{context.Request.Url?.AbsolutePath}",
                            ActivityKind.Client);

                        var headerKeys = context.Request.Headers.AllKeys;
                        foreach (var headerKey in headerKeys)
                        {
                            string headerValue = context.Request.Headers[headerKey];
                            activity?.SetTag($"http.header.{headerKey}", headerValue);
                        }

                        string requestContent;
                        using (var childSpan = source.StartActivity("ReadStream", ActivityKind.Server))
                        using (var reader = new StreamReader(context.Request.InputStream,
                            context.Request.ContentEncoding))
                        {
                            requestContent = reader.ReadToEnd();
                            childSpan?.AddEvent(new ActivityEvent("StreamReader.ReadToEnd"));
                        }

                        activity?.SetTag("request.content", requestContent);
                        activity?.SetTag("request.length", requestContent.Length.ToString());

                        var echo = Encoding.UTF8.GetBytes("echo: " + requestContent);
                        context.Response.ContentEncoding = Encoding.UTF8;
                        context.Response.ContentLength64 = echo.Length;
                        context.Response.OutputStream.Write(echo, 0, echo.Length);
                        context.Response.Close();
                    }
                    catch (Exception)
                    {
                        // expected when closing the listener.
                    }
                }
            });
        }

        public void Dispose()
        {
            ((IDisposable) listener).Dispose();
        }
    }
}
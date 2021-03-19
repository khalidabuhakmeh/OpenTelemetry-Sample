using System;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ConsoleApp6
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var openTelemetry = Sdk.CreateTracerProviderBuilder()
                .AddSource(nameof(SampleClient))
                .AddSource(nameof(SampleServer))
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("zipkin-test"))
                // default uri: http://localhost:9411/api/v2/spans
                .AddZipkinExporter()
                .Build();

            using var sample = new InstrumentationWithActivitySource();
            sample.Start();

            Console.WriteLine("Traces are being created and exported" +
                              "to Zipkin in the background. Use Zipkin to view them. " +
                              "Press ENTER to stop.");
            Console.ReadLine();
        }

    }
}
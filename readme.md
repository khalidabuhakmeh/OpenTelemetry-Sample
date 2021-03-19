# OpenTelemetry Sample

This stand-alone example is ported from the Open Telemetry repository and designed to make it easy to experiment with the sample.

## Requirements

1. Download and Install [Docker Desktop](https://www.docker.com/products/docker-desktop)
2. [.NET Framework 5](https://dot.net) 

## Getting Started

1. Create a Docker container using the following command:

```
docker run -d -p 9411:9411 openzipkin/zipkin-slim
```

2. From the `ConsoleApp6` directory, run the following command:

```
dotnet run
```

Which will present you with the following output:

```console
Traces are being created and exportedto Zipkin in the background. Use Zipkin to view them. Press ENTER to stop.
```

3. Navigate to http://localhost:9411/zipkin which will allow you to see traces for client and server activities. These are generated from `SampleServer` and `SampleClient`.

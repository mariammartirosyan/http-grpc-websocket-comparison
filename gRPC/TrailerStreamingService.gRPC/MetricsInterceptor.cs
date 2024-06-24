using System.Diagnostics;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Prometheus;

namespace TrailerStreamingService.gRPC
{
    public class MetricsInterceptor : Interceptor
    {
        private static readonly Histogram ResponseTimeHistogram = Metrics.CreateHistogram(
            "grpc_response_time_seconds", "gRPC response time in seconds");

        private static readonly Counter ResponseSizeCounter = Metrics.CreateCounter(
            "grpc_response_size_bytes", "gRPC response size in bytes");

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var stopwatch = Stopwatch.StartNew();
            TResponse response = null;

            try
            {
                response = await continuation(request, context);
                return response;
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

                ResponseTimeHistogram.Observe(elapsedMilliseconds / 1000);

                if (response is IMessage message)
                {
                    ResponseSizeCounter.Inc(message.CalculateSize());
                }
            }
        }
    }


}




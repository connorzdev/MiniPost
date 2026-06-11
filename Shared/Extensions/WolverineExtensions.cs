using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Shared.Extensions;

public static class WolverineExtensions
{
    public static async Task UseWolverineWithRabbitMqAsync(
        this IHostApplicationBuilder builder,
        Action<WolverineOptions> configure
    )
    {
        builder
            .Services.AddOpenTelemetry()
            .WithTracing(traceBuilder =>
            {
                traceBuilder
                    .SetResourceBuilder(
                        ResourceBuilder
                            .CreateDefault()
                            .AddService(builder.Environment.ApplicationName)
                    )
                    .AddSource("Wolverine");
            });

        builder.UseWolverine(options =>
        {
            options
                .UseRabbitMqUsingNamedConnection("messaging")
                .AutoProvision()
                .UseConventionalRouting(x =>
                {
                    x.QueueNameForListener(t =>
                        $"{t.FullName}.{builder.Environment.ApplicationName}"
                    );
                });

            configure(options);
        });
    }
}

using Serilog;

IHost host = Host
    .CreateDefaultBuilder(args)
    .UseSerilog((context, config) =>
    {
        config
            .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture)
            .MinimumLevel.Information();
    })
    .ConfigureServices(services =>
    {
        // MassTransit setup
        services.AddMassTransit(x =>
        {
            // Register all consumers from this assembly
            x.AddConsumers(typeof(Program).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                string? rabbitHost = context.GetRequiredService<IConfiguration>()["RabbitMQ:Host"]
                    ?? context.GetRequiredService<IConfiguration>()["RabbitMQ__Host"]
                    ?? "localhost";
                string? rabbitUser = context.GetRequiredService<IConfiguration>()["RabbitMQ:Username"]
                    ?? context.GetRequiredService<IConfiguration>()["RabbitMQ__Username"]
                    ?? "guest";
                string? rabbitPass = context.GetRequiredService<IConfiguration>()["RabbitMQ:Password"]
                    ?? context.GetRequiredService<IConfiguration>()["RabbitMQ__Password"]
                    ?? "guest";

                cfg.Host(rabbitHost, h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

await host.RunAsync();

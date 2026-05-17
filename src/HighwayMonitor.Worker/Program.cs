using MassTransit;
using HighwayMonitor.Worker.Consumers;
using HighwayMonitor.Worker.Repositories;

var builder = Host.CreateApplicationBuilder(args);

var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "highway_user";
var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "Highway@123456";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddSingleton(new InspecaoRepository(connectionString));
builder.Services.AddSingleton(new AcidenteRepository(connectionString));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<InspecaoConsumer>();
    x.AddConsumer<AcidenteConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ReceiveEndpoint("inspecoes-queue", e =>
        {
            e.ConfigureConsumer<InspecaoConsumer>(context);
        });

        cfg.ReceiveEndpoint("acidentes-queue", e =>
        {
            e.ConfigureConsumer<AcidenteConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
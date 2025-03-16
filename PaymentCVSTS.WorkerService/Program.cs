using PaymentCVSTS.WorkerService;
using PaymentCVSTS.Services.Interfaces;
using PaymentCVSTS.Services.Implements;
using Microsoft.Extensions.Logging.EventLog;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddEventLog(config =>
{
    config.LogName = "PaymentCVSTS Service";
    config.SourceName = "PaymentCVSTS Service Source";
});

// DI
builder.Services.AddSingleton<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IAppointmentService, AppointmentService>();
builder.Services.AddSingleton<IUserAccountService, UserAccountService>();

// Worker Service
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "PaymentCVSTS";
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
using PaymentCVSTS.WorkerService;
using PaymentCVSTS.Services.Interfaces;
using PaymentCVSTS.Services.Implements;

var builder = Host.CreateApplicationBuilder(args);

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
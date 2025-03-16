using PaymentCVSTS.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace PaymentCVSTS.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public Worker(ILogger<Worker> logger, IPaymentService paymentService, IConfiguration configuration)
        {
            _logger = logger;
            _paymentService = paymentService;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("PaymentCVSTS Service started at: {time}", DateTimeOffset.Now);
                Utilities.Log("Service started at: " + DateTimeOffset.Now);

                // Log connection string (redacted for security)
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogInformation("Using database connection (server only): {server}",
                        connectionString.Split(';').FirstOrDefault(s => s.StartsWith("Data Source=")));
                }

                // Continue running until cancellation is requested
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    // Process data
                    await WriteLogDataAsync(stoppingToken);

                    // Wait before next processing cycle (configurable)
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal during shutdown, don't log as error
                _logger.LogInformation("Service operation cancelled at: {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in worker service");
                Utilities.Log(ex);
            }
            finally
            {
                _logger.LogInformation("Service stopping at: {time}", DateTimeOffset.Now);
                Utilities.Log("Service stopping at: " + DateTimeOffset.Now);
            }
        }

        // Changed from async void to async Task - very important for proper error handling
        private async Task WriteLogDataAsync(CancellationToken cancellationToken)
        {
            // Use semaphore to prevent multiple concurrent runs
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                var payments = await _paymentService.GetAll();
                if (payments == null || !payments.Any())
                {
                    _logger.LogInformation("No payments found to log");
                    return;
                }

                _logger.LogInformation("Found {count} payments to log", payments.Count);
                string content = string.Empty;

                foreach (var payment in payments)
                {
                    content += Utilities.ConvertObjectToJSONString(payment) + Environment.NewLine;
                }

                // Properly await the async method
                await Utilities.WriteLoggerFileAsync(content);
                _logger.LogInformation("Successfully logged {count} payments", payments.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing payment data logs");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
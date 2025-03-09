using PaymentCVSTS.Services.Interfaces;

namespace PaymentCVSTS.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IPaymentService _paymentService;

        public Worker(ILogger<Worker> logger, IPaymentService paymentService)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int isCountdown = 10;
            try
            {
                while (!stoppingToken.IsCancellationRequested && isCountdown > 0)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                        Utilities.Log("Worker running at: " + DateTimeOffset.Now);
                        --isCountdown;
                        this.WriteLogData();
                    }
                    await Task.Delay(1000, stoppingToken);
                }

                if (isCountdown == 0)
                {
                    _logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);
                    Utilities.Log("Worker stopped at: " + DateTimeOffset.Now);
                    await StopAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Utilities.Log(ex);
            }
            finally
            {
                if (isCountdown == 0)
                {
                    _logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);
                    Utilities.Log("Worker stopped at: " + DateTimeOffset.Now);
                    await StopAsync(stoppingToken);
                }
            }
        }

        private async void WriteLogData()
        {
            try
            {
                var payments = await _paymentService.GetAll();
                string content = string.Empty;

                foreach (var payment in payments)
                {
                    content += Utilities.ConvertObjectToJSONString(payment) + "\r\n";
                }

                // Properly await the async method
                await Utilities.WriteLoggerFileAsync(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing payment data logs");
            }
        }
    }
}
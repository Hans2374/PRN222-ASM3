using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentCVSTS.WorkerService
{
    public class Utilities
    {
        // Use a semaphore to prevent concurrent file access
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private static string GetLogFolder()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        }

        private static string GetLogFilePath()
        {
            return Path.Combine(GetLogFolder(), "log.txt");
        }

        public static void Log(string message)
        {
            try
            {
                if (!Directory.Exists(GetLogFolder()))
                {
                    Directory.CreateDirectory(GetLogFolder());
                }

                using (StreamWriter streamWriter = new StreamWriter(GetLogFilePath(), true))
                {
                    streamWriter.WriteLine($"{DateTime.Now} - {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log: {ex.Message}");
            }
        }

        public static void Log(Exception ex)
        {
            try
            {
                if (!Directory.Exists(GetLogFolder()))
                {
                    Directory.CreateDirectory(GetLogFolder());
                }

                using (StreamWriter streamWriter = new StreamWriter(GetLogFilePath(), true))
                {
                    streamWriter.WriteLine($"{DateTime.Now} - Error: {ex.Message}");
                    streamWriter.WriteLine($"{DateTime.Now} - Stack: {ex.StackTrace}");
                }
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Error writing error log: {logEx.Message}");
            }
        }

        public static string ConvertObjectToJSONString<T>(T entity)
        {
            try
            {
                return JsonSerializer.Serialize(entity, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                });
            }
            catch (Exception ex)
            {
                Log(ex);
                return $"Error serializing object: {ex.Message}";
            }
        }

        public static async Task WriteLoggerFileAsync(string content)
        {
            // Use semaphore to prevent concurrent file access
            await semaphore.WaitAsync();

            try
            {
                string logFilePath = Path.Combine(GetLogFolder(), "PaymentDataLog.txt");

                // Ensure directory exists
                Directory.CreateDirectory(GetLogFolder());

                // Use File.AppendAllTextAsync instead of StreamWriter
                await File.AppendAllTextAsync(logFilePath, $"{DateTimeOffset.Now}: {content}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging error: {ex.Message}");
                Log(ex);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentCVSTS.WorkerService
{
    public class Utilities
    {
        private static string loggerFildPath = Directory.GetCurrentDirectory() + "\\Logs\\";

        private static string loggerFileName = "log.txt";


        public static void Log(string message)
        {
            if (!Directory.Exists(loggerFildPath))
            {
                Directory.CreateDirectory(loggerFildPath);
            }

            using (StreamWriter streamWriter = new StreamWriter(loggerFildPath + loggerFileName, true))
            {
                streamWriter.WriteLine(message);
            }
        }

        public static void Log(Exception ex)
        {
            if (!Directory.Exists(loggerFildPath))
            {
                Directory.CreateDirectory(loggerFildPath);
            }

            using (StreamWriter streamWriter = new StreamWriter(loggerFildPath + loggerFileName, true))
            {
                streamWriter.WriteLine(DateTime.Now.ToString() + " - " + ex.Message);
                streamWriter.WriteLine(DateTime.Now.ToString() + " - " + ex.StackTrace);
            }

        }

        public static string ConvertObjectToJSONString<T>(T entity)
        {
            return JsonSerializer.Serialize(entity, new JsonSerializerOptions
            {
                WriteIndented = false,
                ReferenceHandler = ReferenceHandler.Preserve
            });
        }

        public static async Task WriteLoggerFileAsync(string content)
        {
            try
            {
                string logFilePath = @"D:\PaymentDataLog.txt";

                // Ensure directory exists
                string? directoryPath = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Use using statement with await to ensure proper disposal
                using (var file = new StreamWriter(logFilePath, append: true))
                {
                    await file.WriteLineAsync($"{DateTimeOffset.Now}: {content}");
                    await file.FlushAsync();
                } // Stream is automatically closed here
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging error: {ex.Message}");
            }
        }
    }
}
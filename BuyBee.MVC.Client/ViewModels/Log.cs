namespace EMart.ViewModels
{
    public static class Log
    {
        private static readonly object _lock = new();

        public static void WriteLog(Exception ex)
        {
            if (ex == null) return;

            try
            {
                lock (_lock)
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "ErrorLog");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string filePath = Path.Combine(folderPath, $"Log_{DateTime.Now:ddMMyyyy}.txt");

                    using (StreamWriter sw = new StreamWriter(filePath, true))
                    {
                        sw.WriteLine("---------------------------------------------------");
                        sw.WriteLine($"Date       : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        sw.WriteLine($"Machine    : {Environment.MachineName}");
                        sw.WriteLine($"User       : {Environment.UserName}");
                        sw.WriteLine($"Message    : {ex.Message}");
                        sw.WriteLine($"Source     : {ex.Source}");
                        sw.WriteLine($"Method     : {ex.TargetSite?.Name}");
                        sw.WriteLine($"StackTrace : {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            sw.WriteLine($"Inner Ex   : {ex.InnerException.Message}");
                        }
                        sw.WriteLine("---------------------------------------------------");
                        sw.WriteLine();
                    }
                }
            }
            catch (Exception logEx)
            {
                // Never recurse into WriteLog to avoid StackOverflowException
                Console.Error.WriteLine($"[Logging Error] Failed to write error log: {logEx.Message}");
                Console.Error.WriteLine($"[Original Error]: {ex.Message}");
            }
        }
    }
}

using System.Text.Json;

namespace EMart.Services
{
    public static class DotEnvLoader
    {
        public static void Load(string? filePath = null)
        {
            filePath ??= Path.Combine(Directory.GetCurrentDirectory(), ".env");

            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                var content = File.ReadAllText(filePath).Trim();

                // Try parsing as JSON first if it looks like JSON
                if (content.StartsWith("{") && content.EndsWith("}"))
                {
                    try
                    {
                        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
                        if (dict != null)
                        {
                            foreach (var (key, val) in dict)
                            {
                                if (!string.IsNullOrWhiteSpace(key) && val != null)
                                {
                                    Environment.SetEnvironmentVariable(key.Trim(), val.Trim());
                                }
                            }
                            return;
                        }
                    }
                    catch
                    {
                        // Fall back to line by line parsing
                    }
                }

                // Line by line parsing (supports "KEY": "VAL", KEY=VAL, KEY="VAL")
                var lines = File.ReadAllLines(filePath);
                foreach (var rawLine in lines)
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("//"))
                    {
                        continue;
                    }

                    // Handle "KEY": "VALUE" or "KEY": "VALUE",
                    if (line.Contains(':') && (line.StartsWith("\"") || line.StartsWith("'")))
                    {
                        var parts = line.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim().Trim('"', '\'');
                            var val = parts[1].Trim().TrimEnd(',').Trim().Trim('"', '\'');
                            if (!string.IsNullOrEmpty(key))
                            {
                                Environment.SetEnvironmentVariable(key, val);
                                continue;
                            }
                        }
                    }

                    // Handle KEY=VALUE
                    if (line.Contains('='))
                    {
                        var parts = line.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim().Trim('"', '\'');
                            var val = parts[1].Trim().Trim('"', '\'');
                            if (!string.IsNullOrEmpty(key))
                            {
                                Environment.SetEnvironmentVariable(key, val);
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DotEnvLoader] Failed to load .env: {ex.Message}");
            }
        }
    }
}

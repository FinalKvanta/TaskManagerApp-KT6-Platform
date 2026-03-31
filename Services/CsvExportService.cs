using System.Text;
using TaskManagerApp.Models;

namespace TaskManagerApp.Services;

public class CsvExportService
{
    private static string CsvFolder =>
        Path.Combine(FileSystem.AppDataDirectory, "Export");

    public async Task<string> ExportTasksAsync(List<TaskItem> tasks)
    {
        Directory.CreateDirectory(CsvFolder);

        var fileName = $"tasks_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = Path.Combine(CsvFolder, fileName);

        var sb = new StringBuilder();
        sb.AppendLine("Id;Title;Description;DueDate;IsCompleted;Priority");

        foreach (var t in tasks)
        {
            var title = EscapeCsv(t.Title);
            var desc = EscapeCsv(t.Description);
            sb.AppendLine($"{t.Id};{title};{desc};{t.DueDate:yyyy-MM-dd};{t.IsCompleted};{t.Priority}");
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        return filePath;
    }

    public async Task<List<TaskItem>> ImportTasksAsync(string filePath)
    {
        var tasks = new List<TaskItem>();
        var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);

        // Пропускаем заголовок
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var parts = ParseCsvLine(line);
            if (parts.Count < 6) continue;

            try
            {
                tasks.Add(new TaskItem
                {
                    Title = parts[1],
                    Description = parts[2],
                    DueDate = DateTime.TryParse(parts[3], out var date) ? date : DateTime.Now.AddDays(7),
                    IsCompleted = bool.TryParse(parts[4], out var completed) && completed,
                    Priority = parts[5]
                });
            }
            catch
            {
                // Пропускаем некорректные строки
            }
        }

        return tasks;
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ';' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result;
    }
}

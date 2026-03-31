using SQLite;

namespace TaskManagerApp.Models;

public class TaskItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);
    public bool IsCompleted { get; set; }
    public string Priority { get; set; } = "Средний";
    public string? PhotoPath { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? LocationAddress { get; set; }
}

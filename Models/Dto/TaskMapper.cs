namespace TaskManagerApp.Models.Dto;

/// <summary>
/// Маппер между API-моделью и локальной моделью
/// </summary>
public static class TaskMapper
{
    public static TaskItem ToLocal(TaskApiModel api)
    {
        return new TaskItem
        {
            Title = api.Title,
            Description = $"Импортировано из облака (userId: {api.UserId})",
            IsCompleted = api.Completed,
            DueDate = DateTime.Now.AddDays(7),
            Priority = api.Completed ? "Низкий" : "Средний"
        };
    }

    public static TaskApiModel ToApi(TaskItem local)
    {
        return new TaskApiModel
        {
            Id = local.Id,
            UserId = 1,
            Title = local.Title,
            Completed = local.IsCompleted
        };
    }
}

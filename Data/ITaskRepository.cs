using TaskManagerApp.Models;

namespace TaskManagerApp.Data;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllTasksAsync();
    Task<TaskItem?> GetTaskByIdAsync(int id);
    Task<int> SaveTaskAsync(TaskItem task);
    Task<int> DeleteTaskAsync(TaskItem task);
    Task SeedDataAsync();
}

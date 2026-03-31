using TaskManagerApp.Models.Dto;

namespace TaskManagerApp.Services;

public interface ITaskApiService
{
    Task<List<TaskApiModel>> GetTasksAsync(int limit = 20);
    Task<TaskApiModel?> GetTaskByIdAsync(int id);
    Task<TaskApiModel> CreateTaskAsync(TaskApiModel task);
    Task<TaskApiModel> UpdateTaskAsync(TaskApiModel task);
    Task<bool> DeleteTaskAsync(int id);
}

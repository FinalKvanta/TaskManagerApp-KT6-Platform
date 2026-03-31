using TaskManagerApp.Models;
using TaskManagerApp.Models.Dto;
using TaskManagerApp.Services;

namespace TaskManagerApp.Data;

/// <summary>
/// Репозиторий с синхронизацией: облако + локальное хранилище.
/// Стратегия: приоритет облака, fallback на локальные данные.
/// </summary>
public class SyncTaskRepository : ITaskRepository
{
    private readonly SQLiteTaskRepository _localRepo;
    private readonly ITaskApiService _apiService;

    public SyncTaskRepository(SQLiteTaskRepository localRepo, ITaskApiService apiService)
    {
        _localRepo = localRepo;
        _apiService = apiService;
    }

    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        return await _localRepo.GetAllTasksAsync();
    }

    public async Task<TaskItem?> GetTaskByIdAsync(int id)
    {
        return await _localRepo.GetTaskByIdAsync(id);
    }

    public async Task<int> SaveTaskAsync(TaskItem task)
    {
        // Сохраняем локально
        var result = await _localRepo.SaveTaskAsync(task);

        // Пробуем синхронизировать с облаком (не блокируем при ошибке)
        try
        {
            var apiModel = TaskMapper.ToApi(task);
            if (task.Id == 0)
                await _apiService.CreateTaskAsync(apiModel);
            else
                await _apiService.UpdateTaskAsync(apiModel);
        }
        catch (ApiException)
        {
            // Облако недоступно — данные сохранены локально
        }

        return result;
    }

    public async Task<int> DeleteTaskAsync(TaskItem task)
    {
        var result = await _localRepo.DeleteTaskAsync(task);

        try
        {
            await _apiService.DeleteTaskAsync(task.Id);
        }
        catch (ApiException)
        {
            // Облако недоступно — удалено локально
        }

        return result;
    }

    /// <summary>
    /// Загрузить задачи из облака и сохранить локально
    /// </summary>
    public async Task<int> SyncFromCloudAsync()
    {
        try
        {
            var apiTasks = await _apiService.GetTasksAsync(20);
            int imported = 0;

            foreach (var apiTask in apiTasks)
            {
                var localTask = TaskMapper.ToLocal(apiTask);
                await _localRepo.SaveTaskAsync(localTask);
                imported++;
            }

            return imported;
        }
        catch (ApiException)
        {
            return -1; // Ошибка синхронизации
        }
    }

    public async Task SeedDataAsync()
    {
        await _localRepo.SeedDataAsync();
    }
}

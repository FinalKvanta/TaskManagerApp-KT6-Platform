using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TaskManagerApp.Models.Dto;

namespace TaskManagerApp.Services;

public class TaskApiService : ITaskApiService
{
    private readonly HttpClient _httpClient;
    private const int MaxRetries = 3;
    private static readonly TimeSpan[] RetryDelays = { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4) };

    public TaskApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<List<TaskApiModel>> GetTasksAsync(int limit = 20)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"todos?_limit={limit}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TaskApiModel>>(content) ?? new List<TaskApiModel>();
        });
    }

    public async Task<TaskApiModel?> GetTaskByIdAsync(int id)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"todos/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TaskApiModel>(content);
        });
    }

    public async Task<TaskApiModel> CreateTaskAsync(TaskApiModel task)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var json = JsonSerializer.Serialize(task);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("todos", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TaskApiModel>(responseContent)!;
        });
    }

    public async Task<TaskApiModel> UpdateTaskAsync(TaskApiModel task)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var json = JsonSerializer.Serialize(task);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"todos/{task.Id}", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TaskApiModel>(responseContent)!;
        });
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.DeleteAsync($"todos/{id}");
            return response.IsSuccessStatusCode;
        });
    }

    /// <summary>
    /// Retry-логика: повторяет запрос до MaxRetries раз при сетевых ошибках
    /// </summary>
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action)
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return await action();
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                if (attempt < MaxRetries)
                {
                    await Task.Delay(RetryDelays[attempt]);
                }
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                // Таймаут
                lastException = ex;
                if (attempt < MaxRetries)
                {
                    await Task.Delay(RetryDelays[attempt]);
                }
            }
        }

        throw new ApiException(
            $"Не удалось выполнить запрос после {MaxRetries + 1} попыток",
            lastException);
    }
}

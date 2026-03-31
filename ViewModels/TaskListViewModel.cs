using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskManagerApp.Data;
using TaskManagerApp.Models;
using TaskManagerApp.Services;
using TaskManagerApp.Views;

namespace TaskManagerApp.ViewModels;

public class TaskListViewModel : BaseViewModel
{
    private readonly SyncTaskRepository _syncRepository;
    private readonly CsvExportService _csvService;

    private ObservableCollection<TaskItem> _allTasks = new();
    private ObservableCollection<TaskItem> _tasks = new();
    private TaskItem? _selectedTask;
    private string _searchText = string.Empty;
    private string _filterStatus = "Все";
    private bool _isLoading;
    private string _syncStatus = string.Empty;

    public ObservableCollection<TaskItem> Tasks
    {
        get => _tasks;
        set => SetProperty(ref _tasks, value);
    }

    public TaskItem? SelectedTask
    {
        get => _selectedTask;
        set => SetProperty(ref _selectedTask, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            ApplyFilter();
        }
    }

    public string FilterStatus
    {
        get => _filterStatus;
        set
        {
            SetProperty(ref _filterStatus, value);
            ApplyFilter();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string SyncStatus
    {
        get => _syncStatus;
        set => SetProperty(ref _syncStatus, value);
    }

    public ICommand LoadTasksCommand { get; }
    public ICommand SelectTaskCommand { get; }
    public ICommand AddTaskCommand { get; }
    public ICommand FilterAllCommand { get; }
    public ICommand FilterActiveCommand { get; }
    public ICommand FilterCompletedCommand { get; }
    public ICommand ExportCsvCommand { get; }
    public ICommand ImportCsvCommand { get; }
    public ICommand SyncFromCloudCommand { get; }
    public ICommand RefreshCommand { get; }

    public TaskListViewModel(SyncTaskRepository syncRepository, CsvExportService csvService)
    {
        _syncRepository = syncRepository;
        _csvService = csvService;

        LoadTasksCommand = new Command(async () => await LoadTasksAsync());
        SelectTaskCommand = new Command<TaskItem>(async (task) => await OnTaskSelected(task));
        AddTaskCommand = new Command(async () => await OnAddTask());
        FilterAllCommand = new Command(() => FilterStatus = "Все");
        FilterActiveCommand = new Command(() => FilterStatus = "Активные");
        FilterCompletedCommand = new Command(() => FilterStatus = "Завершённые");
        ExportCsvCommand = new Command(async () => await OnExportCsv());
        ImportCsvCommand = new Command(async () => await OnImportCsv());
        SyncFromCloudCommand = new Command(async () => await OnSyncFromCloud());
        RefreshCommand = new Command(async () => await LoadTasksAsync());

        Task.Run(async () => await LoadTasksAsync());
    }

    public async Task LoadTasksAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        SyncStatus = "Загрузка...";

        try
        {
            await _syncRepository.SeedDataAsync();
            var tasks = await _syncRepository.GetAllTasksAsync();
            _allTasks = new ObservableCollection<TaskItem>(tasks);
            ApplyFilter();
            SyncStatus = $"Загружено задач: {tasks.Count}";
        }
        catch (Exception ex)
        {
            SyncStatus = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnSyncFromCloud()
    {
        if (IsLoading) return;
        IsLoading = true;
        SyncStatus = "Синхронизация с облаком...";

        try
        {
            var count = await _syncRepository.SyncFromCloudAsync();
            if (count >= 0)
            {
                SyncStatus = $"Синхронизировано: {count} задач из облака";
                await LoadTasksAsync();
            }
            else
            {
                SyncStatus = "Облако недоступно. Используются локальные данные.";
            }
        }
        catch (Exception ex)
        {
            SyncStatus = $"Ошибка синхронизации: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        var filtered = _allTasks.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(t =>
                t.Title.ToLower().Contains(search) ||
                t.Description.ToLower().Contains(search));
        }

        filtered = FilterStatus switch
        {
            "Активные" => filtered.Where(t => !t.IsCompleted),
            "Завершённые" => filtered.Where(t => t.IsCompleted),
            _ => filtered
        };

        Tasks = new ObservableCollection<TaskItem>(filtered);
    }

    private async Task OnTaskSelected(TaskItem? task)
    {
        if (task == null) return;

        var parameters = new Dictionary<string, object>
        {
            { "TaskItem", task },
            { "IsNew", false }
        };

        await Shell.Current.GoToAsync(nameof(TaskDetailPage), parameters);
        SelectedTask = null;
    }

    private async Task OnAddTask()
    {
        var newTask = new TaskItem
        {
            Title = "Новая задача",
            Description = "Описание новой задачи",
            DueDate = DateTime.Now.AddDays(7),
            Priority = "Средний"
        };

        var parameters = new Dictionary<string, object>
        {
            { "TaskItem", newTask },
            { "IsNew", true }
        };

        await Shell.Current.GoToAsync(nameof(TaskDetailPage), parameters);
    }

    private async Task OnExportCsv()
    {
        try
        {
            var tasks = await _syncRepository.GetAllTasksAsync();
            var path = await _csvService.ExportTasksAsync(tasks);
            await Shell.Current.DisplayAlert("Экспорт завершён",
                $"Файл сохранён:\n{path}\n\nЗадач: {tasks.Count}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Экспорт: {ex.Message}", "OK");
        }
    }

    private async Task OnImportCsv()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите CSV файл",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".csv" } },
                    { DevicePlatform.Android, new[] { "text/csv" } },
                    { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                })
            });

            if (result == null) return;

            var tasks = await _csvService.ImportTasksAsync(result.FullPath);
            int imported = 0;
            foreach (var task in tasks)
            {
                await _syncRepository.SaveTaskAsync(task);
                imported++;
            }

            await LoadTasksAsync();
            await Shell.Current.DisplayAlert("Импорт завершён", $"Импортировано: {imported}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Импорт: {ex.Message}", "OK");
        }
    }
}

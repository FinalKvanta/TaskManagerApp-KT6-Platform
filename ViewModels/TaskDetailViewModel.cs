using System.Windows.Input;
using TaskManagerApp.Data;
using TaskManagerApp.Models;
using TaskManagerApp.Services;

namespace TaskManagerApp.ViewModels;

[QueryProperty(nameof(TaskItem), "TaskItem")]
[QueryProperty(nameof(IsNew), "IsNew")]
public class TaskDetailViewModel : BaseViewModel
{
    private readonly SyncTaskRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly ICameraService _cameraService;
    private readonly ILocationService _locationService;

    private TaskItem _taskItem = new();
    private bool _isNew;
    private bool _isEditing;
    private string _editTitle = string.Empty;
    private string _editDescription = string.Empty;
    private string _editPriority = string.Empty;
    private DateTime _editDueDate;
    private ImageSource? _photoSource;

    public TaskItem TaskItem
    {
        get => _taskItem;
        set
        {
            SetProperty(ref _taskItem, value);
            EditTitle = value.Title;
            EditDescription = value.Description;
            EditPriority = value.Priority;
            EditDueDate = value.DueDate;
            if (!string.IsNullOrEmpty(value.PhotoPath) && File.Exists(value.PhotoPath))
                PhotoSource = ImageSource.FromFile(value.PhotoPath);
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(PriorityColor));
            OnPropertyChanged(nameof(HasPhoto));
            OnPropertyChanged(nameof(HasLocation));
            OnPropertyChanged(nameof(LocationText));
        }
    }

    public bool IsNew { get => _isNew; set { SetProperty(ref _isNew, value); if (value) IsEditing = true; } }
    public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }
    public string EditTitle { get => _editTitle; set => SetProperty(ref _editTitle, value); }
    public string EditDescription { get => _editDescription; set => SetProperty(ref _editDescription, value); }
    public string EditPriority { get => _editPriority; set => SetProperty(ref _editPriority, value); }
    public DateTime EditDueDate { get => _editDueDate; set => SetProperty(ref _editDueDate, value); }

    public ImageSource? PhotoSource
    {
        get => _photoSource;
        set => SetProperty(ref _photoSource, value);
    }

    public bool HasPhoto => !string.IsNullOrEmpty(TaskItem.PhotoPath);
    public bool HasLocation => TaskItem.Latitude.HasValue && TaskItem.Longitude.HasValue;
    public string LocationText => HasLocation
        ? $"📍 {TaskItem.LocationAddress ?? $"{TaskItem.Latitude:F4}, {TaskItem.Longitude:F4}"}"
        : "Местоположение не указано";

    public string StatusText => TaskItem.IsCompleted ? "Завершена" : "В работе";
    public Color StatusColor => TaskItem.IsCompleted ? Color.FromArgb("#4CAF50") : Color.FromArgb("#FF9800");
    public Color PriorityColor => TaskItem.Priority switch
    {
        "Высокий" => Color.FromArgb("#F44336"),
        "Средний" => Color.FromArgb("#FF9800"),
        "Низкий" => Color.FromArgb("#4CAF50"),
        _ => Color.FromArgb("#9E9E9E")
    };

    public ICommand ToggleStatusCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand SetHighPriorityCommand { get; }
    public ICommand SetMediumPriorityCommand { get; }
    public ICommand SetLowPriorityCommand { get; }
    // Платформенные команды
    public ICommand TakePhotoCommand { get; }
    public ICommand PickPhotoCommand { get; }
    public ICommand GetLocationCommand { get; }
    public ICommand SetReminderCommand { get; }

    public TaskDetailViewModel(
        SyncTaskRepository repository,
        INotificationService notificationService,
        ICameraService cameraService,
        ILocationService locationService)
    {
        _repository = repository;
        _notificationService = notificationService;
        _cameraService = cameraService;
        _locationService = locationService;

        ToggleStatusCommand = new Command(async () => await OnToggleStatus());
        EditCommand = new Command(() => IsEditing = true);
        SaveCommand = new Command(async () => await OnSave());
        CancelEditCommand = new Command(OnCancelEdit);
        DeleteCommand = new Command(async () => await OnDelete());
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        SetHighPriorityCommand = new Command(() => EditPriority = "Высокий");
        SetMediumPriorityCommand = new Command(() => EditPriority = "Средний");
        SetLowPriorityCommand = new Command(() => EditPriority = "Низкий");

        TakePhotoCommand = new Command(async () => await OnTakePhoto());
        PickPhotoCommand = new Command(async () => await OnPickPhoto());
        GetLocationCommand = new Command(async () => await OnGetLocation());
        SetReminderCommand = new Command(async () => await OnSetReminder());
    }

    private async Task OnTakePhoto()
    {
        var path = await _cameraService.TakePhotoAsync();
        if (path != null)
        {
            TaskItem.PhotoPath = path;
            PhotoSource = ImageSource.FromFile(path);
            await _repository.SaveTaskAsync(TaskItem);
            OnPropertyChanged(nameof(HasPhoto));
        }
    }

    private async Task OnPickPhoto()
    {
        var path = await _cameraService.PickPhotoAsync();
        if (path != null)
        {
            TaskItem.PhotoPath = path;
            PhotoSource = ImageSource.FromFile(path);
            await _repository.SaveTaskAsync(TaskItem);
            OnPropertyChanged(nameof(HasPhoto));
        }
    }

    private async Task OnGetLocation()
    {
        var location = await _locationService.GetCurrentLocationAsync();
        if (location != null)
        {
            TaskItem.Latitude = location.Latitude;
            TaskItem.Longitude = location.Longitude;
            TaskItem.LocationAddress = location.Address;
            await _repository.SaveTaskAsync(TaskItem);
            OnPropertyChanged(nameof(HasLocation));
            OnPropertyChanged(nameof(LocationText));
        }
    }

    private async Task OnSetReminder()
    {
        await _notificationService.ScheduleNotificationAsync(
            "Напоминание о задаче", TaskItem.Title, TaskItem.DueDate);
    }

    private async Task OnToggleStatus()
    {
        TaskItem.IsCompleted = !TaskItem.IsCompleted;
        await _repository.SaveTaskAsync(TaskItem);
        OnPropertyChanged(nameof(TaskItem));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusColor));

        if (TaskItem.IsCompleted)
            await _notificationService.ShowNotificationAsync("Задача завершена", $"✅ {TaskItem.Title}");
    }

    private async Task OnSave()
    {
        TaskItem.Title = EditTitle;
        TaskItem.Description = EditDescription;
        TaskItem.Priority = EditPriority;
        TaskItem.DueDate = EditDueDate;

        try
        {
            await _repository.SaveTaskAsync(TaskItem);
            OnPropertyChanged(nameof(TaskItem));
            OnPropertyChanged(nameof(PriorityColor));
            IsEditing = false;
            if (IsNew) { IsNew = false; await Shell.Current.GoToAsync(".."); }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось сохранить: {ex.Message}", "OK");
        }
    }

    private void OnCancelEdit()
    {
        EditTitle = TaskItem.Title;
        EditDescription = TaskItem.Description;
        EditPriority = TaskItem.Priority;
        EditDueDate = TaskItem.DueDate;
        IsEditing = false;
    }

    private async Task OnDelete()
    {
        bool confirm = await Shell.Current.DisplayAlert("Удаление", "Удалить эту задачу?", "Да", "Отмена");
        if (confirm)
        {
            try { await _repository.DeleteTaskAsync(TaskItem); }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK"); }
            await Shell.Current.GoToAsync("..");
        }
    }
}

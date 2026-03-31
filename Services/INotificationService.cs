namespace TaskManagerApp.Services;

public interface INotificationService
{
    Task ShowNotificationAsync(string title, string message);
    Task ScheduleNotificationAsync(string title, string message, DateTime notifyTime);
}

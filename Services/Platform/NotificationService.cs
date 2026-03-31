namespace TaskManagerApp.Services.Platform;

/// <summary>
/// Кроссплатформенная реализация уведомлений.
/// На Windows — системный DisplayAlert (Toast недоступен без UWP).
/// На Android — через Android NotificationManager.
/// </summary>
public class NotificationService : INotificationService
{
    public async Task ShowNotificationAsync(string title, string message)
    {
        if (OperatingSystem.IsAndroid())
        {
            await ShowAndroidNotificationAsync(title, message);
        }
        else
        {
            // Windows / iOS / другие — через DisplayAlert как fallback
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert(title, message, "OK");
            }
        }
    }

    public async Task ScheduleNotificationAsync(string title, string message, DateTime notifyTime)
    {
        var delay = notifyTime - DateTime.Now;
        if (delay.TotalMilliseconds <= 0)
        {
            await ShowNotificationAsync(title, $"⏰ Просрочено: {message}");
            return;
        }

        // Показываем подтверждение что напоминание установлено
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlert("Напоминание установлено",
                $"Вы получите напоминание о задаче \"{message}\" через {FormatDelay(delay)}", "OK");
        }

        // Запускаем таймер для уведомления (в рамках сессии приложения)
        _ = Task.Run(async () =>
        {
            await Task.Delay(delay);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await ShowNotificationAsync(title, message);
            });
        });
    }

    private static string FormatDelay(TimeSpan delay)
    {
        if (delay.TotalDays >= 1) return $"{(int)delay.TotalDays} дн. {delay.Hours} ч.";
        if (delay.TotalHours >= 1) return $"{(int)delay.TotalHours} ч. {delay.Minutes} мин.";
        return $"{(int)delay.TotalMinutes} мин.";
    }

    private Task ShowAndroidNotificationAsync(string title, string message)
    {
#if ANDROID
        var context = Android.App.Application.Context;
        var channelId = "task_notifications";

        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var channel = new Android.App.NotificationChannel(channelId, "Задачи",
                Android.App.NotificationImportance.Default);
            var manager = context.GetSystemService(Android.Content.Context.NotificationService) as Android.App.NotificationManager;
            manager?.CreateNotificationChannel(channel);
        }

        var builder = new Android.App.Notification.Builder(context, channelId)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
            .SetAutoCancel(true);

        var notificationManager = context.GetSystemService(Android.Content.Context.NotificationService) as Android.App.NotificationManager;
        notificationManager?.Notify(new Random().Next(), builder.Build());
#endif
        return Task.CompletedTask;
    }
}

using Microsoft.Extensions.Logging;
using TaskManagerApp.Data;
using TaskManagerApp.Services;
using TaskManagerApp.Services.Platform;
using TaskManagerApp.ViewModels;
using TaskManagerApp.Views;

namespace TaskManagerApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // HttpClient
        builder.Services.AddSingleton<HttpClient>();

        // API сервис
        builder.Services.AddSingleton<ITaskApiService, TaskApiService>();

        // Репозитории
        builder.Services.AddSingleton<SQLiteTaskRepository>();
        builder.Services.AddSingleton<SyncTaskRepository>();

        // Платформенные сервисы
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.AddSingleton<ICameraService, CameraService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();

        // Прочие сервисы
        builder.Services.AddSingleton<CsvExportService>();

        // Страницы и ViewModel
        builder.Services.AddTransient<TaskListPage>();
        builder.Services.AddTransient<TaskListViewModel>();
        builder.Services.AddTransient<TaskDetailPage>();
        builder.Services.AddTransient<TaskDetailViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

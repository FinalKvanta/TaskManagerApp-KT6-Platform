using TaskManagerApp.Views;

namespace TaskManagerApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(TaskDetailPage), typeof(TaskDetailPage));
    }
}

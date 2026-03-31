namespace TaskManagerApp.Data;

public static class Constants
{
    public const string DatabaseFilename = "TaskManager.db3";

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}

using SQLite;
using TaskManagerApp.Models;

namespace TaskManagerApp.Data;

public class SQLiteTaskRepository : ITaskRepository
{
    private SQLiteAsyncConnection? _database;

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_database is not null)
            return _database;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        await _database.CreateTableAsync<TaskItem>();
        return _database;
    }

    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<TaskItem>().ToListAsync();
    }

    public async Task<TaskItem?> GetTaskByIdAsync(int id)
    {
        var db = await GetConnectionAsync();
        return await db.Table<TaskItem>().Where(t => t.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveTaskAsync(TaskItem task)
    {
        var db = await GetConnectionAsync();
        if (task.Id != 0)
            return await db.UpdateAsync(task);
        else
            return await db.InsertAsync(task);
    }

    public async Task<int> DeleteTaskAsync(TaskItem task)
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAsync(task);
    }

    public async Task SeedDataAsync()
    {
        var db = await GetConnectionAsync();
        var count = await db.Table<TaskItem>().CountAsync();
        if (count > 0) return;

        var tasks = new List<TaskItem>
        {
            new()
            {
                Title = "Изучить MVVM паттерн",
                Description = "Прочитать документацию по Model-View-ViewModel и реализовать пример с INotifyPropertyChanged и ICommand.",
                DueDate = new DateTime(2026, 4, 5), IsCompleted = false, Priority = "Высокий"
            },
            new()
            {
                Title = "Сделать домашнее задание",
                Description = "Решить задачи по алгоритмам: сортировка, поиск, рекурсия.",
                DueDate = new DateTime(2026, 4, 2), IsCompleted = true, Priority = "Высокий"
            },
            new()
            {
                Title = "Купить продукты",
                Description = "Молоко, хлеб, яйца, сыр, овощи для салата, куриное филе.",
                DueDate = new DateTime(2026, 3, 31), IsCompleted = false, Priority = "Средний"
            },
            new()
            {
                Title = "Подготовить презентацию",
                Description = "Создать слайды для выступления по кроссплатформенной разработке на .NET MAUI.",
                DueDate = new DateTime(2026, 4, 10), IsCompleted = false, Priority = "Высокий"
            },
            new()
            {
                Title = "Записаться к врачу",
                Description = "Позвонить в поликлинику и записаться на плановый осмотр к терапевту.",
                DueDate = new DateTime(2026, 4, 7), IsCompleted = false, Priority = "Средний"
            },
            new()
            {
                Title = "Прочитать книгу",
                Description = "Дочитать «Чистый код» Роберта Мартина — осталось 3 главы.",
                DueDate = new DateTime(2026, 4, 15), IsCompleted = false, Priority = "Низкий"
            },
            new()
            {
                Title = "Оплатить коммунальные услуги",
                Description = "Оплатить счета за электричество, воду и интернет через личный кабинет.",
                DueDate = new DateTime(2026, 4, 1), IsCompleted = true, Priority = "Средний"
            },
            new()
            {
                Title = "Тренировка в зале",
                Description = "Выполнить программу: жим лёжа, приседания, становая тяга, подтягивания.",
                DueDate = new DateTime(2026, 3, 31), IsCompleted = false, Priority = "Низкий"
            },
        };

        foreach (var task in tasks)
            await db.InsertAsync(task);
    }
}

namespace TaskManagerApp.Services.Platform;

/// <summary>
/// Сервис камеры через MAUI MediaPicker (Windows + Android + iOS).
/// </summary>
public class CameraService : ICameraService
{
    public async Task<string?> TakePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Камера не поддерживается на этом устройстве", "OK");
                return null;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Сфотографируйте задачу"
            });

            if (photo == null) return null;

            // Сохраняем в папку приложения
            var localPath = Path.Combine(FileSystem.AppDataDirectory, "Photos");
            Directory.CreateDirectory(localPath);

            var fileName = $"task_photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            var filePath = Path.Combine(localPath, fileName);

            using var stream = await photo.OpenReadAsync();
            using var fileStream = File.OpenWrite(filePath);
            await stream.CopyToAsync(fileStream);

            return filePath;
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Разрешение",
                "Необходимо разрешение на использование камеры", "OK");
            return null;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось сделать фото: {ex.Message}", "OK");
            return null;
        }
    }

    public async Task<string?> PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Выберите фото для задачи"
            });

            if (photo == null) return null;

            var localPath = Path.Combine(FileSystem.AppDataDirectory, "Photos");
            Directory.CreateDirectory(localPath);

            var fileName = $"task_photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            var filePath = Path.Combine(localPath, fileName);

            using var stream = await photo.OpenReadAsync();
            using var fileStream = File.OpenWrite(filePath);
            await stream.CopyToAsync(fileStream);

            return filePath;
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Разрешение",
                "Необходимо разрешение на доступ к галерее", "OK");
            return null;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось выбрать фото: {ex.Message}", "OK");
            return null;
        }
    }
}

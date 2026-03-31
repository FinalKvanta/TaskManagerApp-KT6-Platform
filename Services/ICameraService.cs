namespace TaskManagerApp.Services;

public interface ICameraService
{
    Task<string?> TakePhotoAsync();
    Task<string?> PickPhotoAsync();
}

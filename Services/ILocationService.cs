namespace TaskManagerApp.Services;

public class LocationResult
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
}

public interface ILocationService
{
    Task<LocationResult?> GetCurrentLocationAsync();
}

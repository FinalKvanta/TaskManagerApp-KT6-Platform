namespace TaskManagerApp.Services.Platform;

/// <summary>
/// Геолокация через MAUI Geolocation API (Windows + Android + iOS).
/// </summary>
public class LocationService : ILocationService
{
    public async Task<LocationResult?> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert("Разрешение",
                        "Необходимо разрешение на определение местоположения", "OK");
                    return null;
                }
            }

            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(15)
            });

            if (location == null) return null;

            var result = new LocationResult
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };

            // Попробуем получить адрес (обратное геокодирование)
            try
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();
                if (placemark != null)
                {
                    result.Address = $"{placemark.Locality}, {placemark.SubThoroughfare} {placemark.Thoroughfare}".Trim(' ', ',');
                    if (string.IsNullOrWhiteSpace(result.Address))
                        result.Address = $"{placemark.CountryName}, {placemark.AdminArea}";
                }
            }
            catch
            {
                result.Address = $"{location.Latitude:F4}, {location.Longitude:F4}";
            }

            return result;
        }
        catch (FeatureNotSupportedException)
        {
            await Shell.Current.DisplayAlert("Ошибка", "Геолокация не поддерживается на этом устройстве", "OK");
            return null;
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Разрешение", "Нет разрешения на геолокацию", "OK");
            return null;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось получить местоположение: {ex.Message}", "OK");
            return null;
        }
    }
}

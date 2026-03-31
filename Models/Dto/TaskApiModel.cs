using System.Text.Json.Serialization;

namespace TaskManagerApp.Models.Dto;

/// <summary>
/// DTO для JSONPlaceholder /todos API
/// </summary>
public class TaskApiModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }
}

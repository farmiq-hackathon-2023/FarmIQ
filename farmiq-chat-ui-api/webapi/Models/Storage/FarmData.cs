// Copyright (c) Microsoft. All rights reserved.
using System.Text.Json.Serialization;

namespace CopilotChat.WebApi.Models.Storage;

/// <summary>
/// Custom plugin imported from ChatGPT Manifest file.
/// Docs: https://platform.openai.com/docs/plugins/introduction.
/// </summary>
public class FarmData
{
    /// <summary>
    /// ID of the farm record
    /// </summary>
    [JsonPropertyName("ID")]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// State of the farm record
    /// </summary>
    [JsonPropertyName("State")]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// City of the farm record
    /// </summary>
    [JsonPropertyName("City")]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Location of the farm record
    /// </summary>
    [JsonPropertyName("Location")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Month of the farm record
    /// </summary>
    [JsonPropertyName("Month")]
    public string Month { get; set; } = string.Empty;

    /// <summary>
    /// Nitrogen of the farm record
    /// </summary>
    [JsonPropertyName("Nitrogen")]
    public string Nitrogen { get; set; } = string.Empty;

    /// <summary>
    /// Phosphorous of the farm record
    /// </summary>
    [JsonPropertyName("Phosphorous")]
    public string Phosphorous { get; set; } = string.Empty;

    /// <summary>
    /// Potassium of the farm record
    /// </summary>
    [JsonPropertyName("Potassium")]
    public string Potassium { get; set; } = string.Empty;

    /// <summary>
    /// Temperature of the farm record
    /// </summary>
    [JsonPropertyName("Temperature")]
    public string Temperature { get; set; } = string.Empty;

    /// <summary>
    /// Humidity of the farm record
    /// </summary>
    [JsonPropertyName("Humidity")]
    public string Humidity { get; set; } = string.Empty;

    /// <summary>
    /// Ph of the farm record
    /// </summary>
    [JsonPropertyName("Ph")]
    public string Ph { get; set; } = string.Empty;

    /// <summary>
    /// RainFall of the farm record
    /// </summary>
    [JsonPropertyName("RainFall")]
    public string RainFall { get; set; } = string.Empty;


    /// <summary>
    /// CropType of the farm record
    /// </summary>
    [JsonPropertyName("CropType")]
    public string CropType { get; set; } = string.Empty;
}

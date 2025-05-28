﻿using System.Text.Json.Serialization;

namespace HamsterStudio.BeautyLegs.RedBook.Sub
{
    public class TagModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}

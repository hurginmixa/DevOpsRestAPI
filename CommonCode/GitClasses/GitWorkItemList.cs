﻿using System;
using System.Text.Json.Serialization;

namespace CommonCode.GitClasses
{
    public class GitWorkItemList
    {
        [JsonPropertyName("count")]
        public int Count { get; set; } = 0;

        [JsonPropertyName("value")]
        public GitWorkItem[] Value { get; set; } = Array.Empty<GitWorkItem>();
    }
}
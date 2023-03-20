﻿using System.Text.Json.Serialization;

namespace Test01.GitClasses
{
    public class GitRelation
    {
        [JsonPropertyName("rel")]
        public string Rel { get; set; }
        
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("attributes")]
        public GitRelationAttributes Attributes { get; set; } = new GitRelationAttributes();
    }
}
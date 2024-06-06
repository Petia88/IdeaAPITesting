﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdeaAPITesting.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("id")]
        public string IdeaId { get; set; }
    }
}

﻿using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Bilibili.SubStruct
{
    public struct SegmentBase
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("initialization")]
        public string Initialization { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("index_range")]
        public string IndexRange { get; set; }
    }
}
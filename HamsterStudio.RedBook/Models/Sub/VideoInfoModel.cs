using HamsterStudio.Barefeet.MVVM;
using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models.Sub;

public class VideoDetailModel
{
    public class CapaModel
    {
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
    }

    public class ImageModel
    {
        [JsonPropertyName("thumbnailFileid")]
        public string ThumbnailFileid { get; set; } = string.Empty;

        [JsonPropertyName("firstFrameFileid")]
        public string FirstFrameFileid { get; set; } = string.Empty;
    }

    public class MediaModel
    {
        public class StreamModel
        {
            public class StreamDataModel
            {
                [JsonPropertyName("audioCodec")]
                public string AudioCodec { get; set; } = string.Empty;

                [JsonPropertyName("audioBitrate")]
                public long AudioBitrate { get; set; }

                [JsonPropertyName("hdrType")]
                public long HdrType { get; set; }

                [JsonPropertyName("weight")]
                public int Weight { get; set; }

                [JsonPropertyName("qualityType")]
                public string QualityType { get; set; } = string.Empty;

                [JsonPropertyName("streamType")]
                public int StreamType { get; set; }

                [JsonPropertyName("volume")]
                public int Volume { get; set; }

                [JsonPropertyName("audioChannels")]
                public int AudioChannels { get; set; }

                [JsonPropertyName("streamDesc")]
                public string StreamDesc { get; set; } = string.Empty;

                [JsonPropertyName("defaultStream")]
                public int DefaultStream { get; set; }

                [JsonPropertyName("duration")]
                public long Duration { get; set; }

                [JsonPropertyName("avgBitrate")]
                public long AvgBitrate { get; set; }

                [JsonPropertyName("videoDuration")]
                public long VideoDuration { get; set; }

                [JsonPropertyName("masterUrl")]
                public string MasterUrl { get; set; } = string.Empty;

                [JsonPropertyName("vmaf")]
                public long Vmaf { get; set; }

                [JsonPropertyName("psnr")]
                public double Psnr { get; set; }

                [JsonPropertyName("width")]
                public int Width { get; set; }

                [JsonPropertyName("height")]
                public int Height { get; set; }

                [JsonPropertyName("size")]
                public long Size { get; set; }

                [JsonPropertyName("videoCodec")]
                public string VideoCodec { get; set; } = string.Empty;

                [JsonPropertyName("audioDuration")]
                public long AudioDuration { get; set; }

                [JsonPropertyName("rotate")]
                public int Rotate { get; set; }

                [JsonPropertyName("backupUrls")]
                public string[] BackupUrls { get; set; } = [];

                [JsonPropertyName("ssim")]
                public long Ssim { get; set; }

                [JsonPropertyName("format")]
                public string Format { get; set; } = string.Empty;

                [JsonPropertyName("fps")]
                public int Fps { get; set; }

                [JsonPropertyName("videoBitrate")]
                public long VideoBitrate { get; set; }
            }

            [JsonPropertyName("EF4")] public StreamDataModel[] EncodingFormat4 { get; set; } = [];
            [JsonPropertyName("EF5")] public StreamDataModel[] EncodingFormat5 { get; set; } = [];
            [JsonPropertyName("EF6")] public StreamDataModel[] EncodingFormat6 { get; set; } = [];
            [JsonPropertyName("EF7")] public StreamDataModel[] EncodingFormat7 { get; set; } = [];

        }

        public class VideoModel
        {
            [JsonPropertyName("drmType")]
            public int DrmType { get; set; }

            [JsonPropertyName("streamTypes")]
            public int[] StreamTypes { get; set; } = [];

            [JsonPropertyName("bizName")]
            public int BizName { get; set; }

            [JsonPropertyName("bizId")]
            public string BizId { get; set; } = string.Empty;

            [JsonPropertyName("duration")]
            public int Duration { get; set; }

            [JsonPropertyName("md5")]
            public string Md5 { get; set; } = string.Empty;

            [JsonPropertyName("hdrType")]
            public int HdrType { get; set; }
        }

        [JsonPropertyName("stream")] public StreamModel Stream { get; set; } = new();

        [JsonPropertyName("videoId")] public long VideoId { get; set; }

        [JsonPropertyName("video")] public VideoModel Video { get; set; } = new();
    }

    [JsonPropertyName("media")] public MediaModel Media { get; set; } = new();

    [JsonPropertyName("image")] public ImageModel Image { get; set; } = new();

    [JsonPropertyName("capa")] public CapaModel Capa { get; set; } = new();

    [JsonPropertyName("mediaV2")] public string MediaV2 { get; set; } = string.Empty;
}

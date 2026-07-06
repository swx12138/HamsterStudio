namespace HamsterStudio.Barefeet.Constants
{
    public static class DownloadConstants
    {
        public static readonly int MaxConcurrent = Environment.ProcessorCount; // 最大并发数

        public const int DefaultChunkSize = 1024 * 1024; // 默认分块大小 1MB
        public const int MaxRetries = 5; // 最大重试次数

        public const int DefaultMaxConcurrentPackages = 2;
    }
}

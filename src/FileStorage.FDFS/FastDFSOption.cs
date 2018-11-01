namespace FileStorage.FDFS
{
    public sealed class FastDfsOption
    {
        public string[] TrackerIps { get; set; }

        public int TrackerPort { get; set; }

        public string FileUrlPrefix { get; set; }

        public string[] InsecureGroups { get; set; }
    }
}
namespace FastDFS.Client
{
    internal class NameValuePair
    {
        public NameValuePair()
        {

        }

        public NameValuePair(string name)
        {
            this.Name = name;
        }

        public NameValuePair(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
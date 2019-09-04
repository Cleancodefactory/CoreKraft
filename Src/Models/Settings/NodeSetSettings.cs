namespace Ccf.Ck.Models.Settings
{
    public class NodeSetSettings
    {
        public NodeSetSettings()
        {
            SourceLoaderMapping = new SourceLoaderMapping();
        }

        public SourceLoaderMapping SourceLoaderMapping { get; set; }
    }
}

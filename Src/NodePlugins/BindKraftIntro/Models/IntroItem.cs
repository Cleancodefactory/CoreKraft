namespace Ccf.Ck.NodePlugins.BindKraftIntro.Models
{
    public class IntroItem
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public int OrderIdx { get; set; }
        public string Author { get; set; }
        public Sources Sources { get; set; }
        public IntroItemLaunchSpec LaunchSpec { get; set; }        
    }
}

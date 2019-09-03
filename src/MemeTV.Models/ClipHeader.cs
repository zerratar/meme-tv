namespace MemeTV.Models
{
    public class ClipHeader
    {
        public string Name { get; set; }
        public string Image => "/data/clips/images/grandeclip" + Name + ".jpg";
        public int SubtitleCount { get; set; }
    }
}
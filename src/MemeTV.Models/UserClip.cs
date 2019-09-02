using System;

namespace MemeTV.Models
{
    public class UserClip
    {
        public string Name { get; set; }
        public string ClipName { get; set; }
        public string Webm => "/data/clips/webm/" + ClipName + ".webm";
        public string Mp4 => "/data/clips/mp4/" + ClipName + ".mp4";
        public float Duration { get; set; }
        public string VTT { get; set; }
        public DateTime Created { get; set; }
    }
}
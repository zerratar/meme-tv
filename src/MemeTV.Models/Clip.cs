using System;
using System.Collections.Generic;

namespace MemeTV.Models
{
    public class Clip
    {
        public string Name { get; set; }        
        public string Webm => "/data/clips/webm/" + Name + ".webm";
        public string Mp4 => "/data/clips/mp4/" + Name + ".mp4";
        public string Image => "/data/clips/images/" + Name + ".jpg";
        public string ImageBig => "/data/clips/images/grande" + Name + ".jpg";
        public float Duration { get; set; }
        public float Audiodelay { get; set; }
        public IReadOnlyList<CaptionCue> CaptionCues { get; set; }
    }
}
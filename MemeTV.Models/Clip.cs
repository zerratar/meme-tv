using System;
using System.Collections.Generic;

namespace MemeTV.Models
{
    public class Clip
    {
        public string Name { get; set; }        
        public string Webm => "/assets/data/clips/webm/" + Name + ".webm";
        public string Mp4 => "/assets/data/clips/mp4/" + Name + ".mp4";
        public float Duration { get; set; }
        public float Audiodelay { get; set; }
        public IReadOnlyList<CaptionCue> CaptionCues { get; set; }
    }
}
using System;

namespace MemeTV.Models
{
    public class CaptionCue
    {
        public string Name { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }
}
using System;

namespace MemeTV.Models
{
    public class SubtitlesSocialData
    {
        public Guid Id { get; set; }
        public string SubtitlesId { get; set; }
        public long Views { get; set; }
        public long Likes { get; set; }
    }
}
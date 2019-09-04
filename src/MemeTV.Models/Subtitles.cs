using System;
using System.Collections.Generic;

namespace MemeTV.Models
{
    public class Subtitles
    {
        public string Id { get; set; }
        public string ClipName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }        
        public IReadOnlyList<string> Captions { get; set; }
        public DateTime Created { get; set; }
    }
}

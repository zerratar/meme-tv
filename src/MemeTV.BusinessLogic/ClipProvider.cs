using System;
using System.Collections.Generic;
using System.Linq;
using MemeTV.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace MemeTV.BusinessLogic
{
    public class ClipProvider : IClipProvider
    {
        private readonly IHostingEnvironment env;
        private readonly IReadOnlyList<Clip> clips;
        private readonly IReadOnlyList<ClipHeader> headers;

        public ClipProvider(IHostingEnvironment env)
        {
            this.env = env;
            var timings = System.IO.Path.Combine(this.env.WebRootPath, "data", "subtitle-timings.json");
            var playlist = System.IO.Path.Combine(this.env.WebRootPath, "data", "playlist.json");

            clips = JsonConvert.DeserializeObject<List<Clip>>(System.IO.File.ReadAllText(timings));
            headers = JsonConvert.DeserializeObject<List<BombayTvClip>>(System.IO.File.ReadAllText(playlist)).Select(
                x => new ClipHeader
                {
                    Name = x.Clip,
                    SubtitleCount = x.Title
                }).ToList();
        }

        public Clip Get(string name)
        {
            return this.clips.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public ClipHeader[] GetHeaders()
        {
            return headers.ToArray();
        }
    }
}
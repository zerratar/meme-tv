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
        public ClipProvider(IHostingEnvironment env)
        {
            this.env = env;
            var file = System.IO.Path.Combine(this.env.WebRootPath, "data", "subtitle-timings.json");
            clips = JsonConvert.DeserializeObject<List<Clip>>(System.IO.File.ReadAllText(file));
        }

        public Clip Get(string name)
        {
            return this.clips.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
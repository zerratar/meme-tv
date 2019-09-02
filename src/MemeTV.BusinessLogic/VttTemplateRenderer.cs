using System.Collections.Generic;
using System.Linq;
using MemeTV.Models;
using Microsoft.AspNetCore.Hosting;

namespace MemeTV.BusinessLogic
{
    public class VttTemplateRenderer : IVttTemplateRenderer
    {
        private readonly IHostingEnvironment env;

        public VttTemplateRenderer(IHostingEnvironment env)
        {
            this.env = env;
        }

        public string Render(Clip clip, IEnumerable<string> captions)
        {
            var templateData = GetTemplateData(clip.Name);
            var values = captions.ToList();
            var dif = clip.CaptionCues.Count - values.Count;
            if (dif > 0) values.AddRange(Enumerable.Range(0, dif).Select(x => ""));
            return string.Format(templateData, values.ToArray());
        }

        private string GetTemplateData(string clipName)
        {
            var path = System.IO.Path.Combine(env.WebRootPath, "data", "templates", clipName + ".vtt");
            return System.IO.File.ReadAllText(path);
        }
    }
}
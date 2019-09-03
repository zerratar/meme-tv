using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MemeTV.Models;

namespace MemeTV.BusinessLogic
{
    public class ClipManager : IClipManager
    {
        private readonly IClipProvider clipProvider;
        private readonly IVttTemplateRenderer vttRenderer;

        public ClipManager(IClipProvider clipProvider, IVttTemplateRenderer vttRenderer)
        {
            this.clipProvider = clipProvider;
            this.vttRenderer = vttRenderer;
        }

        public async Task<Subtitles> GetAsync(string id)
        {
            return new Subtitles
            {
                Id = id,
                ClipName = "clip1",
                Created = DateTime.UtcNow,
                Email = "test@test.com",
                Name = "John Doe",
                Captions = new List<string> { "Hello, world!" }
            };
        }

        public Task StoreAsync(string modelName, string modelEmail, string modelClip, string[] modelSubtitles)
        {
            return Task.CompletedTask;
        }

        public async Task<UserClip> GetClipSubtitleAsync(string id)
        {
            var subtitles = await GetAsync(id);
            var clip = clipProvider.Get(subtitles.ClipName);
            return new UserClip
            {
                //Email = subtitles.Email,
                Name = subtitles.Name,
                Created = subtitles.Created,
                VTT = "/api/subtitles/vtt/" + id,
                ClipName = clip.Name
            };
        }

        public async Task<string> GetClipVttAsync(string id)
        {
            var subtitles = await GetAsync(id);
            var clip = clipProvider.Get(subtitles.ClipName);
            return vttRenderer.Render(clip, subtitles.Captions);
        }

        public string GetEmptyVtt()
        {
            var sb = new StringBuilder();
            sb.AppendLine("WEBVTT FILE");
            sb.AppendLine();
            return sb.ToString();
        }

        public async Task<ClipHeader[]> GetHeadersAsync()
        {
            return clipProvider.GetHeaders();
        }
    }
}
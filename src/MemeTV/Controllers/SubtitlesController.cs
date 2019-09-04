using System.Text;
using System.Threading.Tasks;
using MemeTV.BusinessLogic;
using MemeTV.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MemeTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubtitlesController : ControllerBase
    {
        private readonly ILogger logger;
        private readonly IClipManager clipManager;

        public SubtitlesController(
            ILogger logger,
            IClipManager clipManager)
        {
            this.logger = logger;
            this.clipManager = clipManager;
        }

        [HttpGet("like/{id}")]
        public async Task<object> LikeAsync(string id)
        {
            // since we don't have a login
            // we cant stop people from spamming like
            var liked = HttpContext.Session.GetInt32("like_" + id) == 1;
            try
            {
                var newValue = !liked;
                var likeCount = await clipManager.UpdateLikesAsync(id, newValue);
                HttpContext.Session.SetInt32("like_" + id, newValue ? 1 : 0);
                return new { liked = newValue, likes = likeCount };
            }
            finally
            {
                await HttpContext.Session.CommitAsync();
            }
        }

        [HttpGet("report/{clip}")]
        public Task ReportBadClipAsync(string clip)
        {
            return clipManager.ReportBadCaptionsAsync(clip);
        }

        [HttpGet("clips")]
        public async Task<ClipHeader[]> GetClips()
        {
            return await clipManager.GetHeadersAsync();
        }

        [HttpPost]
        public async Task<object> SaveSubtitlesAsync(SaveSubtitleModel model)
        {
            return new { id = await clipManager.StoreAsync(model.Name, model.Email, model.Title, model.Description, model.Clip, model.Subtitles) };
        }

        [HttpGet("{id}")]
        public async Task<UserClip> GetSubtitlesAsync(string id)
        {
            var viewed = HttpContext.Session.GetInt32("view_" + id) == 1;
            if (!viewed)
            {
                HttpContext.Session.SetInt32("view_" + id, 1);
                await HttpContext.Session.CommitAsync();
            }

            var clip = await clipManager.GetClipSubtitleAsync(id, !viewed);
            var liked = HttpContext.Session.GetInt32("like_" + id) == 1;
            clip.Liked = liked;
            return clip;
        }

        [HttpGet("vtt/empty/{clip}")]
        public async Task<IActionResult> GetEmptyVtt(string clip)
        {
            var vtt = await clipManager.GetEmptyVttAsync(clip);
            var bytes = Encoding.UTF8.GetBytes(vtt);
            return File(bytes, "text/vtt");
        }

        [HttpGet("vtt/{id}")]
        public async Task<IActionResult> GetVtt(string id)
        {
            var vtt = await clipManager.GetClipVttAsync(id);
            var bytes = Encoding.UTF8.GetBytes(vtt);
            return File(bytes, "text/vtt");
        }
    }

    public class SaveSubtitleModel
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("clip")] public string Clip { get; set; }
        [JsonProperty("subtitles")] public string[] Subtitles { get; set; }
    }
}

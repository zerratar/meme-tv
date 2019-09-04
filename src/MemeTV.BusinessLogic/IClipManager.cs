using System.Threading.Tasks;
using MemeTV.Models;

namespace MemeTV.BusinessLogic
{
    public interface IClipManager
    {
        Task<Subtitles> GetAsync(string id);
        Task<string> StoreAsync(string name, string email, string title, string description, string clip, string[] modelSubtitles);
        Task<UserClip> GetClipSubtitleAsync(string id, bool updateViewCount);
        Task<string> GetClipVttAsync(string id);
        Task<string> GetEmptyVttAsync(string id);
        Task<ClipHeader[]> GetHeadersAsync();
        Task ReportBadCaptionsAsync(string clip);
        Task<long> UpdateLikesAsync(string id, bool liked);
    }
}
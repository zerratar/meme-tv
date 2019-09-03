using System.Threading.Tasks;
using MemeTV.Models;

namespace MemeTV.BusinessLogic
{
    public interface IClipManager
    {
        Task<Subtitles> GetAsync(string id);
        Task StoreAsync(string modelName, string modelEmail, string modelClip, string[] modelSubtitles);
        Task<UserClip> GetClipSubtitleAsync(string id);
        Task<string> GetClipVttAsync(string id);
        string GetEmptyVtt();
        Task<ClipHeader[]> GetHeadersAsync();
    }
}
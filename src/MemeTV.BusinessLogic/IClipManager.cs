using System.Threading.Tasks;
using MemeTV.Models;

namespace MemeTV.BusinessLogic
{
    public interface IClipManager
    {
        Task<Subtitles> GetAsync(string id);
        Task<string> StoreAsync(string modelName, string modelEmail, string modelClip, string[] modelSubtitles);
        Task<UserClip> GetClipSubtitleAsync(string id);
        Task<string> GetClipVttAsync(string id);
        Task<string> GetEmptyVttAsync(string id);
        Task<ClipHeader[]> GetHeadersAsync();
    }
}
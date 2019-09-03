using MemeTV.Models;

namespace MemeTV.BusinessLogic
{
    public interface IClipProvider
    {
        Clip Get(string name);
        ClipHeader[] GetHeaders();
    }
}
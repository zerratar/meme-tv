using System.Collections.Generic;
using MemeTV.Models;

namespace MemeTV.BusinessLogic
{
    public interface IVttTemplateRenderer
    {
        string Render(Clip clip, IEnumerable<string> captions);
    }
}
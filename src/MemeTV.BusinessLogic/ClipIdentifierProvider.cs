using System;

namespace MemeTV.BusinessLogic
{
    public class ClipIdentifierProvider : IClipIdentifierProvider
    {
        public bool IsValid(string id)
        {
            if (long.TryParse(id, out _)) return true;
            return Guid.TryParse(id, out _);
        }

        public string Get()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
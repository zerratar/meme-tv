namespace MemeTV.BusinessLogic
{
    public interface IClipIdentifierProvider
    {
        bool IsValid(string id);
        string Get();
    }
}
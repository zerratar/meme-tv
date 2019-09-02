namespace MemeTV.BusinessLogic
{
    public interface ILogger
    {
        void Debug(string msg);
        void Error(string msg);
        void Message(string msg);
        void Warning(string msg);
    }
}

using System.Data.Common;

namespace MemeTV.BusinessLogic
{
    public interface ISqlConnectionProvider
    {
        DbConnection Get();
    }
}
namespace MemeTV.BusinessLogic
{
    public interface IDbConnectionStringProvider
    {
        string Get();
    }

    public class DbConnectionStringProvider : IDbConnectionStringProvider
    {
        private AppSettings setting;

        public DbConnectionStringProvider(AppSettings settings)
        {
            this.setting = settings;
        }
        public string Get()
        {
            return setting.DbConnectionString;
        }
    }
}
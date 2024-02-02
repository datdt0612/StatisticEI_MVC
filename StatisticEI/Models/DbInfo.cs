namespace StatisticEI.Models
{
    public class DbInfo
    {
        public string Id { get; set; } = null!;
        public string IP { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;

        public DbInfo(string connectionString)
        {
            var info = connectionString.Split(";");
            foreach (var item in info)
            {
                if (item.Contains("Data Source"))
                {
                    IP = item.Split("=")[1].Replace(",1433", "");
                }
                else if (item.Contains("initial catalog"))
                {
                    Name = item.Split("=")[1];
                }
            }
            Id = IP + "_" + Name;
            ConnectionString = connectionString;
        }
    }
}

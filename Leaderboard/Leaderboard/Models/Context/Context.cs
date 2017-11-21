using System.Data.Entity;

namespace Leaderboard.Models.Context
{
    public class Context : DbContext
    {
        public Context() : base("StrConn")
        {

        }

        public System.Data.Entity.DbSet<Leaderboard.Models.Player> Players { get; set; }
    }
}
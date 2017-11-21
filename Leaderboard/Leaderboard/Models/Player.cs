using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Leaderboard.Models
{
    public class Player
    {
        public int PlayerID { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
    }
}
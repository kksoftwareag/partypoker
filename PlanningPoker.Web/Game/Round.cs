using System;
using System.Collections.Generic;

namespace PlanningPoker.Web.Game
{
    public class Round
    {
        public Dictionary<Player, string> Cards { get; set; } = new Dictionary<Player, string>();

        public bool IsRevealed { get; set; } = false;
    }
}
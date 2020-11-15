using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlanningPoker.Web
{
    public class UserState
    {
        public event EventHandler<EventArgs> GamesChangedEvent;

        public List<string> RecentGames { get; } = new List<string>();
        public string CurrentGame { get; set; }

        internal void NavigateToGameHash(string hash)
        {
            this.CurrentGame = hash;

            if (this.RecentGames.Contains(hash) == true)
            {
                this.RecentGames.Remove(hash);
            }

            this.RecentGames.Add(hash);

            this.GamesChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}

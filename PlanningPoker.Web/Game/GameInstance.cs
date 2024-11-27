using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PlanningPoker.Web.Game
{
    public class GameInstance
    {
        public event EventHandler Changed;
        public event EventHandler<PlaySoundEventArgs> PlaySound;

        private static readonly ConcurrentDictionary<string, GameInstance> instances = new ConcurrentDictionary<string, GameInstance>();

        public static List<string[]> Decks { get; } = new List<string[]>()
        {
            new []{ "❓", "0", "0.5", "1", "2", "3", "5", "8", "13", "20", "40", "100", "☕" }
        };

        public string[] CardDeck => GameInstance.Decks.Single();

        public string Hash { get; private set; }

        public List<Player> Players { get; } = new List<Player>();

        public List<Round> Rounds { get; } = new List<Round>() { new Round() };

        public Round CurrentRound => this.Rounds.LastOrDefault();

        public int RoundNumber => this.Rounds.Count;

        public void NewRound()
        {
            this.Rounds.Add(new Round());
            this.RaiseChanged();
        }

        internal void Join(Player player)
        {
            if (this.Players.Any(x => x.Secret == player.Secret) == false)
            {
                this.Players.Add(player);
            }

            this.RaiseChanged();
        }

        private void RaiseChanged()
        {
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        internal static GameInstance Find(string hash)
        {
            if (GameInstance.instances.TryGetValue(hash, out var instance) == false)
            {
                instance = new GameInstance()
                {
                    Hash = hash
                };

                GameInstance.instances[hash] = instance;
            }

            return instance;
        }

        internal void ChangePlayersName(Player player, string playerName)
        {
            player.Name = playerName;
            this.RaiseChanged();
        }

        internal void Vote(Player player, string card)
        {
            if (this.Players.Contains(player) == false)
            {
                this.Players.Add(player);
            }

            this.CurrentRound.Cards[player] = card;
            this.RaiseChanged();
            this.RaisePlaySound("place-card");
        }

        internal void StartNewRound()
        {
            this.Rounds.Add(new Round());
            this.RaiseChanged();
        }

        internal void RevealCards()
        {
            this.CurrentRound.IsRevealed = true;
            this.RaiseChanged();
            this.RaisePlaySound("turn-cards");
        }

        internal void RemovePlayer(Player player)
        {
            this.Players.Remove(player);

            if (this.Players.Any() == false)
            {
                GameInstance.instances.Remove(this.Hash, out var _);
            }

            this.RaiseChanged();
        }

        internal void PlayBingSound()
        {
            this.RaisePlaySound("bing-sound");
        }

        private void RaisePlaySound(string sound)
        {
            this.PlaySound?.Invoke(this, new PlaySoundEventArgs(sound));
        }
    }
}

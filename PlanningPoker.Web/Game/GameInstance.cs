using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlanningPoker.Web.Game
{
    public class GameInstance
    {
        public event EventHandler Changed;
        public event EventHandler<PlaySoundEventArgs> PlaySound;

        private static readonly ConcurrentDictionary<string, GameInstance> instances = new();

        public static List<string[]> Decks { get; } =
        [
            ["❓", "0", "0.5", "1", "2", "3", "5", "8", "13", "20", "40", "100", "☕"]
        ];

        public List<int> RevealCountDownInSeconds { get; } =
        [
            0, 10, 30, 60, 120, 180,
        ];

        public string[] CardDeck => GameInstance.Decks.Single();

        public string Hash { get; private set; }

        public List<Player> Players { get; } = [];

        public List<Player> Spectators { get; } = [];

        public List<Round> Rounds { get; } = [new()];

        public Round CurrentRound => this.Rounds.LastOrDefault();

        public int RoundNumber => this.Rounds.Count;

        public int SelectedCountDown { get; set; }

        public int TimerSeconds { get; set; }

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
                return;
            }

            this.CurrentRound.Cards[player] = card;
            this.RaiseChanged();
            this.RaisePlaySound("place-card");
        }

        internal void StartNewRound()
        {
            this.Rounds.Add(new Round());
            this.RaiseChanged();
            this.RaisePlaySound("shuffle-cards");
        }

        internal async Task RevealCardsWithCountdown(CancellationToken cancellationToken)
        {
            this.CurrentRound.IsRevealing = true;

            bool soundWasPlayed = false;
            this.CurrentRound.IsRevealing = true;
            var endTime = DateTime.Now.AddSeconds(this.SelectedCountDown);

            try
            {
                while (!cancellationToken.IsCancellationRequested && DateTime.Now < endTime)
                {
                    var timeDiff = endTime - DateTime.Now;

                    TimerSeconds = (int)Math.Round(timeDiff.TotalSeconds);
                    this.RaiseChanged();

                    if ((SelectedCountDown - timeDiff.TotalSeconds) <= 10)
                    {
                        if (!soundWasPlayed)
                        {
                            soundWasPlayed = true;
                            this.RaisePlaySound("stopwatch_1_sec");
                        }
                    }

                    await Task.Delay(500, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Reveal countdown was canceled");
            }
            finally
            {
                this.CurrentRound.IsRevealing = false;
            }


            if (!cancellationToken.IsCancellationRequested)
            {
                this.RevealCards();
            }
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

            if (this.Players.Count != 0)
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

        internal void SwitchPlayStatus(Player player)
        {
            player.IsPlaying = !player.IsPlaying;

            if (!player.IsPlaying)
            {
                this.Players.Remove(player);
                this.Spectators.Add(player);
                this.CurrentRound.Cards.Remove(player);
            }
            else
            {
                if (!this.Players.Contains(player))
                {
                    this.Players.Add(player);
                    this.Spectators.Remove(player);
                }
            }

            this.RaiseChanged();
        }
    }
}

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using PlanningPoker.Web.Game;
using PlanningPoker.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlanningPoker.Web.ViewModels
{
    public class GameViewModel : ComponentBase, IDisposable
    {
        [Parameter]
        public string Hash { get; set; }

        [Inject]
        public ILocalStorageService LocalStorage { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Inject]
        public UserState UserState { get; set; }

        [Inject]
        public ToastService Toasts { get; set; }

        public string PlayerName { get; protected set; }

        public Player Player { get; private set; }

        public GameInstance Game { get; private set; }

        private CancellationTokenSource cancellationTokenSource;

        private HashSet<Player> knownPlayers;

        private string browserOrigin;

        public string CardChoosenInCurrentRound
        {
            get
            {
                var cards = this.Game?.CurrentRound?.Cards;
                if (cards is not null && this.Player is not null)
                {
                    if (cards.ContainsKey(this.Player) == true)
                    {
                        return this.Game.CurrentRound.Cards[this.Player];
                    }
                }

                return null;
            }
        }

        // ---------- Derived UI state ----------

        public Round CurrentRound => this.Game?.CurrentRound;

        public int RoundNumber => this.Game?.RoundNumber ?? 0;

        public bool IsRevealed => this.CurrentRound?.IsRevealed == true;

        public bool IsRevealing => this.CurrentRound?.IsRevealing == true;

        public bool IsPlaying => this.Player?.IsPlaying == true;

        public int ActiveVoterCount => this.Game?.Players.Count(p => p.IsPlaying) ?? 0;

        public int VoteCount => this.ActiveVoterCount == 0 ? 0 : this.Game.Players.Count(p => p.IsPlaying && this.CurrentRound.Cards.ContainsKey(p));

        public bool IsAllVoted => this.ActiveVoterCount > 0 && this.VoteCount == this.ActiveVoterCount;

        public int VotePercent => this.ActiveVoterCount == 0 ? 0 : (int)Math.Round(100d * this.VoteCount / this.ActiveVoterCount);

        public int CountdownDisplay => this.IsRevealing ? this.Game.TimerSeconds : this.Game.SelectedCountDown;

        public bool HasVotedInRound => this.Player is not null && this.CurrentRound?.Cards.ContainsKey(this.Player) == true;

        public bool CanReveal => this.IsRevealed == false && this.VoteCount > 0;

        public string RevealLabel
        {
            get
            {
                if (this.IsRevealing)
                {
                    return "Jetzt aufdecken";
                }

                return this.IsAllVoted ? "Alle haben gewählt. Aufdecken" : "Karten aufdecken";
            }
        }

        public IReadOnlyList<CardCount> CardCounts
        {
            get
            {
                if (this.CurrentRound is null)
                {
                    return [];
                }

                return [.. this.CurrentRound.Cards
                    .GroupBy(c => c.Value)
                    .Select(g => new CardCount(
                        g.Key,
                        g.Count(),
                        g.Select(x => x.Key.Name).ToList(),
                        double.TryParse(g.Key, out var value) ? value : (double?)null))
                    .OrderByDescending(c => c.Count)
                    .ThenBy(c => c.NumericValue ?? double.MaxValue)];
            }
        }

        public int MaxCardCount => this.CardCounts.Count == 0 ? 1 : this.CardCounts.Max(c => c.Count);

        public IReadOnlyList<RoundResult> History
        {
            get
            {
                if (this.Game is null)
                {
                    return [];
                }

                var result = new List<RoundResult>();
                for (var i = 0; i < this.Game.Rounds.Count; i++)
                {
                    var round = this.Game.Rounds[i];
                    result.Add(new RoundResult(i + 1, round.IsRevealed, round.Cards.Count, CommonCard(round), Average(round)));
                }

                return result;
            }
        }

        public string ShareUrl
        {
            get
            {
                return this.browserOrigin is null
                    ? $"/{this.Hash}"
                    : $"{this.browserOrigin.TrimEnd('/')}/{this.Hash}";
            }
        }

        // ---------- Lifecycle ----------

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            this.Game = GameInstance.Find(this.Hash);
            this.Game.Changed += this.Game_Changed;
            this.Game.PlaySound += this.Game_PlaySound;

            this.UserState.NavigateToGameHash(this.Hash);

            Player player = null;
            if (await this.LocalStorage.ContainKeyAsync(nameof(Player)) == true)
            {
                player = await this.LocalStorage.GetItemAsync<Player>(nameof(Player));

                if (player.Secret != Guid.Empty)
                {
                    this.Player = player;
                }
            }

            if (this.Player is null)
            {
                player = new Player()
                {
                    Name = String.Join(String.Empty, Enumerable.Range(0, 3).Select(x => EmojiRandomizer.GetRandomEmoji())),
                    Secret = Guid.NewGuid()
                };

                await this.LocalStorage.SetItemAsync(nameof(Player), player);
            }

            this.Player = player;
            this.PlayerName = player.Name;
            this.Player.PlaySound += this.Game_PlaySound;

            this.Game.Join(this.Player);

            await this.LoadNameAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender && this.browserOrigin is null)
            {
                try
                {
                    this.browserOrigin = await this.JsRuntime.InvokeAsync<string>("Kk.origin");
                    this.StateHasChanged();
                }
                catch { }
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            // Bubble room events to the current player only, so nobody sees their own "joined" toast.
            if (this.Game is not null)
            {
                var current = this.Game.Players.Append(this.Player).ToHashSet();
                if (this.knownPlayers is not null)
                {
                    foreach (var newcomer in current.Except(this.knownPlayers).Where(p => p != this.Player))
                    {
                        this.Toasts.Show("Beigetreten:", newcomer.Name);
                    }
                }

                this.knownPlayers = current;
            }
        }

        // ---------- Sounds / key handling ----------

        private void Game_PlaySound(object sender, PlaySoundEventArgs e)
        {
            _ = this.InvokeAsync(async () =>
            {
                try
                {
                    await this.JsRuntime.InvokeVoidAsync("MediaPlayer.PlayAudio", e.SoundName);
                }
                catch { }
            });
        }

        protected void PlayBingSound()
        {
            this.Game.PlayBingSound();
            this.Toasts.Show("Signal gesendet:", "alle Teilnehmer");
        }

        protected void PlayBingSoundForPlayer(Player player)
        {
            player.NotifyPlayer();

            if (player != this.Player)
            {
                this.Player.NotifyPlayer();
            }

            this.Toasts.Show("Signal gesendet:", player.Name);
        }

        protected void OnKeyDown(KeyboardEventArgs eventArgs)
        {
            if (this.Player is not null && this.Player.IsPlaying && this.Game?.CurrentRound?.IsRevealed == false && this.Game.CardDeck.Contains(eventArgs.Key) == true)
            {
                this.VoteCard(eventArgs.Key);
            }
        }

        // ---------- Actions ----------

        protected void VoteCard(string card)
        {
            this.Game.Vote(this.Player, card);
        }

        protected void StartNewRound()
        {
            this.Game.StartNewRound();
        }

        protected void SelectCountdown(int seconds)
        {
            this.Game.SelectedCountDown = seconds;
            this.StateHasChanged();
        }

        protected void RevealCards()
            => this.Game.RevealCards();

        protected void RevealCardsNow()
        {
            this.RevealCardsOrNow();
        }

        protected void RevealCardsOrNow()
        {
            if (this.IsRevealing)
            {
                this.CancelCardsWithCountdown();
                this.Game.RevealCards();
            }
            else
            {
                this.Game.RevealCards();
            }
        }

        protected async Task RevealCardsWithCountdown()
        {
            cancellationTokenSource = new CancellationTokenSource();
            await this.Game.RevealCardsWithCountdown(cancellationTokenSource.Token);
            cancellationTokenSource.Dispose();
        }

        protected void CancelCardsWithCountdown()
            => cancellationTokenSource?.Cancel();

        protected void SwitchPlayStatus()
        {
            this.Game.SwitchPlayStatus(this.Player);
        }

        protected void SwitchToSpectator() => this.Game.SwitchPlayStatus(this.Player);

        /// <summary>
        /// Restores the name this identity chose earlier (kept under its own key, scoped to the
        /// player's secret so two tabs in one browser don't overwrite each other). Emoji names
        /// are only a fallback.
        /// </summary>
        private async Task LoadNameAsync()
        {
            try
            {
                var key = NameKey(this.Player);
                if (await this.LocalStorage.ContainKeyAsync(key) && this.Player is not null)
                {
                    var savedName = await this.LocalStorage.GetItemAsync<string>(key);
                    if (string.IsNullOrWhiteSpace(savedName) == false)
                    {
                        this.PlayerName = savedName;
                        this.Game.ChangePlayersName(this.Player, savedName);
                    }
                }
            }
            catch { }
        }

        protected async Task NameChanged(ChangeEventArgs e)
        {
            this.PlayerName = (string)e.Value;
            this.Game.ChangePlayersName(this.Player, this.PlayerName);
            await this.LocalStorage.SetItemAsync(nameof(this.Player), this.Player);
            await this.LocalStorage.SetItemAsync(NameKey(this.Player), this.PlayerName);
        }

        protected async Task CopyInviteLink()
        {
            try
            {
                await this.JsRuntime.InvokeVoidAsync("Kk.copyText", this.ShareUrl);
                this.Toasts.Show("Link kopiert", "jetzt im Team teilen");
            }
            catch
            {
                this.Toasts.Show("Kopieren nicht möglich", "bitte die URL aus der Adresszeile übernehmen");
            }

            // The origin is only known in the browser; refresh so ShareUrl/Telegram use the absolute link.
            if (this.browserOrigin is null)
            {
                try
                {
                    this.browserOrigin = await this.JsRuntime.InvokeAsync<string>("Kk.origin");
                    this.StateHasChanged();
                }
                catch { }
            }
        }

        private void Game_Changed(object sender, EventArgs e)
        {
            _ = this.InvokeAsync(() => this.StateHasChanged());
        }

        public void Dispose()
        {
            this.Game.Changed -= this.Game_Changed;
            this.Game.PlaySound -= this.Game_PlaySound;
            this.Player.PlaySound -= this.Game_PlaySound;
            this.Game.RemovePlayer(this.Player);
        }

        // ---------- Helpers ----------

        private static string NameKey(Player player) => $"PlayerName:{player.Secret}";

        private static string CommonCard(Round round)
        {
            if (round.Cards.Count == 0)
            {
                return null;
            }

            return round.Cards
                .GroupBy(c => c.Value)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => double.TryParse(g.Key, out var v) ? v : double.MaxValue)
                .First()
                .Key;
        }

        private static double? Average(Round round)
        {
            var values = round.Cards.Values
                .Select(v => double.TryParse(v, out var parsed) ? parsed : (double?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();

            return values.Count == 0 ? null : Math.Round(values.Average(), 1);
        }

        public double? AverageCard => this.CurrentRound is null ? null : Average(this.CurrentRound);

        public bool IsConsensus => this.CardCounts.Count == 1;

        public bool HasSpread
        {
            get
            {
                if (this.IsRevealed == false || this.CardCounts.Count < 2)
                {
                    return false;
                }

                var numeric = this.CardCounts.Where(c => c.NumericValue.HasValue).Select(c => c.NumericValue!.Value).ToList();
                return numeric.Count >= 2 && numeric.Max() - numeric.Min() > numeric.Max() * 0.5;
            }
        }

        public string HighestNames => this.CurrentRound is null ? string.Empty : string.Join(", ", this.CurrentRound.HighestCardsWithPlayer.Select(c => c.Player.Name).Distinct());
        public string HighestValue => this.CurrentRound is null ? string.Empty : string.Join(", ", this.CurrentRound.HighestCardsWithPlayer.Select(c => c.Card).Distinct());
        public string LowestNames => this.CurrentRound is null ? string.Empty : string.Join(", ", this.CurrentRound.LowestCardsWithPlayer.Select(c => c.Player.Name).Distinct());
        public string LowestValue => this.CurrentRound is null ? string.Empty : string.Join(", ", this.CurrentRound.LowestCardsWithPlayer.Select(c => c.Card).Distinct());
        public record CardCount(string Card, int Count, IReadOnlyList<string> Players, double? NumericValue);

        public record RoundResult(int Round, bool IsRevealed, int Votes, string CommonCard, double? Average);
    }
}

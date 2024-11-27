using System;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using PlanningPoker.Web.Game;

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

        public string PlayerName { get; protected set; }

        public Player Player { get; private set; }

        public GameInstance Game { get; private set; }

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
        }

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
        }

        protected void PlayBingSoundForPlayer(Player player)
        {
            player.NotifyPlayer();

            if (player != this.Player)
            {
                this.Player.NotifyPlayer();
            }
        }

        protected void OnKeyDown(KeyboardEventArgs eventArgs)
        {
            if (this.Player is not null && this.Game?.CurrentRound?.IsRevealed == false && this.Game.CardDeck.Contains(eventArgs.Key) == true)
            {
                this.VoteCard(eventArgs.Key);
            }
        }

        protected void VoteCard(string card)
        {
            this.Game.Vote(this.Player, card);
        }

        protected void StartNewRound()
        {
            this.Game.StartNewRound();
        }

        protected void RevealCards()
        {
            this.Game.RevealCards();
        }

        protected async Task NameChanged(ChangeEventArgs e)
        {
            this.PlayerName = (string)e.Value;
            this.Game.ChangePlayersName(this.Player, this.PlayerName);
            await this.LocalStorage.SetItemAsync(nameof(this.Player), this.Player);
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
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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

        protected override void OnInitialized()
        {
            this.Game = GameInstance.Find(this.Hash);
            this.Game.Changed += this.Game_Changed;

            this.UserState.NavigateToGameHash(this.Hash);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender == false)
            {
                return;
            }

            Player player;
            if (await this.LocalStorage.ContainKeyAsync(nameof(Player)) == true)
            {
                player = await this.LocalStorage.GetItemAsync<Player>(nameof(Player));
            }
            else
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

            this.Game.Join(this.Player);

            this.StateHasChanged();
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

        protected readonly TimeSpan nameChangeTimes = TimeSpan.FromSeconds(3);
        protected DateTime lastNameChange = DateTime.MinValue;
        protected async Task NameChanged(ChangeEventArgs e)
        {
            if (this.lastNameChange.Add(this.nameChangeTimes) > DateTime.Now)
            {
                return;
            }

            this.PlayerName = (string)e.Value;
            this.Game.ChangePlayersName(this.Player, this.PlayerName);
            this.lastNameChange = DateTime.Now;
            await this.LocalStorage.SetItemAsync(nameof(this.Player), this.Player);
        }

        private void Game_Changed(object sender, EventArgs e)
        {
            _ = this.InvokeAsync(() => this.StateHasChanged());
        }

        public void Dispose()
        {
            this.Game.Changed -= this.Game_Changed;
            this.Game.RemovePlayer(this.Player);
        }
    }
}

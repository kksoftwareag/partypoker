using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using PlanningPoker.Web.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlanningPoker.Web.ViewModels
{
    public class GameViewModel : ComponentBase
    {
        [Parameter]
        public string Hash { get; set; }

        [Inject]
        public ILocalStorageService LocalStorage { get; set; }

        public string PlayerName { get; protected set; }

        public Player Player { get; private set; }
        public GameInstance Game { get; private set; }

        protected override void OnInitialized()
        //protected override async Task OnInitializedAsync()
        {
            this.Game = GameInstance.Find(this.Hash);
            this.Game.Changed += this.Game_Changed;
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
            await this.LocalStorage.SetItemAsync(nameof(Player), this.Player);
        }

        private void Game_Changed(object sender, EventArgs e)
        {
            _ = this.InvokeAsync(() => this.StateHasChanged());
        }
    }
}

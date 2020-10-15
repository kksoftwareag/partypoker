using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlanningPoker.Web.ViewModels
{
    public class IndexViewModel : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public async Task CreateGame()
        {
            this.NavigationManager.NavigateTo("Game1");
        }
    }
}

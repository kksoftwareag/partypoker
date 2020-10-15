using Microsoft.AspNetCore.Components;
using System;
using System.Linq;

namespace PlanningPoker.Web.ViewModels
{
    public class IndexViewModel : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public void CreateGame()
        {
            this.NavigationManager.NavigateTo(Guid.NewGuid().ToString().Split("-").First());
        }
    }
}

using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PlanningPoker.Web.ViewModels
{
    public class IndexViewModel : ComponentBase
    {
        private static readonly Regex RoomHashPattern = new("^[a-z0-9-]{4,64}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public JoinRequest Join { get; } = new();

        public bool HasValidRoom => this.TryGetRoomHash(out _);

        public void CreateGame()
        {
            this.NavigationManager.NavigateTo(Guid.NewGuid().ToString()[..8]);
        }

        public void JoinGame()
        {
            if (this.TryGetRoomHash(out var hash))
            {
                this.NavigationManager.NavigateTo(hash);
            }
        }

        /// <summary>
        /// Accepts either a bare room code ("a1b2c3d4") or a full link
        /// ("https://planningpoker.party/a1b2c3d4") and extracts the room hash.
        /// </summary>
        public bool TryGetRoomHash(out string hash)
        {
            hash = null;
            var input = this.Join.Room?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            if (input.Contains("://") || input.Contains('/'))
            {
                // Full link (or "planningpoker.party/abc"): take the last path segment.
                if (Uri.TryCreate(input.Contains("://") ? input : "https://" + input, UriKind.Absolute, out var uri) == false)
                {
                    return false;
                }

                hash = uri.AbsolutePath.Trim('/').Split('/').LastOrDefault();
            }
            else
            {
                hash = input;
            }

            if (string.IsNullOrEmpty(hash) || RoomHashPattern.IsMatch(hash) == false)
            {
                hash = null;
                return false;
            }

            return true;
        }

        public class JoinRequest
        {
            public string Room { get; set; }
        }
    }
}

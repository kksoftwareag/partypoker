using System.Collections.Generic;
using System.Linq;

namespace PlanningPoker.Web.Game
{
    public class Round
    {
        public Dictionary<Player, string> Cards { get; set; } = new Dictionary<Player, string>();

        public bool IsRevealed { get; set; } = false;

        public bool IsRevealing { get; set; } = false;

        public List<(Player Player, string Card)> HighestCardsWithPlayer
        {
            get
            {
                var parsedCardValues = Cards.Select(c => (player: c.Key, card: double.TryParse(c.Value, out var parsedValue) ? parsedValue : double.NaN));
                var parsedCardValuesWithoutNan = parsedCardValues.Where(p => !double.IsNaN(p.card));

                if (!parsedCardValuesWithoutNan.Any())
                {
                    return [];
                }

                return [.. parsedCardValuesWithoutNan.Where(p => p.card == parsedCardValuesWithoutNan.Max(c => c.card)).Select(max => (max.player, max.card.ToString()))];
            }
        }

        public List<(Player Player, string Card)> LowestCardsWithPlayer
        {
            get
            {
                var parsedCardValues = Cards.Select(c => (player: c.Key, card: double.TryParse(c.Value, out var parsedValue) ? parsedValue : double.NaN));
                var parsedCardValuesWithoutNan = parsedCardValues.Where(p => !double.IsNaN(p.card));

                if(!parsedCardValuesWithoutNan.Any())
                {
                    return [];
                }

                return [.. parsedCardValuesWithoutNan.Where(p => p.card == parsedCardValuesWithoutNan.Min(c => c.card)).Select(max => (max.player, max.card.ToString()))];
            }
        }
    }
}
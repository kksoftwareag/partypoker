using System;
using System.Diagnostics.CodeAnalysis;

namespace PlanningPoker.Web.Game
{
    public class Player : IEquatable<Player>
    {
        public string Name { get; set; }
        public Guid Secret { get; set; }

        public bool Equals([AllowNull] Player other)
        {
            if (other is null)
            {
                return false;
            }

            return other.Secret == this.Secret;
        }

        public override bool Equals(object other)
        {
            if (other is Player p)
            {
                return this.Equals(p);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.Secret.GetHashCode();
        }

        public static bool operator ==(Player left, Player right)
        {
            return left?.Equals(right) == true;
        }

        public static bool operator !=(Player left, Player right) => left == right == false;

    }
}
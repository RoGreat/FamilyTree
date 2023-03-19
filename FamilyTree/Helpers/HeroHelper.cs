using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace FamilyTree.Helpers
{
    public static class HeroHelper
    {
        // Patrilineal -> leader ranking -> clan ranking
        public static Hero FindAncestorOf(Hero hero)
        {
            List<Hero> parents = new();

            // Add parents to list if not null
            // Also account for null clans just in case
            if (hero.Father?.Clan is not null) 
            {
                parents.Add(hero.Father);
            }

            if (hero.Mother?.Clan is not null)
            {
                parents.Add(hero.Mother);
            }

            // Kingdom Ruling Clan Leader
            foreach (var parent in parents.Where(parent => parent.Clan.Kingdom?.Leader == parent))
            {
                return FindAncestorOf(parent);
            }
            // Kingdom Ruling Clan
            foreach (var parent in parents.Where(parent => parent.Clan.Kingdom?.RulingClan == parent.Clan))
            {
                return FindAncestorOf(parent);
            }

            // Kingdom Clan Leader
            foreach (var parent in parents.Where(parent => parent.MapFaction.IsKingdomFaction && parent.IsFactionLeader))
            {
                return FindAncestorOf(parent);
            }
            // Kingdom Clan
            foreach (var parent in parents.Where(parent => parent.MapFaction.IsKingdomFaction))
            {
                return FindAncestorOf(parent);
            }

            // Minor Faction Leader
            foreach (var parent in parents.Where(parent => parent.Clan.IsMinorFaction && parent.IsFactionLeader))
            {
                return FindAncestorOf(parent);
            }
            // Minor Faction Clan
            foreach (var parent in parents.Where(parent => parent.Clan.IsMinorFaction))
            {
                return FindAncestorOf(parent);
            }

            // Clan Leader
            foreach (var parent in parents.Where(parent => parent.Clan.Leader == parent))
            {
                return FindAncestorOf(parent);
            }

            // Other
            foreach (var parent in parents)
            {
                return FindAncestorOf(parent);
            }

            return hero;
        }
    }
}
using TaleWorlds.CampaignSystem;

namespace FamilyTree.Extension
{
    public static class HeroHelper
    {
        public static Hero FindAncestorOf(Hero hero, Clan clan)
        {
            if (hero.Father?.IsFactionLeader ?? false)
            {
                return FindAncestorOf(hero.Father, clan);
            }
            if (hero.Mother?.IsFactionLeader ?? false)
            {
                return FindAncestorOf(hero.Mother, clan);
            }
            if (hero.Father?.Clan == clan)
            {
                return FindAncestorOf(hero.Father, clan);
            }
            if (hero.Mother?.Clan == clan)
            {
                return FindAncestorOf(hero.Mother, clan);
            }
            return hero;
        }
    }
}
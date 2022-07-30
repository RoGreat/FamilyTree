using TaleWorlds.CampaignSystem;

namespace FamilyTree.Extension
{
    public static class HeroHelper
    {
        public static Hero FindPaternalAncestorOf(Hero hero)
        {
            if (hero.Father is not null)
            {
                return FindPaternalAncestorOf(hero.Father);
            }
            return hero;
        }

        public static Hero FindMaternalAncestorOf(Hero hero)
        {
            if (hero.Mother is not null)
            {
                return FindMaternalAncestorOf(hero.Mother);
            }
            return hero;
        }
    }
}
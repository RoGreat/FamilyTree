using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;

namespace FamilyTree.Encyclopedia
{
    /* Reference EncyclopediaTroopTreeNodeVM */
    public class EncyclopediaFamilyTreeNodeVM : ViewModel 
    {
        private MBBindingList<EncyclopediaFamilyTreeNodeVM> _branch;

        private HeroVM _familyMember;

        public EncyclopediaFamilyTreeNodeVM(Hero rootHero, Hero activeHero)
        {
            Branch = new MBBindingList<EncyclopediaFamilyTreeNodeVM>();
            FamilyMember = new HeroVM(rootHero);
            foreach (Hero child in rootHero.Children)
            {
                Branch.Add(new EncyclopediaFamilyTreeNodeVM(child, activeHero));
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Branch.ApplyActionOnAllItems(delegate (EncyclopediaFamilyTreeNodeVM x)
            {
                x.RefreshValues();
            });
            FamilyMember.RefreshValues();
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFamilyTreeNodeVM> Branch
        {
            get
            {
                return _branch;
            }
            set
            {
                if (value != _branch)
                {
                    _branch = value;
                    OnPropertyChanged("Branch");
                }
            }
        }

        [DataSourceProperty]
        public HeroVM FamilyMember
        {
            get
            {
                return _familyMember;
            }
            set
            {
                if (value != _familyMember)
                {
                    _familyMember = value;
                    OnPropertyChanged("FamilyMember");
                }
            }
        }
    }
}

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

        public EncyclopediaFamilyTreeNodeVM(Hero ancestor, Hero hero)
        {
            Branch = new MBBindingList<EncyclopediaFamilyTreeNodeVM>();
            FamilyMember = new HeroVM(hero);
            foreach (Hero child in ancestor.Children)
            {
                if (child == ancestor)
                {
                    continue;
                }
                Branch.Add(new EncyclopediaFamilyTreeNodeVM(hero, child));
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

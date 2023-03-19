using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.Library;

namespace FamilyTree.ViewModels
{
    /* Reference EncyclopediaTroopTreeNodeVM */
    public class EncyclopediaFamilyTreeNodeVM : ViewModel 
    {
        private MBBindingList<EncyclopediaFamilyTreeNodeVM> _branch;

        private MBBindingList<EncyclopediaFamilyMemberVM> _familyMember;

        // Reminding myself what the params are:
        // rootHero runs through each member in the family
        // activeHero is always the currently selected hero
        public EncyclopediaFamilyTreeNodeVM(Hero rootHero, Hero activeHero)
        {
            FamilyBranch = new MBBindingList<EncyclopediaFamilyTreeNodeVM>();
            FamilyMember = new MBBindingList<EncyclopediaFamilyMemberVM>
            {
                new(rootHero, activeHero)
            };
            if (rootHero.Spouse is not null)
            {
                FamilyMember.Add(new EncyclopediaFamilyMemberVM(rootHero.Spouse, activeHero));
            }
            // Almost forgot to add exspouses
            // We'll see how crazy this gets!
            foreach (var exSpouses in rootHero.ExSpouses)
            {
                FamilyMember.Add(new EncyclopediaFamilyMemberVM(exSpouses, activeHero));
            }
            foreach (var child in rootHero.Children)
            {
                FamilyBranch.Add(new EncyclopediaFamilyTreeNodeVM(child, activeHero));
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            FamilyBranch.ApplyActionOnAllItems(delegate (EncyclopediaFamilyTreeNodeVM x)
            {
                x.RefreshValues();
            });
            FamilyMember.ApplyActionOnAllItems(delegate (EncyclopediaFamilyMemberVM x)
            {
                x.RefreshValues();
            });
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFamilyMemberVM> FamilyMember
        {
            get => _familyMember;
            set
            {
                if (value == _familyMember)
                {
                    return;
                }
                _familyMember = value;
                OnPropertyChanged();
            }
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFamilyTreeNodeVM> FamilyBranch
        {
            get => _branch;
            set
            {
                if (value == _branch)
                {
                    return;
                }
                _branch = value;
                OnPropertyChanged();
            }
        }
    }
}

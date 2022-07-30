using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Library;
using FamilyTree.Encyclopedia;

namespace FamilyTree.Extension
{
    /* Reference EncyclopediaUnitPageVM and EncyclopediaHeroPageVM */
    [ViewModelMixin]
    internal class EncyclopediaHeroPageVMMixin : BaseViewModelMixin<EncyclopediaHeroPageVM>
    {
        private EncyclopediaFamilyTreeNodeVM _familyTree;

        private string _familyTreeText;

        private readonly Hero _hero;

        public EncyclopediaHeroPageVMMixin(EncyclopediaHeroPageVM vm) : base(vm)
        {
            _hero = vm.Obj as Hero;
            // Hero ancestor = FamilyHelper.FindAncestorOf(_hero);
            Hero paternalAncestor = HeroHelper.FindPaternalAncestorOf(_hero);
            Hero maternalAncestor = HeroHelper.FindMaternalAncestorOf(_hero);
            FamilyTree = new EncyclopediaFamilyTreeNodeVM(paternalAncestor, _hero);
            vm.RefreshValues();
            FamilyTreeText = GameTexts.FindText("str_family_tree_group", null).ToString();
            vm.Refresh();
        }

        [DataSourceProperty]
        public EncyclopediaFamilyTreeNodeVM FamilyTree
        {
            get
            {
                return _familyTree;
            }
            set
            {
                if (value != _familyTree)
                {
                    _familyTree = value;
                    ViewModel?.OnPropertyChangedWithValue(value, "FamilyTree");
                }
            }
        }

        [DataSourceProperty]
        public string FamilyTreeText
        {
            get
            {
                return _familyTreeText;
            }
            set
            {
                if (value != _familyTreeText)
                {
                    _familyTreeText = value;
                    ViewModel?.OnPropertyChangedWithValue(value, "FamilyTreeText");
                }
            }
        }
    }
}

using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Library;
using FamilyTree.Helpers;

namespace FamilyTree.ViewModels
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
            _hero = (vm.Obj as Hero)!;
            var rootHero = HeroHelper.FindAncestorOf(_hero);
            FamilyTree = new EncyclopediaFamilyTreeNodeVM(rootHero, _hero);
            vm.RefreshValues();
            FamilyTreeText = GameTexts.FindText("str_familytreegroup").ToString();
        }

        [DataSourceProperty]
        public EncyclopediaFamilyTreeNodeVM FamilyTree
        {
            get => _familyTree;
            set
            {
                if (value == _familyTree)
                {
                    return;
                }
                _familyTree = value;
                ViewModel?.OnPropertyChangedWithValue(value);
            }
        }

        [DataSourceProperty]
        public string FamilyTreeText
        {
            get => _familyTreeText;
            set
            {
                if (value == _familyTreeText)
                {
                    return;
                }
                _familyTreeText = value;
                ViewModel?.OnPropertyChangedWithValue(value);
            }
        }
    }
}

using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace FamilyTree.Patches
{
    /* Repurposed from Extended Family to add in more relevant roles */
    [HarmonyPatch(typeof(ConversationHelper), "GetHeroRelationToHeroTextShort")]
    internal class GetHeroRelationToHeroTextShortPatch
    {
        private static List<string> _list;

        private static bool Prefix(ref string __result, Hero queriedHero, Hero baseHero, bool uppercaseFirst)
        {
            _list = new List<string>();
            var skip = false;

            // Siblings
            foreach (var sibling in baseHero.Siblings)
            {
                if (sibling == queriedHero)
                {
                    if (baseHero.Father == queriedHero.Father && baseHero.Mother == queriedHero.Mother)
                    {
                        skip = baseHero.Age == queriedHero.Age ? AddList("str_twin") : AddList(queriedHero.IsFemale ? "str_sister" : "str_brother");
                    }
                    // != acts as an XOR (exclusive OR)
                    else if (baseHero.Father == queriedHero.Father != (baseHero.Mother == queriedHero.Mother))
                    {
                        skip = AddList(queriedHero.IsFemale ? "str_halfsister" : "str_halfbrother");
                    }
                }
                else if (sibling.Spouse == queriedHero)
                {
                    // In-laws
                    skip = AddList(queriedHero.IsFemale ? "str_sisterinlaw" : "str_brotherinlaw");
                }
                else
                {
                    // Nieces/nephews
                    skip = GetNieceNephew(sibling, queriedHero);
                }
                if (skip)
                {
                    break;
                }
            }
            // Children
            if (!skip)
            {
                foreach (var child in baseHero.Children)
                {
                    if (child == queriedHero)
                    {
                        skip = AddList(queriedHero.IsFemale ? "str_daughter" : "str_son");
                    }
                    else if (child.Spouse == queriedHero)
                    {
                        // In-laws
                        skip = AddList(queriedHero.IsFemale ? "str_daughterinlaw" : "str_soninlaw");
                    }
                    else
                    {
                        // Grandchildren
                        skip = GetGrandChildren(child, queriedHero);
                    }
                    if (skip)
                    {
                        break;
                    }
                }
            }
            // Parents
            if (!skip)
            {
                if (baseHero.Father == queriedHero)
                {
                    AddList("str_father");
                }
                else if (baseHero.Mother == queriedHero)
                {
                    AddList("str_mother");
                }
                else if (!RelatedToParent(baseHero.Father, queriedHero))
                {
                    RelatedToParent(baseHero.Mother, queriedHero);
                }
            }
            // Spouse
            if (baseHero.Spouse == queriedHero)
            {
                AddList(queriedHero.IsFemale ? "str_wife" : "str_husband");
            }
            else if (baseHero.ExSpouses.Contains(queriedHero))
            {
                AddList(queriedHero.IsFemale ? "str_exwife" : "str_exhusband");
            }
            // You
            if (queriedHero == baseHero)
            {
                AddList("str_you");
            }
            // Companion
            // Weirdness with hero's family being considered companions
            // Should only apply when no other titles are applied
            if (_list.Count == 0)
            {
                if (queriedHero.CompanionOf == baseHero.Clan)
                {
                    AddList("str_companion");
                }
            }

            // Filter out dupes
            _list = _list.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();

            // Join titles together
            var result = string.Join(", ", _list);
            TextObject textObject = new(result);

            // Adapted from original method
            if (textObject.Length == 0)
            {
                textObject = GameTexts.FindText("str_relative_of_player");
            }
            else
            {
                textObject.SetCharacterProperties("NPC", queriedHero.CharacterObject);
            }

            var text = textObject.ToString();
            if (!char.IsLower(text[0]) != uppercaseFirst)
            {
                var array = text.ToCharArray();
                text = (uppercaseFirst ? array[0].ToString().ToUpper() : array[0].ToString().ToLower());
                for (var i = 1; i < array.Count(); i++)
                {
                    text += array[i].ToString();
                }
            }
            __result = text;

            // Skipping this method
            return false;
        }

        private static bool GetNieceNephew(Hero sibling, Hero queriedHero, int order = 0)
        {
            foreach (var nieceNephew in sibling.Children)
            {
                if (nieceNephew == queriedHero)
                {
                    return order switch
                    {
                        0 => AddList(queriedHero.IsFemale ? "str_niece" : "str_nephew"),
                        1 => AddList(queriedHero.IsFemale ? "str_grandniece" : "str_grandnephew"),
                        _ => AddList(queriedHero.IsFemale ? "str_grandniece" : "str_grandnephew", order)
                    };
                }
                if (nieceNephew.Spouse == queriedHero)
                {
                    return order switch
                    {
                        0 => AddList(queriedHero.IsFemale ? "str_nieceinlaw" : "str_nephewinlaw"),
                        1 => AddList(queriedHero.IsFemale ? "str_grandnieceinlaw" : "str_grandnephewinlaw"),
                        _ => AddList(queriedHero.IsFemale ? "str_grandnieceinlaw" : "str_grandnephewinlaw", order)
                    };
                }
                order += 1;
                if (GetNieceNephew(nieceNephew, queriedHero, order))
                {
                    return true;
                }
                order -= 1;
            }
            return false;
        }

        private static bool GetGrandChildren(Hero child, Hero queriedHero, int order = 1)
        {
            foreach (var grandChild in child.Children)
            {
                if (grandChild == queriedHero)
                {
                    return order == 1 ? AddList(queriedHero.IsFemale ? "str_granddaughter" : "str_grandson") : AddList(queriedHero.IsFemale ? "str_granddaughter" : "str_grandson", order);
                }
                if (grandChild.Spouse == queriedHero)
                {
                    return order == 1 ? AddList(queriedHero.IsFemale ? "str_granddaughterinlaw" : "str_grandsoninlaw") : AddList(queriedHero.IsFemale ? "str_granddaughterinlaw" : "str_grandsoninlaw", order);
                }
                order += 1;
                if (GetGrandChildren(grandChild, queriedHero, order))
                {
                    return true;
                }
                order -= 1;
            }
            return false;
        }

        private static bool RelatedToParent(Hero? parent, Hero queriedHero, int order = 1)
        {
            if (parent is null)
            {
                return false;
            }
            if (GetGrandParent(parent, queriedHero, order))
            {
                return true;
            }
            if (GetParentsSiblingRelative(parent, queriedHero, order))
            {
                return true;
            }
            order += 1;
            return RelatedToParent(parent.Father, queriedHero, order) || RelatedToParent(parent.Mother, queriedHero, order);
        }

        private static bool GetParentsSiblingRelative(Hero parent, Hero queriedHero, int order)
        {
            foreach (var auntUncle in parent.Siblings)
            {
                if (auntUncle == queriedHero)
                {
                    return order == 1 ? AddList(queriedHero.IsFemale ? "str_aunt" : "str_uncle") : AddList(queriedHero.IsFemale ? "str_grandaunt" : "str_granduncle", order);
                }
                if (auntUncle.Spouse == queriedHero)
                {
                    return order == 1 ? AddList(queriedHero.IsFemale ? "str_auntinlaw" : "str_uncleinlaw") : AddList(queriedHero.IsFemale ? "str_grandauntinlaw" : "str_granduncleinlaw", order);
                }
                if (GetCousin(auntUncle, queriedHero))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool GetCousin(Hero auntUncle, Hero queriedHero, int order = 1)
        {
            foreach (var auntUncleChild in auntUncle.Children)
            {
                if (auntUncleChild == queriedHero)
                {
                    return order switch
                    {
                        1 => AddList("str_firstcousin"),
                        2 => AddList("str_secondcousin"),
                        3 => AddList("str_thirdcousin"),
                        _ => AddList("str_distantcousin")
                    };
                }
                if (auntUncleChild.Spouse == queriedHero)
                {
                    return order switch
                    {
                        1 => AddList("str_firstcousininlaw"),
                        2 => AddList("str_secondcousininlaw"),
                        3 => AddList("str_thirdcousininlaw"),
                        _ => AddList("str_distantcousininlaw")
                    };
                }
                order += 1;
                if (GetCousin(auntUncleChild, queriedHero, order))
                {
                    return true;
                }
                order -= 1;
            }
            return false;
        }

        private static bool GetGrandParent(Hero parent, Hero queriedHero, int order)
        {
            if (parent.Father == queriedHero)
            {
                return order == 1 ? AddList("str_grandfather") : AddList("str_grandfather", order);
            }
            if (parent.Mother == queriedHero)
            {
                return order == 1 ? AddList("str_grandmother") : AddList("str_grandmother", order);
            }
            if (parent.Father?.Spouse == queriedHero)
            {
                return order == 1 ? AddList("str_grandfatherinlaw") : AddList("str_grandfatherinlaw", order);
            }
            if (parent.Mother?.Spouse == queriedHero)
            {
                return order == 1 ? AddList("str_grandmotherinlaw") : AddList("str_grandmotherinlaw", order);
            }
            return false;
        }

        private static bool AddList(string str, int order = 1)
        {
            switch (order)
            {
                case <= 1:
                    _list.Add(FindText(str).ToString());
                    break;
                case 2:
                    // Great
                    _list.Add(FindText("str_great") + " " + FindText(str));
                    break;
                default:
                    // Great great = x2 so order - 1 because order starts at 3
                    _list.Add(FindText("str_great") + " x" + (order - 1) + " " + FindText(str));
                    break;
            }
            return true;
        }

        private static TextObject FindText(string id)
        {
            return GameTexts.FindText(id);
        }
    }
}

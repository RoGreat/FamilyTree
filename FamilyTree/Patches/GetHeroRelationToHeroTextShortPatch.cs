﻿using HarmonyLib;
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
            bool skip = false;

            // Siblings
            if (baseHero.Siblings.Contains(queriedHero))
            {
                if (baseHero.Father == queriedHero.Father && baseHero.Mother == queriedHero.Mother)
                {
                    if (baseHero.Age == queriedHero.Age)
                    {
                        skip = AddList("str_twin");
                    }
                    else
                    {
                        skip = AddList(queriedHero.IsFemale ? "str_sister" : "str_brother");
                    }
                }
                // != acts as an XOR (exclusive OR)
                else if (baseHero.Father == queriedHero.Father != (baseHero.Mother == queriedHero.Mother))
                {
                    skip = AddList(queriedHero.IsFemale ? "str_halfsister" : "str_halfbrother");
                }
            }
            else 
            {
                // Nieces/nephews
                foreach (Hero sibling in baseHero.Siblings)
                {
                    skip = GetNieceNephew(sibling, queriedHero);
                    if (skip)
                    {
                        break;
                    } 
                }
            }
            // Children
            if (!skip)
            {
                skip = false;
                if (baseHero.Children.Contains(queriedHero))
                {
                    skip = AddList(queriedHero.IsFemale ? "str_daughter" : "str_son");
                }
                else
                {
                    // Grandchildren
                    foreach (Hero child in baseHero.Children)
                    {
                        skip = GetGrandChildren(child, queriedHero);
                        if (skip)
                        {
                            break;
                        }
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
            string result = string.Join(", ", _list);
            TextObject textObject = new(result, null);

            // Adapted from original method
            if (textObject.Length == 0)
            {
                Debug.FailedAssert("GENERIC - UNSPECIFIED RELATION in clan", "C:\\Develop\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Conversation\\ConversationHelper.cs", "GetHeroRelationToHeroTextShort", 275);
                textObject = GameTexts.FindText("str_relative_of_player", null);
            }
            else if (queriedHero != null)
            {
                textObject.SetCharacterProperties("NPC", queriedHero.CharacterObject, false);
            }
            string text = textObject.ToString();
            if (!char.IsLower(text[0]) != uppercaseFirst)
            {
                char[] array = text.ToCharArray();
                text = (uppercaseFirst ? array[0].ToString().ToUpper() : array[0].ToString().ToLower());
                for (int i = 1; i < array.Count(); i++)
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
            foreach (Hero nieceNephew in sibling.Children)
            {
                if (nieceNephew == queriedHero)
                {
                    if (order == 0)
                    {
                        return AddList(queriedHero.IsFemale ? "str_niece" : "str_nephew");
                    }
                    if (order == 1)
                    {
                        return AddList(queriedHero.IsFemale ? "str_grandniece" : "str_grandnephew");
                    }
                    return AddList(queriedHero.IsFemale ? "str_grandniece" : "str_grandnephew", order);
                }
                if (GetNieceNephew(nieceNephew, queriedHero, ++order))
                {
                    return true;
                }
                else
                {
                    order--;
                }
            }
            return false;
        }

        private static bool GetGrandChildren(Hero child, Hero queriedHero, int order = 1) 
        {
            foreach(Hero grandChild in child.Children)
            {
                if (grandChild == queriedHero)
                {
                    if (order == 1)
                    {
                        return AddList(queriedHero.IsFemale ? "str_granddaughter" : "str_grandson");
                    }
                    return AddList(queriedHero.IsFemale ? "str_granddaughter" : "str_grandson", order);
                }
                if (GetGrandChildren(grandChild, queriedHero, ++order))
                {
                    return true;
                }
                else
                {
                    order--;
                }
            }
            return false;
        }

        private static bool RelatedToParent(Hero parent, Hero queriedHero, int order = 1)
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
            if (RelatedToParent(parent.Father, queriedHero, ++order))
            {
                return true;
            }
            else
            {
                order--;
            }
            if (RelatedToParent(parent.Mother, queriedHero, ++order))
            {
                return true;
            }
            return false;
        }

        private static bool GetParentsSiblingRelative(Hero parent, Hero queriedHero, int order)
        {
            foreach (Hero auntUncle in parent.Siblings)
            {
                if (auntUncle == queriedHero)
                {
                    if (order == 1)
                    {
                        return AddList(queriedHero.IsFemale ? "str_aunt" : "str_uncle");
                    }
                    return AddList(queriedHero.IsFemale ? "str_grandaunt" : "str_granduncle", order);
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
            foreach(Hero auntUncleChild in auntUncle.Children)
            {
                if (auntUncleChild == queriedHero)
                {
                    if (order == 1)
                    {
                        return AddList("str_firstcousin");
                    }
                    if (order == 2)
                    {
                        return AddList("str_secondcousin");
                    }
                    if (order == 3)
                    {
                        return AddList("str_thirdcousin");
                    }
                    return AddList("str_distantcousin");
                }
                if (GetCousin(auntUncleChild, queriedHero, ++order))
                {
                    return true;
                }
                else
                {
                    order--;
                }
            }
            return false;
        }

        private static bool GetGrandParent(Hero parent, Hero queriedHero, int order)
        {
            if (parent.Father == queriedHero)
            {
                if (order == 1)
                {
                    return AddList("str_grandfather");
                }
                return AddList("str_grandfather", order);
            }
            else if (parent.Mother == queriedHero)
            {
                if (order == 1)
                {
                    return AddList("str_grandmother");
                }
                return AddList("str_grandmother", order);
            }
            return false;
        }

        private static bool AddList(string str, int order = 1)
        {
            if (order <= 1)
            {
                _list.Add(FindText(str).ToString());
            }
            else if (order == 2)
            {
                _list.Add(FindText("str_great") + " " + FindText(str).ToString());
            }
            else
            {
                _list.Add(FindText("str_great") + " x" + order.ToString() + " " +  FindText(str).ToString());
            }
            return true;
        }

        private static TextObject FindText(string id)
        {
            return GameTexts.FindText(id, null);
        }
    }
}

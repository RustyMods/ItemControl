using System;
using System.Collections.Generic;
using System.Text;
using ItemControl.API;

namespace ItemControl.Requirements;

[Serializable]
public class ItemControlData
{
    public string PrefabName = null!;
    public Requirement CraftRequirements = new();
    public Requirement EquipRequirements = new();
    public Requirement ConsumeRequirements = new();
}

[Serializable]
public class Requirement
{
    public Dictionary<string, int> SkillRequirements = new();
    public ClassRequirement AlmanacClassRequirement = new();
    public EpicMMORequirement EpicMMORequirement = new();
}


[Serializable]
public class ClassRequirement
{
    public int Level;
    public int Constitution;
    public int Dexterity;
    public int Strength;
    public int Intelligence;
    public int Wisdom;
}

[Serializable]
public class EpicMMORequirement
{
    public int Level;
    public int Strength;
    public int Agility;
    public int Intellect;
    public int Body;
    public int Vigour;
    public int Special;
}

public class ValidatedItemControlData
{
    public ItemDrop m_item = null!;
    public readonly ValidatedRequirement m_craftRequirements = new();
    public readonly ValidatedRequirement m_equipRequirements = new();
    public readonly ValidatedRequirement m_consumeRequirements = new();

    public bool HaveRequirements() => !CanCraft() || !CanEquip() || !CanConsume();

    public bool CanCraft() => m_craftRequirements.HaveRequirement();
    public bool CanEquip() => m_equipRequirements.HaveRequirement();
    public bool CanConsume() => m_consumeRequirements.HaveRequirement();

    public string GetToolTip()
    {
        StringBuilder stringBuilder = new StringBuilder();
        
        List<string> CraftRequirements = new();
        List<string> EquipRequirements = new();
        List<string> ConsumeRequirements = new();

        foreach (var requirement in m_craftRequirements.GetMissingRequirements())
        {
            CraftRequirements.Add($"{requirement.Key}: <color=red>{requirement.Value}</color>");
        }

        foreach (var requirement in m_equipRequirements.GetMissingRequirements())
        {
            EquipRequirements.Add($"{requirement.Key}: <color=red>{requirement.Value}</color>");
        }

        foreach (var requirement in m_consumeRequirements.GetMissingRequirements())
        {
            ConsumeRequirements.Add($"{requirement.Key}: <color=red>{requirement.Value}</color>");
        }

        if (CraftRequirements.Count > 0)
        {
            stringBuilder.Append("<color=yellow>$craft_requirement</color>\n");
            foreach (var requirement in CraftRequirements)
            {
                stringBuilder.Append(requirement);
                stringBuilder.Append("\n");
            }
        }
        if (EquipRequirements.Count > 0)
        {
            stringBuilder.Append("<color=yellow>$equip_requirement</color>\n");
            foreach (var requirement in EquipRequirements)
            {
                stringBuilder.Append(requirement);
                stringBuilder.Append("\n");
            }
        }
        if (ConsumeRequirements.Count > 0)
        {
            stringBuilder.Append("<color=yellow>$consume_requirement</color>\n");
            foreach (var requirement in ConsumeRequirements)
            {
                stringBuilder.Append(requirement);
                stringBuilder.Append("\n");
            }
        }
        return Localization.instance.Localize(stringBuilder.ToString());
    }

}

public class ValidatedRequirement
{
    public readonly List<ValidatedSkillRequirement> m_skillRequirements = new();
    public ClassRequirement m_classRequirements = new();
    public EpicMMORequirement m_epicMMORequirements = new();

    public bool HaveRequirement()
    {
        if (AlmanacClassAPI.GetLevel() < m_classRequirements.Level) return false;
        if (AlmanacClassAPI.GetConstitution() < m_classRequirements.Constitution) return false;
        if (AlmanacClassAPI.GetDexterity() < m_classRequirements.Dexterity) return false;
        if (AlmanacClassAPI.GetStrength() < m_classRequirements.Strength) return false;
        if (AlmanacClassAPI.GetIntelligence() < m_classRequirements.Intelligence) return false;
        if (AlmanacClassAPI.GetWisdom() < m_classRequirements.Wisdom) return false;
        
        if (EpicMMOSystemAPI.GetLevel() < m_epicMMORequirements.Level) return false;
        if (EpicMMOSystemAPI.GetAttribute(EpicMMOSystemAPI.Attribut.Strength) < m_epicMMORequirements.Strength) return false;
        if (EpicMMOSystemAPI.GetAttribute(EpicMMOSystemAPI.Attribut.Agility) < m_epicMMORequirements.Agility) return false;
        if (EpicMMOSystemAPI.GetAttribute(EpicMMOSystemAPI.Attribut.Intellect) < m_epicMMORequirements.Intellect) return false;
        if (EpicMMOSystemAPI.GetAttribute(EpicMMOSystemAPI.Attribut.Body) < m_epicMMORequirements.Body) return false;
        if (EpicMMOSystemAPI.GetAttribute(EpicMMOSystemAPI.Attribut.Vigour) < m_epicMMORequirements.Vigour) return false;
        if (EpicMMOSystemAPI.GetAttribute(EpicMMOSystemAPI.Attribut.Special) < m_epicMMORequirements.Special) return false;
        foreach (var requirement in m_skillRequirements)
        {
            if (GetPlayerSkill(requirement.m_skill) < requirement.m_level) return false;
        }

        return true;
    }

    public Dictionary<string, string> GetMissingRequirements()
    {
        Dictionary<string, string> output = new();

        foreach (var requirement in m_skillRequirements)
        {
            if (GetPlayerSkill(requirement.m_skill) < requirement.m_level) output[requirement.m_skillName] = requirement.m_level.ToString();
        }
        if (AlmanacClassAPI.GetLevel() < m_classRequirements.Level) output["$text_level"] = m_classRequirements.Level.ToString();
        if (AlmanacClassAPI.GetConstitution() < m_classRequirements.Constitution) output["$almanac_constitution"] = m_classRequirements.Constitution.ToString();
        if (AlmanacClassAPI.GetDexterity() < m_classRequirements.Dexterity) output["$almanac_dexterity"] = m_classRequirements.Dexterity.ToString();
        if (AlmanacClassAPI.GetStrength() < m_classRequirements.Strength) output["$almanac_strength"] = m_classRequirements.Strength.ToString();
        if (AlmanacClassAPI.GetIntelligence() < m_classRequirements.Intelligence) output["$almanac_intelligence"] = m_classRequirements.Intelligence.ToString();
        if (AlmanacClassAPI.GetWisdom() < m_classRequirements.Wisdom) output["$almanac_wisdom"] = m_classRequirements.Wisdom.ToString();
        
        if (EpicMMOSystemAPI.GetLevel() < m_epicMMORequirements.Level) output["$level"] = m_epicMMORequirements.Level.ToString();
        if (EpicMMOSystemAPI.GetAttribute(EpicMMOSystemAPI.Attribut.Strength) < m_epicMMORequirements.Strength) output["$parameter_strength"] = m_epicMMORequirements.Strength.ToString();
        if (EpicMMOSystemAPI.GetAttribute(EpicMMOSystemAPI.Attribut.Agility) < m_epicMMORequirements.Agility) output["$parameter_agility"] = m_epicMMORequirements.Agility.ToString();
        if (EpicMMOSystemAPI.GetAttribute(EpicMMOSystemAPI.Attribut.Intellect) < m_epicMMORequirements.Intellect) output["$parameter_intellect"] = m_epicMMORequirements.Intellect.ToString();
        if (EpicMMOSystemAPI.GetAttribute(EpicMMOSystemAPI.Attribut.Body) < m_epicMMORequirements.Body) output["$parameter_body"] = m_epicMMORequirements.Body.ToString();
        if (EpicMMOSystemAPI.GetAttribute(EpicMMOSystemAPI.Attribut.Vigour) < m_epicMMORequirements.Vigour) output["$parameter_vigour"] = m_epicMMORequirements.Vigour.ToString();
        if (EpicMMOSystemAPI.GetAttribute(EpicMMOSystemAPI.Attribut.Special) < m_epicMMORequirements.Special) output["$parameter_special"] = m_epicMMORequirements.Special.ToString();
        return output;
    }

    private float GetPlayerSkill(Skills.SkillType type) => !Player.m_localPlayer ? 0f : Player.m_localPlayer.GetSkillLevel(type);

}

public class ValidatedSkillRequirement
{
    public Skills.SkillType m_skill;
    public string m_skillName = "";
    public int m_level;
}

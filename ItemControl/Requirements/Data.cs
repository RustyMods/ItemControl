using System;
using System.Collections.Generic;

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
    public List<SkillRequirement> SkillRequirements = new();
    public ClassRequirement AlmanacClassRequirement = new();
}

[Serializable]
public class SkillRequirement
{
    public string Skill = null!;
    public int Level;
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

public class ValidatedItemControlData
{
    public ItemDrop m_item = null!;
    public readonly ValidatedRequirement m_craftRequirements = new();
    public readonly ValidatedRequirement m_equipRequirements = new();
    public readonly ValidatedRequirement m_consumeRequirements = new();
}

public class ValidatedRequirement
{
    public readonly List<ValidatedSkillRequirement> m_skillRequirements = new();
    public ClassRequirement m_classRequirements = new();
}

public class ValidatedSkillRequirement
{
    public Skills.SkillType m_skill;
    public string m_skillName = "";
    public int m_level;
}
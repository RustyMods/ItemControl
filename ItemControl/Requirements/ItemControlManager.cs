using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using ItemControl.API;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ItemControl.Requirements;

public static class ItemControlManager
{
    private static readonly CustomSyncedValue<string> ServerItemController = new(ItemControlPlugin.ConfigSync, "ServerItemController", "");
    private static readonly string m_folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "ItemControl";

    private static readonly string m_customSkillNamesPath = m_folderPath + Path.DirectorySeparatorChar + "SkillDisplayNames.yml";

    private static readonly Dictionary<string, ValidatedItemControlData> m_namedItemControlData = new();
    private static readonly Dictionary<string, ValidatedItemControlData> m_itemControlData = new();
    private static Dictionary<string, string> m_skillDisplayNames = new();

    public static List<string> GetItemControlItemNames() => m_itemControlData.Values.Select(x => x.m_item.name).ToList();
    private static List<ItemDrop> GetItemControlItems() => m_itemControlData.Values.Select(x => x.m_item).ToList();
    private static string GetSkillName(Skills.SkillType type) => m_skillDisplayNames.TryGetValue(type.ToString(), out string name) ? name : type.ToString();
    private static string GetSkillName(string type) => m_skillDisplayNames.TryGetValue(type, out string name) ? name : type;
    public static void LoadCustomSkillNames()
    {
        if (!File.Exists(m_customSkillNamesPath)) return;
        ItemControlPlugin.ItemControlLogger.LogDebug("Loading skill display names file");
        try
        {
            var deserializer = new DeserializerBuilder().Build();
            var file = File.ReadAllText(m_customSkillNamesPath);
            m_skillDisplayNames = deserializer.Deserialize<Dictionary<string, string>>(file);
        }
        catch
        {
            ItemControlPlugin.ItemControlLogger.LogDebug("Failed to parse file: " + m_customSkillNamesPath);
        }
    }
    public static void LearnUnknownRecipes()
    {
        foreach (ItemDrop? data in GetItemControlItems())
        {
            Recipe? recipe = ObjectDB.instance.GetRecipe(data.m_itemData);
            if (!recipe) continue;
            Player.m_localPlayer.AddKnownRecipe(recipe);
        }
    }

    public static string GetTooltip(string name, bool itemName)
    {
        ValidatedItemControlData data = GetItemControlData(name, itemName);
        var stringBuilder = new StringBuilder();

        List<string> CraftRequirements = new();
        List<string> EquipRequirements = new();
        List<string> ConsumeRequirements = new();
        if (AlmanacClassAPI.installed)
        {
            if (data.m_craftRequirements.m_classRequirements.Level > 0)
            {
                CraftRequirements.Add($"<color={GetColor(data.m_craftRequirements.m_classRequirements.Level, AlmanacClassAPI.GetLevel())}>$text_level:</color> <color=orange>{data.m_craftRequirements.m_classRequirements.Level}</color>");
            }
            if (data.m_craftRequirements.m_classRequirements.Constitution > 0)
            {
                CraftRequirements.Add($"<color={GetColor(data.m_craftRequirements.m_classRequirements.Constitution, AlmanacClassAPI.GetConstitution())}>$almanac_constitution:</color> <color=orange>{data.m_craftRequirements.m_classRequirements.Constitution}</color>");
            }
            if (data.m_craftRequirements.m_classRequirements.Dexterity > 0)
            {
                CraftRequirements.Add($"<color={GetColor(data.m_craftRequirements.m_classRequirements.Dexterity, AlmanacClassAPI.GetDexterity())}>$almanac_dexterity:</color> <color=orange>{data.m_craftRequirements.m_classRequirements.Dexterity}</color>");
            }
            if (data.m_craftRequirements.m_classRequirements.Strength > 0)
            {
                CraftRequirements.Add($"<color={GetColor(data.m_craftRequirements.m_classRequirements.Strength, AlmanacClassAPI.GetStrength())}>$almanac_strength:</color> <color=orange>{data.m_craftRequirements.m_classRequirements.Strength}</color>");
            }
            if (data.m_craftRequirements.m_classRequirements.Intelligence > 0)
            {
                CraftRequirements.Add($"<color={GetColor(data.m_craftRequirements.m_classRequirements.Intelligence, AlmanacClassAPI.GetIntelligence())}>$almanac_intelligence:</color> <color=orange>{data.m_craftRequirements.m_classRequirements.Intelligence}</color>");
            }
            if (data.m_craftRequirements.m_classRequirements.Wisdom > 0)
            {
                CraftRequirements.Add($"<color={GetColor(data.m_craftRequirements.m_classRequirements.Wisdom, AlmanacClassAPI.GetWisdom())}>$almanac_wisdom:</color> <color=orange>{data.m_craftRequirements.m_classRequirements.Wisdom}</color>");
            }
            
            if (data.m_equipRequirements.m_classRequirements.Level > 0)
            {
                EquipRequirements.Add($"<color={GetColor(data.m_equipRequirements.m_classRequirements.Level, AlmanacClassAPI.GetLevel())}>$text_level:</color> <color=orange>{data.m_equipRequirements.m_classRequirements.Level}</color>");
            }
            if (data.m_equipRequirements.m_classRequirements.Constitution > 0)
            {
                EquipRequirements.Add($"<color={GetColor(data.m_equipRequirements.m_classRequirements.Constitution, AlmanacClassAPI.GetConstitution())}>$almanac_constitution:</color> <color=orange>{data.m_equipRequirements.m_classRequirements.Constitution}</color>");
            }
            if (data.m_equipRequirements.m_classRequirements.Dexterity > 0)
            {
                EquipRequirements.Add($"<color={GetColor(data.m_equipRequirements.m_classRequirements.Dexterity, AlmanacClassAPI.GetDexterity())}>$almanac_dexterity:</color> <color=orange>{data.m_equipRequirements.m_classRequirements.Dexterity}</color>");
            }
            if (data.m_equipRequirements.m_classRequirements.Strength > 0)
            {
                EquipRequirements.Add($"<color={GetColor(data.m_equipRequirements.m_classRequirements.Strength, AlmanacClassAPI.GetStrength())}>$almanac_strength:</color> <color=orange>{data.m_equipRequirements.m_classRequirements.Strength}</color>");
            }
            if (data.m_equipRequirements.m_classRequirements.Intelligence > 0)
            {
                EquipRequirements.Add($"<color={GetColor(data.m_equipRequirements.m_classRequirements.Intelligence, AlmanacClassAPI.GetIntelligence())}>$almanac_intelligence:</color> <color=orange>{data.m_equipRequirements.m_classRequirements.Intelligence}</color>");
            }
            if (data.m_equipRequirements.m_classRequirements.Wisdom > 0)
            {
                EquipRequirements.Add($"<color={GetColor(data.m_equipRequirements.m_classRequirements.Wisdom, AlmanacClassAPI.GetWisdom())}>$almanac_wisdom:</color> <color=orange>{data.m_equipRequirements.m_classRequirements.Wisdom}</color>");
            }
            
            if (data.m_consumeRequirements.m_classRequirements.Level > 0)
            {
                ConsumeRequirements.Add($"<color={GetColor(data.m_consumeRequirements.m_classRequirements.Level, AlmanacClassAPI.GetLevel())}>$text_level:</color> <color=orange>{data.m_consumeRequirements.m_classRequirements.Level}</color>");
            }
            if (data.m_consumeRequirements.m_classRequirements.Constitution > 0)
            {
                ConsumeRequirements.Add($"<color={GetColor(data.m_consumeRequirements.m_classRequirements.Constitution, AlmanacClassAPI.GetConstitution())}>$almanac_constitution:</color> <color=orange>{data.m_consumeRequirements.m_classRequirements.Constitution}</color>");
            }
            if (data.m_consumeRequirements.m_classRequirements.Dexterity > 0)
            {
                ConsumeRequirements.Add($"<color={GetColor(data.m_consumeRequirements.m_classRequirements.Dexterity, AlmanacClassAPI.GetDexterity())}>$almanac_dexterity:</color> <color=orange>{data.m_consumeRequirements.m_classRequirements.Dexterity}</color>");
            }
            if (data.m_consumeRequirements.m_classRequirements.Strength > 0)
            {
                ConsumeRequirements.Add($"<color={GetColor(data.m_consumeRequirements.m_classRequirements.Strength, AlmanacClassAPI.GetStrength())}>$almanac_strength:</color> <color=orange>{data.m_consumeRequirements.m_classRequirements.Strength}</color>");
            }
            if (data.m_consumeRequirements.m_classRequirements.Intelligence > 0)
            {
                ConsumeRequirements.Add($"<color={GetColor(data.m_consumeRequirements.m_classRequirements.Intelligence, AlmanacClassAPI.GetIntelligence())}>$almanac_intelligence:</color> <color=orange>{data.m_consumeRequirements.m_classRequirements.Intelligence}</color>");
            }
            if (data.m_consumeRequirements.m_classRequirements.Wisdom > 0)
            {
                ConsumeRequirements.Add($"<color={GetColor(data.m_consumeRequirements.m_classRequirements.Wisdom, AlmanacClassAPI.GetWisdom())}>$almanac_wisdom:</color> <color=orange>{data.m_consumeRequirements.m_classRequirements.Wisdom}</color>");
            }
        }

        foreach (var requirement in data.m_craftRequirements.m_skillRequirements)
        {
            CraftRequirements.Add($"<color={GetColor(requirement.m_skill, requirement.m_level)}>{GetSkillName(requirement.m_skillName)}</color>: <color=orange>{requirement.m_level}</color>");
        }
        foreach (var requirement in data.m_equipRequirements.m_skillRequirements)
        {
            EquipRequirements.Add($"<color={GetColor(requirement.m_skill, requirement.m_level)}>{GetSkillName(requirement.m_skillName)}</color>: <color=orange>{requirement.m_level}</color>");
        }
        foreach (var requirement in data.m_consumeRequirements.m_skillRequirements)
        {
            ConsumeRequirements.Add($"<color={GetColor(requirement.m_skill, requirement.m_level)}>{GetSkillName(requirement.m_skillName)}</color>: <color=orange>{requirement.m_level}</color>");
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

    private static string GetColor(Skills.SkillType type, int requirement) => GetPlayerSkill(type) < requirement ? "red" : "green";
    private static string GetColor(int requirement, int level) => level < requirement ? "red" : "green";
    public static bool HaveRequirements(string name, bool itemName) => CanCraft(name, itemName) || CanEquip(name, itemName) || CanConsume(name, itemName);
    public static bool CanCraft(string name, bool itemName)
    {
        ValidatedItemControlData data = GetItemControlData(name, itemName);
        if (AlmanacClassAPI.GetLevel() < data.m_craftRequirements.m_classRequirements.Level) return false;
        if (AlmanacClassAPI.GetConstitution() < data.m_craftRequirements.m_classRequirements.Constitution) return false;
        if (AlmanacClassAPI.GetDexterity() < data.m_craftRequirements.m_classRequirements.Dexterity) return false;
        if (AlmanacClassAPI.GetStrength() < data.m_craftRequirements.m_classRequirements.Strength) return false;
        if (AlmanacClassAPI.GetIntelligence() < data.m_craftRequirements.m_classRequirements.Intelligence) return false;
        if (AlmanacClassAPI.GetWisdom() < data.m_craftRequirements.m_classRequirements.Wisdom) return false;

        foreach (var requirement in data.m_craftRequirements.m_skillRequirements)
        {
            if (GetPlayerSkill(requirement.m_skill) < requirement.m_level) return false;
        }

        return true;
    }

    private static ValidatedItemControlData GetItemControlData(string name, bool itemName)
    {
        ValidatedItemControlData data = new();
        if (itemName)
        {
            if (!m_namedItemControlData.TryGetValue(name, out ValidatedItemControlData validatedItemControlData)) return data;
            data = validatedItemControlData;
        }
        else
        {
            if (!m_itemControlData.TryGetValue(name, out ValidatedItemControlData validatedItemControlData)) return data;
            data = validatedItemControlData;
        }

        return data;
    }

    public static bool CanEquip(string name, bool itemName)
    {
        ValidatedItemControlData data = GetItemControlData(name, itemName);

        if (AlmanacClassAPI.installed)
        {
            if (AlmanacClassAPI.GetLevel() < data.m_equipRequirements.m_classRequirements.Level) return false;
            if (AlmanacClassAPI.GetConstitution() < data.m_equipRequirements.m_classRequirements.Constitution) return false;
            if (AlmanacClassAPI.GetDexterity() < data.m_equipRequirements.m_classRequirements.Dexterity) return false;
            if (AlmanacClassAPI.GetStrength() < data.m_equipRequirements.m_classRequirements.Strength) return false;
            if (AlmanacClassAPI.GetIntelligence() < data.m_equipRequirements.m_classRequirements.Intelligence) return false;
            if (AlmanacClassAPI.GetWisdom() < data.m_equipRequirements.m_classRequirements.Wisdom) return false;
        }

        foreach (var requirement in data.m_equipRequirements.m_skillRequirements)
        {
            if (GetPlayerSkill(requirement.m_skill) < requirement.m_level) return false;
        }

        return true;
    }
    
    public static bool CanConsume(string name, bool itemName)
    {
        ValidatedItemControlData data = GetItemControlData(name, itemName);
        
        if (AlmanacClassAPI.GetLevel() < data.m_consumeRequirements.m_classRequirements.Level) return false;
        if (AlmanacClassAPI.GetConstitution() < data.m_consumeRequirements.m_classRequirements.Constitution) return false;
        if (AlmanacClassAPI.GetDexterity() < data.m_consumeRequirements.m_classRequirements.Dexterity) return false;
        if (AlmanacClassAPI.GetStrength() < data.m_consumeRequirements.m_classRequirements.Strength) return false;
        if (AlmanacClassAPI.GetIntelligence() < data.m_consumeRequirements.m_classRequirements.Intelligence) return false;
        if (AlmanacClassAPI.GetWisdom() < data.m_consumeRequirements.m_classRequirements.Wisdom) return false;

        foreach (var requirement in data.m_consumeRequirements.m_skillRequirements)
        {
            if (GetPlayerSkill(requirement.m_skill) < requirement.m_level) return false;
        }

        return true;
    }

    private static float GetPlayerSkill(Skills.SkillType type) => !Player.m_localPlayer ? 0f : Player.m_localPlayer.GetSkillLevel(type);

    private static List<ItemControlData> GetDefaultItemData()
    {
        List<ItemControlData> output = new()
        {
            new ItemControlData()
            {
                PrefabName = "SwordBronze",
                CraftRequirements = new Requirement()
                {
                    SkillRequirements = new()
                    {
                        new SkillRequirement()
                        {
                            Skill = "Swords",
                            Level = 5,
                        }
                    },
                    AlmanacClassRequirement = new()
                    {
                        Level = 1,
                    }
                },
                EquipRequirements = new()
                {
                    SkillRequirements = new()
                    {
                        new SkillRequirement()
                        {
                            Skill = "Swords",
                            Level = 10
                        }
                    }
                }
            }
        };
        return output;
    }
    
    public static void InitItemController()
    {
        if (!ObjectDB.instance) return;
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (!ZNet.instance.IsServer())
        {
            ItemControlPlugin.ItemControlLogger.LogDebug("Client: Loading server item controller");
            OnServerDataChange();
        }
        else
        {
            LoadLocalItemController();
        }
        
    }

    private static void LoadLocalItemController()
    {
        ItemControlPlugin.ItemControlLogger.LogDebug("Loading files");
        m_namedItemControlData.Clear();
        m_itemControlData.Clear();
        ISerializer serializer = new SerializerBuilder().Build();

        List<ItemControlData> ForServer = new();
        int count = 0;

        string[] paths = Directory.GetFiles(m_folderPath, "*.yml");
        if (paths.Length == 0)
        {
            ItemControlPlugin.ItemControlLogger.LogDebug("Found no files, writing example to disk");
            foreach (var example in GetDefaultItemData())
            {
                if (ValidateFile(example, out ValidatedItemControlData validatedItemControlData))
                {
                    m_namedItemControlData[validatedItemControlData.m_item.m_itemData.m_shared.m_name] = validatedItemControlData;
                    m_itemControlData[validatedItemControlData.m_item.name] = validatedItemControlData;
                    string data = serializer.Serialize(example);
                    string path = m_folderPath + Path.DirectorySeparatorChar + example.PrefabName + ".yml";
                    File.WriteAllText(path, data);
                }
                else
                {
                    ItemControlPlugin.ItemControlLogger.LogWarning("Failed to parse data: " + example.PrefabName);
                }
            }
        }
        else
        {
            foreach (string path in paths)
            {
                if (path == m_customSkillNamesPath) continue;
                try
                {
                    string data = File.ReadAllText(path);
                    IDeserializer deserializer = new DeserializerBuilder().Build();
                    ItemControlData info = deserializer.Deserialize<ItemControlData>(data);

                    if (!Player.m_localPlayer)
                    {
                        ForServer.Add(info);
                        ++count;
                    }
                    else
                    {
                        if (ValidateFile(info, out ValidatedItemControlData validatedItemControlData))
                        {
                            m_namedItemControlData[validatedItemControlData.m_item.m_itemData.m_shared.m_name] =
                                validatedItemControlData;
                            m_itemControlData[validatedItemControlData.m_item.name] = validatedItemControlData;
                            ForServer.Add(info);
                            ++count;
                        }
                        else
                        {
                            ItemControlPlugin.ItemControlLogger.LogWarning("Failed to parse file: ");
                            ItemControlPlugin.ItemControlLogger.LogWarning(path);
                        }
                    }
                }
                catch
                {
                    // ignored
                }
                
            }
        }
        
        string serverData = serializer.Serialize(ForServer);
        ServerItemController.Value = serverData;
        ItemControlPlugin.ItemControlLogger.LogDebug("Loaded " + count + " item control files onto server");
    }

    private static bool ValidateFile(ItemControlData data, out ValidatedItemControlData validatedItemControlData)
    {
        validatedItemControlData = new();
        GameObject item = ObjectDB.instance.GetItemPrefab(data.PrefabName);
        if (!item) return false;
        if (!item.TryGetComponent(out ItemDrop itemDrop)) return false;
        validatedItemControlData.m_item = itemDrop;

        validatedItemControlData.m_craftRequirements.m_classRequirements = data.CraftRequirements.AlmanacClassRequirement;
        foreach(var requirement in data.CraftRequirements.SkillRequirements)
        {
            if (!Enum.TryParse(requirement.Skill, out Skills.SkillType skill))
            {
                string normalizedSkill = requirement.Skill.Replace("_", " ");
                int hash = normalizedSkill.GetStableHashCode();
                Skills.SkillType type = (Skills.SkillType)Math.Abs(hash);
                if (Player.m_localPlayer && Player.m_localPlayer.m_skills.GetSkillDef(type) == null) return false;
                skill = type;
            }
            validatedItemControlData.m_craftRequirements.m_skillRequirements.Add(new ValidatedSkillRequirement()
            {
                m_skill = skill,
                m_level = requirement.Level,
                m_skillName = requirement.Skill
            });
        }

        validatedItemControlData.m_equipRequirements.m_classRequirements = data.EquipRequirements.AlmanacClassRequirement;
        foreach (var requirement in data.EquipRequirements.SkillRequirements)
        {
            if (!Enum.TryParse(requirement.Skill, out Skills.SkillType skill))
            {
                string normalizedSkill = requirement.Skill.Replace("_", " ");
                int hash = normalizedSkill.GetStableHashCode();
                Skills.SkillType type = (Skills.SkillType)Math.Abs(hash);
                if (Player.m_localPlayer && Player.m_localPlayer.m_skills.GetSkillDef(type) == null) return false;
                skill = type;
            }
            validatedItemControlData.m_equipRequirements.m_skillRequirements.Add(new ValidatedSkillRequirement()
            {
                m_skill = skill,
                m_level = requirement.Level,
                m_skillName = requirement.Skill
            });
        }

        validatedItemControlData.m_consumeRequirements.m_classRequirements = data.ConsumeRequirements.AlmanacClassRequirement;
        foreach (var requirement in data.ConsumeRequirements.SkillRequirements)
        {
            if (!Enum.TryParse(requirement.Skill, out Skills.SkillType skill))
            {
                string normalizedSkill = requirement.Skill.Replace("_", " ");
                int hash = normalizedSkill.GetStableHashCode();
                Skills.SkillType type = (Skills.SkillType)Math.Abs(hash);
                if (Player.m_localPlayer && Player.m_localPlayer.m_skills.GetSkillDef(type) == null) return false;
                skill = type;
            }
            validatedItemControlData.m_consumeRequirements.m_skillRequirements.Add(new ValidatedSkillRequirement()
            {
                m_skill = skill,
                m_level = requirement.Level,
                m_skillName = requirement.Skill
            });
        }
        return true;
    }
    
    public static void InitFileWatcher()
    {
        FileSystemWatcher FileWatcher = new FileSystemWatcher(m_folderPath)
        {
            Filter = "*.yml",
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };
        FileWatcher.Changed += OnFileChange;
        FileWatcher.Created += OnFileChange;
        FileWatcher.Deleted += OnFileChange;
    }

    private static void OnFileChange(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath == m_customSkillNamesPath)
        {
            LoadCustomSkillNames();
        }
        else
        {
            InitItemController();
        }
    }

    private static void OnServerDataChange()
    {
        if (ServerItemController.Value.IsNullOrWhiteSpace()) return;
        m_itemControlData.Clear();
        m_namedItemControlData.Clear();
        IDeserializer deserializer = new DeserializerBuilder().Build();
        List<ItemControlData> data = deserializer.Deserialize<List<ItemControlData>>(ServerItemController.Value);
        foreach (var itemControlData in data)
        {
            if (!ValidateFile(itemControlData, out ValidatedItemControlData validatedItemControlData)) continue;
            m_itemControlData[validatedItemControlData.m_item.name] = validatedItemControlData;
            m_namedItemControlData[validatedItemControlData.m_item.m_itemData.m_shared.m_name] = validatedItemControlData;
        }
    }

    public static void InitOnServerChange()
    {
        if (!ZNet.instance.IsServer()) ServerItemController.ValueChanged += OnServerDataChange;
    }
}
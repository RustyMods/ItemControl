using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using ServerSync;
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

    private static List<ItemDrop> GetItemControlItems() => m_itemControlData.Values.Select(x => x.m_item).ToList();
    // private static string GetSkillName(Skills.SkillType type) => m_skillDisplayNames.TryGetValue(type.ToString(), out string name) ? name : type.ToString();
    // private static string GetSkillName(string type) => m_skillDisplayNames.TryGetValue(type, out string name) ? name : type;
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

    public static ValidatedItemControlData GetItemControlData(string name, bool itemName)
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
                        ["Swords"] = 5
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
                        ["Swords"] = 10
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
        validatedItemControlData = new ValidatedItemControlData
        {
            m_craftRequirements =
            {
                m_classRequirements = data.CraftRequirements.AlmanacClassRequirement,
                m_epicMMORequirements = data.CraftRequirements.EpicMMORequirement
            },
            m_equipRequirements =
            {
                m_classRequirements = data.EquipRequirements.AlmanacClassRequirement,
                m_epicMMORequirements = data.EquipRequirements.EpicMMORequirement
            },
            m_consumeRequirements =
            {
                m_classRequirements = data.ConsumeRequirements.AlmanacClassRequirement,
                m_epicMMORequirements = data.ConsumeRequirements.EpicMMORequirement
            }
        };
        
        if (ObjectDB.instance.GetItemPrefab(data.PrefabName) is not { } item || !item.TryGetComponent(out ItemDrop itemDrop)) return false;
        validatedItemControlData.m_item = itemDrop;
        
        foreach(var requirement in data.CraftRequirements.SkillRequirements)
        {
            if (GetValidatedSkill(requirement) is { } skillReq)
            {
                validatedItemControlData.m_craftRequirements.m_skillRequirements.Add(skillReq);
            }
        }

        foreach (var requirement in data.EquipRequirements.SkillRequirements)
        {
            if (GetValidatedSkill(requirement) is { } skillReq)
            {
                validatedItemControlData.m_equipRequirements.m_skillRequirements.Add(skillReq);
            }
        }

        foreach (var requirement in data.ConsumeRequirements.SkillRequirements)
        {
            if (GetValidatedSkill(requirement) is { } skillReq)
            {
                validatedItemControlData.m_consumeRequirements.m_skillRequirements.Add(skillReq);
            }
        }
        return true;
    }

    private static ValidatedSkillRequirement? GetValidatedSkill(KeyValuePair<string, int> requirement)
    {
        if (!Enum.TryParse(requirement.Key, true, out Skills.SkillType skill))
        {
            string normalizedSkill = requirement.Key.Replace("_", " ");
            int hash = normalizedSkill.GetStableHashCode();
            Skills.SkillType type = (Skills.SkillType)Math.Abs(hash);
            if (Player.m_localPlayer && Player.m_localPlayer.m_skills.GetSkillDef(type) == null) return null;
            skill = type;
        }
        return new ValidatedSkillRequirement()
        {
            m_skill = skill,
            m_level = requirement.Value,
            m_skillName = requirement.Key
        };
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
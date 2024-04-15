using HarmonyLib;

namespace ItemControl.Requirements;

public static class Patches
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GetAmmoItem))]
    private static class CanGetAmmo
    {
        private static void Postfix(ref ItemDrop.ItemData __result)
        {
            if (ItemControlPlugin._Enabled.Value is ItemControlPlugin.Toggle.Off) return;
            if (!ItemControlManager.CanEquip(__result.m_shared.m_name, true))
            {
                __result = null;
            };
        } 
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.ConsumeItem))]
    private static class CanConsume
    {
        private static void Postfix(ItemDrop.ItemData item, ref bool __result)
        {
            if (ItemControlPlugin._Enabled.Value is ItemControlPlugin.Toggle.Off) return;
            __result &= ItemControlManager.CanConsume(item.m_shared.m_name, true);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.HaveRequirements),typeof(Recipe),typeof(bool),typeof(int))]
    private static class CanCraft
    {
        private static void Postfix(Recipe recipe, ref bool __result)
        {
            if (ItemControlPlugin._Enabled.Value is ItemControlPlugin.Toggle.Off) return;
            __result &= ItemControlManager.CanCraft(recipe.m_item.name, false);
        }
    }
    
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.IsEquipable))]
    private static class ItemData_CanEquip
    {
        private static void Postfix(ItemDrop.ItemData __instance, ref bool __result)
        {
            if (ItemControlPlugin._Enabled.Value is ItemControlPlugin.Toggle.Off) return;
            __result &= ItemControlManager.CanEquip(__instance.m_shared.m_name, true);
        }
    }

    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip), typeof(ItemDrop.ItemData),typeof(int),typeof(bool),typeof(float))]
    private static class ItemData_AddTooltip
    {
        private static void Postfix(ItemDrop.ItemData item, ref string __result)
        {
            if (ItemControlPlugin._Enabled.Value is ItemControlPlugin.Toggle.Off) return;
            if (!ItemControlManager.HaveRequirements(item.m_shared.m_name, true)) return;

            __result += "\n" + ItemControlManager.GetTooltip(item.m_shared.m_name, true);
        }
    }

    private static bool initialized;

    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    private static class Initialize_ItemController
    {
        private static void Postfix(Player __instance)
        {
            if (initialized) return;
            if (!__instance) return;
            if (!ObjectDB.instance) return;
            ItemControlPlugin.ItemControlLogger.LogDebug("Client: Initializing Item Controller");
            ItemControlManager.InitItemController();
            ItemControlManager.InitFileWatcher();
            initialized = true;
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class Server_Initialize_ItemController
    {
        private static void Postfix(ObjectDB __instance)
        {
            if (UnityEngine.SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null) return;

            if (!__instance) return;
            if (!ZNetScene.instance) return;
            ItemControlPlugin.ItemControlLogger.LogDebug("Server: Initializing Item Controller");
            ItemControlManager.InitItemController();
            ItemControlManager.InitFileWatcher();
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
    private static class CustomSyncedValue
    {
        private static void Postfix(ZNet __instance)
        {
            if (!__instance) return;
            ItemControlManager.InitOnServerChange();
        }
    }
    
}
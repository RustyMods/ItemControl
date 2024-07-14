using HarmonyLib;
namespace ItemControl.Requirements;

public static class Patches
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    private static class Humanoid_EquipItem_Patch
    {
        private static bool Prefix(Humanoid __instance, ItemDrop.ItemData item)
        {
            if (ItemControlPlugin._Enabled.Value is ItemControlPlugin.Toggle.Off) return true;
            if (ItemControlManager.CanEquip(item.m_shared.m_name, true)) return true;
            if (!Player.m_localPlayer) return false;
            if (__instance != Player.m_localPlayer) return true;
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"{item.m_shared.m_name} $msg_locked");
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Attack), nameof(Attack.StartDraw))]
    private static class CanUseAmmo
    {
        private static void Postfix(Humanoid character, ItemDrop.ItemData weapon, ref bool __result)
        {
            if (character != Player.m_localPlayer) return;
            if (ItemControlPlugin._Enabled.Value is ItemControlPlugin.Toggle.Off) return;
            ItemDrop.ItemData ammo = Attack.FindAmmo(character, weapon);
            if (ammo == null) return;
            if (ItemControlManager.CanEquip(ammo.m_shared.m_name, true)) return;
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"{ammo.m_shared.m_name} $msg_locked");
            __result = false;
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

    [HarmonyPriority(Priority.Last)]
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
            if (ItemControlPlugin._LearnItems.Value is ItemControlPlugin.Toggle.Off) return;
            ItemControlManager.LearnUnknownRecipes();
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
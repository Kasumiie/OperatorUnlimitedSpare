using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace OperatorUnlimitedSpare
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo("[UnlimitedSpare] Plugin loaded!");

            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            Log.LogInfo("[UnlimitedSpare] All patches applied!");
        }
    }

    // ---------------------------------------------------------------
    // STRATEGY
    //
    // The reload flow in PlayerNetworking works like this:
    //
    //   1. Player presses reload
    //   2. CMD_ClientReloadMag(ammoType, magcount, ...) is sent to server
    //      → magcount tracks how many spare mags you have left
    //   3. DeregisterOldMagazine() removes the current empty mag from the weapon
    //   4. RegisterNewMagazine()   attaches the new full mag from the pouch
    //   5. MagManagerUI.RefreshMags() updates the HUD pouch display
    //
    // When magcount hits 0, CMD_OnReloadNoMags fires instead — locking
    // you out of reloading.
    //
    // Our approach: patch CMD_ClientReloadMag to ensure magcount never
    // drops below 1, and block CMD_OnReloadNoMags from ever executing.
    // The full reload animation still plays because we're not touching
    // DeregisterOldMagazine / RegisterNewMagazine at all.
    // ---------------------------------------------------------------

    // ---------------------------------------------------------------
    // Patch 1: UserCode_CMD_ClientReloadMag (the actual logic method)
    //
    // Mirror networking in Unity splits CMD_ methods into:
    //   CMD_ClientReloadMag      → networking stub (sends to server)
    //   UserCode_CMD_ClientReloadMag → the actual game logic
    //
    // We patch UserCode so that after it runs normally and decrements
    // magcount, we immediately restore it back to max (99).
    // This way the animation, sound, and IK all play correctly.
    // ---------------------------------------------------------------
    [HarmonyPatch(typeof(PlayerNetworking),
        "UserCode_CMD_ClientReloadMag__Int32__Int32__Int32__Int32")]
    public static class Patch_CMD_ClientReloadMag
    {
        // Run AFTER the original so the animation is triggered normally
        [HarmonyPostfix]
        public static void Postfix(PlayerNetworking __instance)
        {
            // Restore magcount so the player always has spare mags
            // 99 is visually clean and won't overflow any int field
            __instance.magcount = 99;

            Plugin.Log.LogDebug("[UnlimitedSpare] magcount restored to 99 after reload.");
        }
    }

    // ---------------------------------------------------------------
    // Patch 2: Block CMD_OnReloadNoMags entirely
    //
    // This CMD fires when the game thinks you have no mags left.
    // It triggers a "no mags" animation/sound and prevents reloading.
    // We block both the command and its UserCode counterpart.
    // ---------------------------------------------------------------
    [HarmonyPatch(typeof(PlayerNetworking), "UserCode_CMD_OnReloadNoMags")]
    public static class Patch_CMD_OnReloadNoMags
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            Plugin.Log.LogDebug("[UnlimitedSpare] Blocked CMD_OnReloadNoMags.");
            return false; // skip the original — no "out of mags" state
        }
    }

    // ---------------------------------------------------------------
    // Patch 3: Block RPC_OnReloadNoMags (server → client version)
    //
    // Mirror also sends this RPC back to clients to sync the no-mags
    // state across the network. Block it client-side too.
    // ---------------------------------------------------------------
    [HarmonyPatch(typeof(PlayerNetworking), "UserCode_RPC_OnReloadNoMags")]
    public static class Patch_RPC_OnReloadNoMags
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            Plugin.Log.LogDebug("[UnlimitedSpare] Blocked RPC_OnReloadNoMags.");
            return false;
        }
    }

    // ---------------------------------------------------------------
    // Patch 4: MagManagerUI — keep the HUD pouch display showing mags
    //
    // RefreshMags() redraws the mag icons in the HUD based on magcount.
    // We postfix it to ensure the displayed count always looks full.
    // ---------------------------------------------------------------
    [HarmonyPatch(typeof(MagManagerUI), nameof(MagManagerUI.RefreshMags))]
    public static class Patch_MagManagerUI_RefreshMags
    {
        [HarmonyPostfix]
        public static void Postfix(MagManagerUI __instance)
        {
            // After the normal refresh, call RefillAllMags so the pouch
            // icons are always shown as available in the UI.
            __instance.RefillAllMags();
            Plugin.Log.LogDebug("[UnlimitedSpare] MagManagerUI refreshed and refilled.");
        }
    }
}

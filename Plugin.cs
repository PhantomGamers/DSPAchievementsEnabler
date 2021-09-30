using BepInEx;

using HarmonyLib;

using System.Reflection;

namespace AchievementsEnabler
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPrefix]
        // Disables GameAbnormalityCheck
        [HarmonyPatch(typeof(GameAbnormalityCheck), nameof(GameAbnormalityCheck.AfterTick))]
        [HarmonyPatch(typeof(GameAbnormalityCheck), nameof(GameAbnormalityCheck.BeforeTick))]
        // Disables unlocking achievements on Steam
        [HarmonyPatch(typeof(STEAMX), nameof(STEAMX.UnlockAchievement))]
        // Disables unlocking achievements on RailWorks
        [HarmonyPatch(typeof(RAILX), nameof(RAILX.UnlockAchievement))]
        [HarmonyPatch(typeof(RAILX), nameof(RAILX.UnlockAchievement))]
        // Disables uploading data to Milky Way
        [HarmonyPatch(typeof(PARTNER), nameof(PARTNER.UploadClusterGenerationToGalaxyServer))]
        public static bool Prefix()
        {
            return false;
        }
    }
}

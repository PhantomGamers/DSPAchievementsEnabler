using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using System.Reflection;

namespace AchievementsEnabler
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;

            // Plugin startup logic
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

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
        [HarmonyPatch(typeof(GameAbnormalityCheck), nameof(GameAbnormalityCheck.CheckMajorClause))]
        [HarmonyPatch(typeof(GameAbnormalityCheck), nameof(GameAbnormalityCheck.NotifyAbnormalityChecked))]
        [HarmonyPatch(typeof(GameAbnormalityCheck), nameof(GameAbnormalityCheck.CheckFreeModeConfig))]
        // Disables unlocking achievements on Steam
        [HarmonyPatch(typeof(STEAMX), nameof(STEAMX.UnlockAchievement))]
        [HarmonyPatch(typeof(SteamAchievementManager), nameof(SteamAchievementManager.UnlockAchievement))]
        [HarmonyPatch(typeof(SteamAchievementManager), nameof(SteamAchievementManager.Update))]
        [HarmonyPatch(typeof(SteamAchievementManager), nameof(SteamAchievementManager.Start))]
        // Disables unlocking achievements on RailWorks
        [HarmonyPatch(typeof(RAILX), nameof(RAILX.UnlockAchievement))]
        [HarmonyPatch(typeof(RailAchievementManager), nameof(RailAchievementManager.UnlockAchievement))]
        [HarmonyPatch(typeof(RailAchievementManager), nameof(RailAchievementManager.Update))]
        [HarmonyPatch(typeof(RailAchievementManager), nameof(RailAchievementManager.Start))]
        // Disables uploading data to Milky Way
        [HarmonyPatch(typeof(PARTNER), nameof(PARTNER.UploadClusterGenerationToGalaxyServer))]
        [HarmonyPatch(typeof(STEAMX), nameof(STEAMX.UploadScoreToLeaderboard))]
        public static bool Prefix()
        {
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameAbnormalityCheck), nameof(GameAbnormalityCheck.Import))]
        public static void GameAbnormalityCheck_Import_Postfix(GameAbnormalityCheck __instance)
        {
            __instance.checkMask = 0;
            __instance.checkTicks = new long[10];
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameAbnormalityCheck), nameof(GameAbnormalityCheck.isGameNormal))]
        public static bool GameAbnormalityCheck_isGameNormal_Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameAbnormalityCheck), nameof(GameAbnormalityCheck.InitAfterGameDataReady))]
        public static void GameAbnormalityCheck_InitAfterGameDataReady_Postfix(GameAbnormalityCheck __instance)
        {
            __instance.gameData.history.onTechUnlocked -= __instance.CheckTechUnlockValid;
            __instance.gameData.mainPlayer.package.onStorageChange -= __instance.OnPlayerStorageChange;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AchievementSystem), nameof(AchievementSystem.isSelfFormalGame), MethodType.Getter)]
        public static bool AchievementSystem_get_isSelfFormalGame_Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}

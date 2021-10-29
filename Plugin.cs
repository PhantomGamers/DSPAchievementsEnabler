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

            Patches.EnablePlatformAchievements = Config.Bind<bool>(section: "General",
                                                                   key: "EnablePlatformAchievements",
                                                                   Patches.EnablePlatformAchievements,
                                                                   "Enables achievements on Steam and RAIL"
                                                                  ).Value;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch]
    public class Patches
    {
        public static bool EnablePlatformAchievements { get; set; } = false;

        [HarmonyPrefix]
        // Disables GameAbnormalityCheck_Obsolete
        [HarmonyPatch(typeof(GameAbnormalityCheck_Obsolete), nameof(GameAbnormalityCheck_Obsolete.AfterTick))]
        [HarmonyPatch(typeof(GameAbnormalityCheck_Obsolete), nameof(GameAbnormalityCheck_Obsolete.BeforeTick))]
        [HarmonyPatch(typeof(GameAbnormalityCheck_Obsolete), nameof(GameAbnormalityCheck_Obsolete.CheckMajorClause))]
        [HarmonyPatch(typeof(GameAbnormalityCheck_Obsolete), nameof(GameAbnormalityCheck_Obsolete.NotifyAbnormalityChecked))]
        [HarmonyPatch(typeof(GameAbnormalityCheck_Obsolete), nameof(GameAbnormalityCheck_Obsolete.CheckFreeModeConfig))]
        // Disables uploading data to Milky Way
        [HarmonyPatch(typeof(PARTNER), nameof(PARTNER.UploadClusterGenerationToGalaxyServer))]
        [HarmonyPatch(typeof(STEAMX), nameof(STEAMX.UploadScoreToLeaderboard))]
        public static bool Prefix()
        {
            return false;
        }

        [HarmonyPrefix]
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
        public static bool PlatformPrefix()
        {
            return EnablePlatformAchievements;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameAbnormalityCheck_Obsolete), nameof(GameAbnormalityCheck_Obsolete.Import))]
        public static void GameAbnormalityCheck_Obsolete_Import_Postfix(GameAbnormalityCheck_Obsolete __instance)
        {
            __instance.checkMask_obsolete = 0;
            __instance.checkTicks = new long[10];
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameAbnormalityCheck_Obsolete), nameof(GameAbnormalityCheck_Obsolete.isGameNormal))]
        public static bool GameAbnormalityCheck_Obsolete_isGameNormal_Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameAbnormalityCheck_Obsolete), nameof(GameAbnormalityCheck_Obsolete.InitAfterGameDataReady))]
        public static void GameAbnormalityCheck_Obsolete_InitAfterGameDataReady_Postfix(GameAbnormalityCheck_Obsolete __instance)
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

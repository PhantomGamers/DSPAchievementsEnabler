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
        // Disables GameAbnormalityData
        [HarmonyPatch(typeof(GameAbnormalityData), nameof(GameAbnormalityData.NotifyAbnormality))]
        [HarmonyPatch(typeof(AbnormalityLogic), nameof(AbnormalityLogic.GameTick))]
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
        [HarmonyPatch(typeof(AbnormalityRuntimeData), nameof(AbnormalityRuntimeData.Import))]
        public static void AbnormalityRuntimeData_Import_Postfix(AbnormalityRuntimeData __instance)
        {
            __instance.abnormalityTime = 0;
            __instance.evidences = new long[0];
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AchievementLogic), nameof(AchievementLogic.active), MethodType.Getter)]
        [HarmonyPatch(typeof(AchievementLogic), nameof(AchievementLogic.isSelfFormalGame), MethodType.Getter)]
        [HarmonyPatch(typeof(GameAbnormalityData), nameof(GameAbnormalityData.IsGameNormal))]
        public static bool AlwaysTrue(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}

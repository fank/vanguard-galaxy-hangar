using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace VGHangar;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInProcess("VanguardGalaxy.exe")]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "vghangar";
    public const string PluginName = "Vanguard Galaxy Hangar";
    // BepInEx parses PluginVersion through System.Version which rejects SemVer
    // pre-release suffixes, so stick to the plain N.N.N form.
    public const string PluginVersion = "0.1.1";

    internal static Plugin Instance { get; private set; } = null!;
    internal static ManualLogSource Log { get; private set; } = null!;

    internal ConfigEntry<string> CfgFilterRoles = null!;
    internal ConfigEntry<string> CfgFilterSizes = null!;

    private Harmony _harmony = null!;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        CfgFilterRoles = Config.Bind("Filters", "ActiveRoles", "",
            "Comma-separated list of active ship role filters (Combat, Mining, Salvaging, Cargo). " +
            "Empty = all roles shown. Persisted across sessions.");
        CfgFilterSizes = Config.Bind("Filters", "ActiveSizes", "",
            "Comma-separated list of active ship size filters (1, 2, 3...). " +
            "Empty = all sizes shown. Persisted across sessions.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll();

        Log.LogInfo($"{PluginName} v{PluginVersion} loaded ({_harmony.GetPatchedMethods().Count()} patches)");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}

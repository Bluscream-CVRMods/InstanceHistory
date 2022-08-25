using InstanceHistory;
using HarmonyLib;
using MelonLoader;
using System;
using System.Linq;
using ButtonAPI = ChilloutButtonAPI.ChilloutButtonAPIMain;

[assembly: MelonInfo(typeof(InstanceHistory.Main), Guh.Name, Guh.Version, Guh.Author, Guh.DownloadLink)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
namespace InstanceHistory;
public static class Guh {
    public const string Name = "InstanceHistory";
    public const string Author = "Bluscream";
    public const string Version = "1.0.0";
    public const string DownloadLink = "";
}
public static class Patches {
    public static void Init(HarmonyLib.Harmony harmonyInstance) {
        try {
            MelonLogger.Msg("Patching SetJoinTarget");
            _ = harmonyInstance.Patch(typeof(ABI_RC.Core.Networking.IO.Instancing.Instances).GetMethod("SetJoinTarget"), postfix: new HarmonyMethod(typeof(Patches).GetMethod("SetJoinTarget")));
        } catch (Exception ex) {
            MelonLogger.Error(ex);
        }
        MelonLogger.Msg("Harmony patches applied!");
    }

    public static void SetJoinTarget(string instanceId, string worldId) {
        MelonLogger.Msg("SetJoinTarget: {0}:{1}", worldId, instanceId);
        if (ButtonAPI.HasInit) {
            _ = Main.instanceHistoryMenu.Add(worldId, instanceId);
            // Main.instanceHistory.Add($"{worldId}:{instanceId}");
        }
        InstanceHistory.Add(worldId, instanceId);
    }
}

public class Main : MelonMod {
    public bool fully_loaded = false;
    public static InstanceHistoryMenu instanceHistoryMenu;
    // public static ObservableCollection<string> instanceHistory = new ObservableCollection<string>();

    public override void OnPreSupportModule() {
    }
    public override void OnApplicationStart() {
        // MelonPreferences_Category cat = MelonPreferences.CreateCategory(Guh.Name);
        InstanceHistory.Init("UserData/InstanceHistory.json");
        ButtonAPI.OnInit += ButtonAPI_OnInit;
        // instanceHistory = new ObservableCollection<string>();
        // instanceHistory.CollectionChanged += InstanceHistory_CollectionChanged;
        Patches.Init(HarmonyInstance);
    }

    private void ButtonAPI_OnInit() {
        instanceHistoryMenu = new InstanceHistoryMenu(ButtonAPI.MainPage);
        foreach (var entry in InstanceHistory.Instances.OrderByDescending(k => k.Value.LastJoined)) {
            instanceHistoryMenu.Add(entry.Value.WorldId.ToString(), entry.Key, entry.Value.LastJoined??DateTime.Now);
        }
    }
}
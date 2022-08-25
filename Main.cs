using HarmonyLib;
using InstanceHistory;
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
            _ = harmonyInstance.Patch(typeof(ABI_RC.Core.Networking.IO.Instancing.Instances).GetMethod("SetJoinTarget"), postfix: new HarmonyMethod(typeof(Patches).GetMethod("SetJoinTarget")));
        } catch (Exception ex) {
            MelonLogger.Error("Error while patching SetJoinTarget: {0}", ex.Message);
        }
    }

    public static void SetJoinTarget(string instanceId, string worldId) {
        MelonLogger.Msg("SetJoinTarget: {0}:{1}", worldId, instanceId);
        if (ButtonAPI.HasInit) {
            _ = Main.instanceHistoryMenu.Add(worldId, instanceId);
        }
        InstanceHistory.Add(worldId, instanceId);
    }
}

public class Main : MelonMod {
    public static InstanceHistoryMenu instanceHistoryMenu;
    public MelonPreferences_Entry<bool> EnableModSetting;
    public MelonPreferences_Entry<string> HistoryFileSetting;
    public MelonPreferences_Entry<int> HistoryMenuLimit;
    public static MelonPreferences_Entry<int> HistoryFileLimit;

    public override void OnApplicationStart() {
        MelonPreferences_Category cat = MelonPreferences.CreateCategory(Guh.Name);
        EnableModSetting = cat.CreateEntry("EnableMod", true, "Enable History");
        HistoryFileSetting = cat.CreateEntry("HistoryFile", "UserData/InstanceHistory.json", "History File Path");
        HistoryFileLimit = cat.CreateEntry("HistoryFileLimit", 50, "Max History File Entries");
        HistoryMenuLimit = cat.CreateEntry("HistoryMenuLimit", 2, "Max History Menu Entries");
        InstanceHistory.Init((string)HistoryFileSetting.BoxedValue);
        ButtonAPI.OnInit += ButtonAPI_OnInit;
        Patches.Init(HarmonyInstance);
    }

    private void ButtonAPI_OnInit() {
        instanceHistoryMenu = new InstanceHistoryMenu(ButtonAPI.MainPage);
        foreach (System.Collections.Generic.KeyValuePair<string, InstanceHistoryEntry> entry in InstanceHistory.Instances.OrderByDescending(k => k.Value.LastJoined).Take((int)HistoryMenuLimit.BoxedValue)) {
            _ = instanceHistoryMenu.Add(entry.Value.WorldId.ToString(), entry.Key, entry.Value.LastJoined ?? DateTime.Now);
        }
    }
}
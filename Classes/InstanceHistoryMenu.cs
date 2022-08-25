using ChilloutButtonAPI.UI;
using System;
using UnityEngine;


namespace InstanceHistory {
    public class InstanceHistoryMenu {
        public SubMenu Parent { get; set; }
        public SubMenu Menu { get; set; }
        public InstanceHistoryMenu(SubMenu parent) {
            Parent = parent;
            _ = Create();
        }
        public InstanceHistoryMenu Create() {
            Menu = Parent.AddSubMenu("Instance History", "Instance History");
            return this;
        }
        public GameObject Add(string worldId, string instanceId) {
            return Add(worldId, instanceId, DateTime.Now);
        }

        public GameObject Add(string worldId, string instanceId, DateTimeOffset timestamp) {
            return Menu.AddButton($"{timestamp.LocalDateTime}", GetInstanceToolTip(worldId, instanceId), () => {
                ABI_RC.Core.Networking.IO.Instancing.Instances.SetJoinTarget(instanceId, worldId);
            });
        }

        public static string GetInstanceToolTip(string worldId, string instanceId) {
            return $"WorldID: {worldId}\nInstanceID: {instanceId}";
        }
    }
}

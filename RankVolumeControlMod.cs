using MelonLoader;
using System.Collections;
using UnityEngine;
using VRC;
using VRC.Core;

namespace RankVolumeControl
{

    public static class BuildInfo
    {
        public const string Name = "RankVolumeControl";
        public const string Author = "dave-kun, maintained by Bluscream";
        public const string Company = null;
        public const string Version = "1.2.0";
        public const string DownloadLink = "https://github.com/Bluscream/RankVolumeControl";
    }

    public class RankVolumeControlMod : MelonMod
    {

        private static MelonPreferences_Category ModCategory;
        private const string VolumeVisitorPref = "Visitor Volume";
        private const string VolumeNewUserPref = "New User Volume";
        private const string VolumeUserPref = "User Volume";
        private const string VolumeKnownUserPref = "Known User Volume";
        private const string VolumeTrustedUserPref = "Trusted User Volume";
        private const string VolumeFriendPref = "Friend Volume";
        public static bool ModEnabled => (bool)ModCategory.GetEntry("Enabled").BoxedValue;

        public override void OnApplicationStart()
        {
            ModCategory = MelonPreferences.CreateCategory("Rank Volume Control", "Rank Volume Control");
            ModCategory.CreateEntry<bool>("Enabled", true, "Enable Mod (Volumes can be between 0.0 and 1.0. Every 0.1 = 10%");
            ModCategory.CreateEntry<float>(VolumeVisitorPref, 1.0f, "Visitors");
            ModCategory.CreateEntry<float>(VolumeNewUserPref, 1.0f, "New users");
            ModCategory.CreateEntry<float>(VolumeUserPref, 1.0f, "Users");
            ModCategory.CreateEntry<float>(VolumeKnownUserPref, 1.0f, "Known users");
            ModCategory.CreateEntry<float>(VolumeTrustedUserPref, 1.0f, "Trusted users");
            ModCategory.CreateEntry<float>(VolumeFriendPref, 1.0f, "Friends");
            MelonCoroutines.Start(Initialize());
        }

        private IEnumerator Initialize()
        {
            while (ReferenceEquals(NetworkManager.field_Internal_Static_NetworkManager_0, null))
                yield return null;

            MelonLogger.Msg($"Initializing RankVolumeControl. Currently {(ModEnabled ? "enabled" : "disabled")}.");
            NetworkManagerHooks.Initialize();
            NetworkManagerHooks.OnJoin += OnPlayerJoined;
        }

        private void OnPlayerJoined(Player player)
        {
            if (player != null || player.field_Private_APIUser_0 != null)
            {
                if (ModEnabled) UpdatePlayerVolume(player);
            }
        }

        [System.Obsolete]
        public override void OnModSettingsApplied()
        {
            if (!ModEnabled) return;
            var Players = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0;
            for (int i = 0; i < Players.Count; i++)
            {
                Player player = Players[i];
                if(player != null || player.field_Private_APIUser_0 != null)
                {
                    UpdatePlayerVolume(player);
                }
            }
        }

        private void UpdatePlayerVolume(Player player)
        {
            var volume = (float)ModCategory.GetEntry(GetUserVolumePref(player.field_Private_APIUser_0)).BoxedValue;
            player.prop_USpeaker_0.field_Private_Single_1 = Mathf.Min(Mathf.Max(volume, 0.00f), 1.0f);
        }

        private string GetUserVolumePref(APIUser user)
        {
            return APIUser.IsFriendsWith(user.id) ? VolumeFriendPref : user.hasVeteranTrustLevel ? VolumeTrustedUserPref : user.hasTrustedTrustLevel ? VolumeKnownUserPref : user.hasKnownTrustLevel ? VolumeUserPref : user.hasBasicTrustLevel ? VolumeNewUserPref : VolumeVisitorPref;
        }

    }
}

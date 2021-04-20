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
        public const string Author = "dave-kun";
        public const string Company = null;
        public const string Version = "1.1.0";
        public const string DownloadLink = "https://github.com/dave-kun/RankVolumeControl";
    }

    public class RankVolumeControlMod : MelonMod
    {

        private const string ModCategory = "Rank Volume Control";
        private const string VolumeVisitorPref = "Visitor Volume";
        private const string VolumeNewUserPref = "New User Volume";
        private const string VolumeUserPref = "User Volume";
        private const string VolumeKnownUserPref = "Known User Volume";
        private const string VolumeTrustedUserPref = "Trusted User Volume";
        private const string VolumeFriendPref = "Friend Volume";

        public override void OnApplicationStart()
        {
            MelonPrefs.RegisterCategory(ModCategory, "Rank Volume Control");
            MelonPrefs.RegisterFloat(ModCategory, VolumeVisitorPref, 1.0f, "Volume for visitors (e.g. between 0.0 and 1.0. Every 0.1 = 10%");
            MelonPrefs.RegisterFloat(ModCategory, VolumeNewUserPref, 1.0f, "Volume for new users (e.g. between 0.0 and 1.0. Every 0.1 = 10%");
            MelonPrefs.RegisterFloat(ModCategory, VolumeUserPref, 1.0f, "Volume for users (e.g. between 0.0 and 1.0. Every 0.1 = 10%");
            MelonPrefs.RegisterFloat(ModCategory, VolumeKnownUserPref, 1.0f, "Volume for known users (e.g. between 0.0 and 1.0. Every 0.1 = 10%");
            MelonPrefs.RegisterFloat(ModCategory, VolumeTrustedUserPref, 1.0f, "Volume for trusted users (e.g. between 0.0 and 1.0. Every 0.1 = 10%");
            MelonPrefs.RegisterFloat(ModCategory, VolumeFriendPref, 1.0f, "Volume for friends (e.g. between 0.0 and 1.0. Every 0.1 = 10%");
            MelonCoroutines.Start(Initialize());
        }

        private IEnumerator Initialize()
        {
            while (ReferenceEquals(NetworkManager.field_Internal_Static_NetworkManager_0, null))
                yield return null;

            MelonLogger.Log("Initializing RankVolumeControl.");
            NetworkManagerHooks.Initialize();
            NetworkManagerHooks.OnJoin += OnPlayerJoined;
        }

        private void OnPlayerJoined(Player player)
        {
            if (player != null || player.field_Private_APIUser_0 != null)
            {
                UpdatePlayerVolume(player);
            }
        }

        public override void OnModSettingsApplied()
        {
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
            float volume = MelonPrefs.GetFloat(ModCategory, GetUserVolumePref(player.field_Private_APIUser_0));
            player.prop_USpeaker_0.field_Private_Single_1 = Mathf.Min(Mathf.Max(volume, 0.00f), 1.0f);
        }

        private string GetUserVolumePref(APIUser user)
        {
            return APIUser.IsFriendsWith(user.id) ? VolumeFriendPref : user.hasVeteranTrustLevel ? VolumeTrustedUserPref : user.hasTrustedTrustLevel ? VolumeKnownUserPref : user.hasKnownTrustLevel ? VolumeUserPref : user.hasBasicTrustLevel ? VolumeNewUserPref : VolumeVisitorPref;
        }

    }
}

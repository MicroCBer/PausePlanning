using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using UnityEngine;
using HarmonyLib;
using System.Net;
using System.Threading.Tasks;

namespace PausePlanning
{
    [HarmonyPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.RefreshContent))]
    public class StandardLevelDetailViewPatch : HarmonyPatch
    {
        public static IBeatmapLevel level;
        public static IDifficultyBeatmap selectedDifficulty;
        static void Postfix(IBeatmapLevel ____level, IDifficultyBeatmap ____selectedDifficultyBeatmap, LevelParamsPanel ____levelParamsPanel, StandardLevelDetailView __instance)
        {
            level = ____level;
            selectedDifficulty = ____selectedDifficultyBeatmap;
            OnLevelDataUpdated();
        }
        public delegate void LevelDataUpdated();
        public static event LevelDataUpdated OnLevelDataUpdated;
    }
    public class ModifierUI : NotifiableSingleton<ModifierUI>
    {
        private string levelName;
        private string pausess;
        private string notice;

        public ModifierUI()
        {

            StandardLevelDetailViewPatch.OnLevelDataUpdated += StandardLevelDetailViewPatch_OnLevelDataUpdated;
            levelName = "";
            pausess = "";
            for (int i = 0; i < 100; i++)
                pausess += "\n";
            notice = "\n\n";
        }

        private void StandardLevelDetailViewPatch_OnLevelDataUpdated()
        {
            notice = "\n\n";
            Notice = "";
            UpdatePauses(StandardLevelDetailViewPatch.selectedDifficulty);
        }

        static string GetTime(float time)
        {
            float h = Mathf.FloorToInt(time / 3600f);
            float m = Mathf.FloorToInt(time / 60f - h * 60f);
            float s = Mathf.FloorToInt(time - m * 60f - h * 3600f);
            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }

        private void UpdatePauses(IDifficultyBeatmap diff)
        {
            var level = diff.level;
            levelName = level.songName;
            LevelName = "changed";
            pausess = "";
            var num = 1;

            foreach (var d in PausePlanningController.GetPauses(diff))
            {
                pausess += $"{num++} - {GetTime(d)}\n";
            }
            for (int i = 0; i < 100 - num; i++)
                pausess += "\n";
            Pausess = "c";
        }


        [UIValue("lvname")]
        public string LevelName

        {
            get => levelName;
            set
            {
                NotifyPropertyChanged();
            }
        }

        [UIValue("pauses")]
        public string Pausess

        {
            get => pausess;
            set
            {
                NotifyPropertyChanged();
            }
        }

        [UIValue("noticee")]
        public string Notice
        {
            get => notice;
            set
            {
                NotifyPropertyChanged();
            }
        }

        [UIValue("mod_enabled")]
        public bool Mod_Enabled
        {
            get => Config.UserConfig.mod_enabled;
            set
            {
                Config.UserConfig.mod_enabled = value;
            }
        }
        [UIValue("recording_enabled")]
        public bool Recording_Enabled
        {
            get => Config.UserConfig.recording_enabled;
            set
            {
                Config.UserConfig.recording_enabled = value;
            }
        }
        [UIAction("FetchFromInternet")]
        void FetchFromInternet()
        {
            _FetchFromInternet();
        }
        async Task _FetchFromInternet()
        {
            var client = new WebClient();
            try
            {
                var result = await client.DownloadStringTaskAsync(PausePlanningController.GetWebConfigPath(StandardLevelDetailViewPatch.selectedDifficulty));

                System.IO.File.WriteAllText(
                    PausePlanningController.GetConfigPath(StandardLevelDetailViewPatch.selectedDifficulty), result);
                notice = "<#31a79d>Online config found. 找到在线难度配置文件";
                Notice = "c";
                UpdatePauses(StandardLevelDetailViewPatch.selectedDifficulty);
            }
            catch (WebException ex)
            {
                notice = $"Failed（获取失败）\nOnline config may not exists.可能没有在线难度配置文件";
                Notice = "c";
            }
        }
        [UIAction("set_mod_enabled")]
        void Set_Mod_Enabled(bool value)
        {
            Mod_Enabled = value;
        }
        [UIAction("set_recording_enabled")]
        void Set_Recording_Enabled(bool value)
        {
            Recording_Enabled = value;
        }
    }
}
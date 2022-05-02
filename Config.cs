using Newtonsoft.Json;
using System.IO;

namespace PausePlanning
{
    public class PausePlanningConfig
    {
        public bool mod_enabled = true;
        public bool recording_enabled = false;
        public bool nfprotection_enabled = true;
        public bool neversubmit_enabled = false;

        public bool trollmap_enabled = false;
        public float trollmap_threshold = 60f;
        public int trollmap_min_time = 0;
        public int trollmap_max_time = 120;

        public PausePlanningConfig()
        {

        }
        [JsonConstructor]
        public PausePlanningConfig(bool mod_enabled,bool recording_enabled, bool nfprotection_enabled, bool neversubmit_enabled,
            bool trollmap_enabled, float trollmap_threshold, int trollmap_min_time, int trollmap_max_time)

        {
            this.mod_enabled = mod_enabled;
            this.recording_enabled = recording_enabled;
            this.nfprotection_enabled = nfprotection_enabled;
            this.neversubmit_enabled = neversubmit_enabled;

            this.trollmap_enabled = trollmap_enabled;
            this.trollmap_threshold = trollmap_threshold;
            this.trollmap_min_time = trollmap_min_time;
            this.trollmap_max_time = trollmap_max_time;
        }
    }

    public class Config
    {
        public static PausePlanningConfig UserConfig { get; private set; }
        public static string ConfigPath { get; private set; } = Path.Combine(IPA.Utilities.UnityGame.UserDataPath, "PausePlanning.json");

        public static void Read()
        {
            if (!File.Exists(ConfigPath))
            {
                UserConfig = new PausePlanningConfig();
                Write();
            }
            else
            {
                UserConfig = JsonConvert.DeserializeObject<PausePlanningConfig>(File.ReadAllText(ConfigPath));
            }
        }

        public static void Write()
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(UserConfig, Formatting.Indented));
        }
    }
}
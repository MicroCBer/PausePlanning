using BeatSaberMarkupLanguage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

namespace PausePlanning
{
    public class PausePlanningController : MonoBehaviour
    {
        public JObject configParsed;
        public static PausePlanningController Instance { get; private set; }
        public static AudioTimeSyncController audiocontroller;
        public static SongController songcontroller;

        private List<float> pauses;
        private GameplayCoreSceneSetupData gameplayCoreSceneSetupData;
        public static PauseController pausecontroller;

        private TextMeshProUGUI _nextPauseText;
        private GameObject _nextPauseObject;

        private void Awake()
        {

            {
                _nextPauseObject = new GameObject("Pause Planning Prompt");
                _nextPauseObject.transform.position = new Vector3(-0.0f, 2.2f, 7.0f);
                _nextPauseObject.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
                _nextPauseObject.SetActive(false);

                Canvas canvas = _nextPauseObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.enabled = false;

                RectTransform canvasRect = (canvas.transform as RectTransform);
                canvasRect.sizeDelta = new Vector2(100, 50);

                _nextPauseText = BeatSaberUI.CreateText(canvasRect, "", new Vector2(0, 10f));
                _nextPauseText.alignment = TextAlignmentOptions.Center;
                RectTransform textTransform = (_nextPauseText.transform as RectTransform);
                textTransform.SetParent(canvas.transform, false);
                textTransform.sizeDelta = new Vector2(100, 20);
                _nextPauseText.fontSize = 15f;
                canvas.enabled = true;
            }
            _nextPauseObject.SetActive(true);

     



            audiocontroller = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().LastOrDefault();
            pausecontroller = Resources.FindObjectsOfTypeAll<PauseController>().LastOrDefault();

            pausecontroller.didPauseEvent += Pausecontroller_didPauseEvent;

            gameplayCoreSceneSetupData = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData;
            if (Config.UserConfig.mod_enabled && !Config.UserConfig.recording_enabled)
                pauses = GetPauses(gameplayCoreSceneSetupData.difficultyBeatmap);
            else 
                pauses = new List<float>();
            configParsed = GetConfig(gameplayCoreSceneSetupData.difficultyBeatmap);
        }
        static public string GetConfigPath(IDifficultyBeatmap difficulty)
        {
            var lp=GetFolderPath(difficulty)+difficulty.difficulty.Name() + ".json";
            if (!System.IO.File.Exists(lp))
            {
                Plugin.Log.Info("Level pause config file not found, creating..");
                System.IO.File.WriteAllText(lp, "{\"pauses\":[],\"pausePlanningVersion\":\"0.1.2\"}");
            }
            return lp;
        }
        static public string GetWebFolderPath(IDifficultyBeatmap difficulty)
        {
            return $"https://cdn.jsdelivr.net/gh/MicroCBer/PausePlanning-Port/${difficulty.level.levelID}/";
        }
        static public string GetWebConfigPath(IDifficultyBeatmap difficulty)
        {
            Plugin.Log.Info($"https://cdn.jsdelivr.net/gh/MicroCBer/PausePlanning-Port/{difficulty.level.levelID}/{difficulty.difficulty.Name()}.json");
            return $"https://cdn.jsdelivr.net/gh/MicroCBer/PausePlanning-Port/{difficulty.level.levelID}/{difficulty.difficulty.Name()}.json";
        }

        static public string GetFolderPath(IDifficultyBeatmap difficulty)
        {
            var levelData = difficulty.level;
            var levelID = levelData.levelID;
            var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = homePath + "/BeatSaberPlugins/PausePlanning/Levels/" + levelID + "/";
            if (!System.IO.Directory.Exists(path))
            {
                Plugin.Log.Info("Level pause folder not found, creating..");
                System.IO.Directory.CreateDirectory(path);
            }
            return path;
        }
        static public JObject GetConfig(IDifficultyBeatmap difficulty)
        {
            var path = GetConfigPath(difficulty);
            
            var levelData = difficulty.level;
            System.IO.File.WriteAllText(GetFolderPath(difficulty) + "/.leveldata.txt", levelData.songName + "\n" + levelData.songAuthorName + "\n" + levelData.songSubName + "\n" + levelData.levelAuthorName);
            var config = System.IO.File.ReadAllText(path);
            Plugin.Log.Info(config);
            var configParsed = JObject.Parse(config);
            return configParsed;
        }
        static public List<float> GetPauses(IDifficultyBeatmap difficulty)
        {
            var configParsed = GetConfig(difficulty);
            var pauses = JsonConvert.DeserializeObject<List<float>>(configParsed["pauses"].ToString());
            return pauses;
        }

        private void Pausecontroller_didPauseEvent()
        {
            if (Config.UserConfig.mod_enabled)
                if (Config.UserConfig.recording_enabled)
                {
                    pauses.Add(audiocontroller.songTime);
                    Plugin.Log.Info(pauses.Count().ToString());
                    configParsed["pauses"] = JArray.FromObject(pauses);
                    System.IO.File.WriteAllText(GetConfigPath(gameplayCoreSceneSetupData.difficultyBeatmap), configParsed.ToString());
                }
        }

        private void LateUpdate()
        {
            if (audiocontroller != null && pausecontroller != null)
            {
                if (Config.UserConfig.mod_enabled && !Config.UserConfig.recording_enabled && pauses.Count() > 0)
                {
                    if (audiocontroller.songTime > pauses[0])
                    {
                        _nextPauseText.text = "";
                        pauses.Remove(pauses[0]);
                        pausecontroller.Pause();
                    }
                    else
                    {
                        var sec = pauses[0] - audiocontroller.songTime;
                        _nextPauseText.text = $"The next pause is in <color=#ffa500ff>{sec:F2}</color> seconds.";
                    }
                }
                else
                {
                    _nextPauseObject.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            Plugin.Log?.Debug($"{name}: OnDestroy()");
            if (Instance == this)
                Instance = null;

        }
        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>

        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {

        }

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        private void Update()
        {

        }

        /// <summary>
        /// Called every frame after every other enabled script's Update().
        /// </summary>


        /// <summary>
        /// Called when the script becomes enabled and active
        /// </summary>
        private void OnEnable()
        {

        }

        /// <summary>
        /// Called when the script becomes disabled or when it is being destroyed.
        /// </summary>
        private void OnDisable()
        {

        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>

        #endregion
    }
}

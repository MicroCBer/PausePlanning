using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace PausePlanning
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static GameObject plannedpause;
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        internal static Harmony harmony { get; private set; }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            BS_Utils.Utilities.BSEvents.gameSceneLoaded += BSEvents_gameSceneLoaded;
        }

        private void BSEvents_gameSceneLoaded()
        {
            Plugin.Log.Info("In Map");
            plannedpause = new GameObject("PlannedPause");
            plannedpause.AddComponent<PausePlanningController>();
        }


            #region BSIPA Config
            //Uncomment to use BSIPA's config
            /*
            [Init]
            public void InitWithConfig(Config conf)
            {
                Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
                Log.Debug("Config loaded");
            }
            */
            #endregion

            [OnStart]
        public void OnApplicationStart()
        {
            Config.Read();
            Log.Debug("OnApplicationStart");
            new GameObject("PausePlanningController").AddComponent<PausePlanningController>();
            BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.instance.AddTab("Pause Planning", "PausePlanning.modifierUI.bsml", ModifierUI.instance);
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            harmony = new Harmony("Kinsi55.BeatSaber.BetterSongList");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            harmony.UnpatchSelf();
            Config.Write();

        }
        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            Config.Write();
        }
    }
}

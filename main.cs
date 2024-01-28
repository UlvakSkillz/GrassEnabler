using MelonLoader;
using System;
using UnityEngine;

namespace GrassEnabler
{
    public class main : MelonMod
    {
        private string currentScene = "";
        private bool sceneChanged = false;

        public override void OnFixedUpdate()
        {
            if ((sceneChanged) && (currentScene == "Gym"))
            {
                try
                {
                    GameObject.Find("--------------SCENE--------------").transform.GetChild(2).gameObject.SetActive(true);
                    MelonLogger.Msg("Ping");
                    sceneChanged = false;
                }
                catch (Exception e)
                {
                    MelonLogger.Error(e);
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            currentScene = sceneName;
            sceneChanged = true;
        }
    }
}

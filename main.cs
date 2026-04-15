using Il2CppRUMBLE.Interactions.InteractionBase;
using MelonLoader;
using RumbleModdingAPI.RMAPI;
using UIFramework;
using System.Collections;
using System.Globalization;
using UnityEngine;

namespace GrassEnabler
{
    public static class ModBuildInfo { public const string Version = "3.2.0"; }
    
    public class Main : MelonMod
    {
        private string currentScene = "Loader";
        private bool init = false;
        private GameObject grassParent, grassShort, grassLong, grassShortNoCollider, grassLongNoCollider, storedGrass, storedLowerParkGrass;
        internal static System.Random random = new System.Random();
        public static Color grassColor = HexToColor("C0B154");
        internal static bool flatLandModFound = false;
        private bool flatLandActive = false;
        private int flatLandSize = 125;

        public override void OnLateInitializeMelon()
        {
            Actions.onMapInitialized += MapLoaded;
            Actions.onMyModsGathered += SetupDDOLGrass;
        }

        public override void OnInitializeMelon()
        {
            flatLandModFound = MelonMod.RegisteredMelons.FirstOrDefault(mod => mod.Info.Name == "FlatLand") != null;
            Preferences.InitPrefs();
            UI.Register(this, Preferences.GrassCountCategory, Preferences.GrassSettingsCategory, Preferences.GrassVisualsCategory).OnModSaved += Save;
        }

        private void SetupDDOLGrass()
        {
            GameObject bundleGO = AssetBundles.LoadAssetFromStream<GameObject>(this, "GrassEnabler.grass", "Grass");
            grassParent = new GameObject();

            grassLong = GameObject.Instantiate(bundleGO.transform.GetChild(0).gameObject, new Vector3(0, 0, 0), Quaternion.EulerAngles(0, 0, 0));
            grassShort = GameObject.Instantiate(bundleGO.transform.GetChild(1).gameObject, new Vector3(0, 0, 0), Quaternion.EulerAngles(0, 0, 0));
            grassLongNoCollider = GameObject.Instantiate(bundleGO.transform.GetChild(2).gameObject, new Vector3(0, 0, 0), Quaternion.EulerAngles(0, 0, 0));
            grassShortNoCollider = GameObject.Instantiate(bundleGO.transform.GetChild(3).gameObject, new Vector3(0, 0, 0), Quaternion.EulerAngles(0, 0, 0));

            grassParent.name = "GrassParent";
            grassLong.name = "GrassLong";
            grassLongNoCollider.name = "GrassLong";
            grassShort.name = "GrassShort";
            grassShortNoCollider.name = "GrassShort";

            grassLong.transform.parent = grassParent.transform;
            grassLongNoCollider.transform.parent = grassParent.transform;
            grassShort.transform.parent = grassParent.transform;
            grassShortNoCollider.transform.parent = grassParent.transform;

            SetupColliders(grassLong);
            SetupColliders(grassShort);

            List<MeshRenderer> renderers = new List<MeshRenderer>();
            renderers.AddRange(grassLong.transform.GetComponentsInChildren<MeshRenderer>());
            renderers.AddRange(grassShort.transform.GetComponentsInChildren<MeshRenderer>());
            renderers.AddRange(grassLongNoCollider.transform.GetComponentsInChildren<MeshRenderer>());
            renderers.AddRange(grassShortNoCollider.transform.GetComponentsInChildren<MeshRenderer>());
            foreach (MeshRenderer renderer in renderers) { renderer.material.color = grassColor; }

            GameObject.DontDestroyOnLoad(grassParent);
            grassParent.SetActive(false);
            init = true;
        }

        internal static Color HexToColor(string hex)
        {
            try
            {
                hex = hex.Replace("0x", "").Replace("#", "");//in case the string == formatted 0xFFFFFF or #FFFFFF
                if (hex.Length == 3)
                {
                    hex = (hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2]).ToString();
                }
                else if (hex.Length == 4)
                {
                    hex = (hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2] + hex[3] + hex[3]).ToString();
                }
                byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                byte a = (hex.Length >= 8 ? byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber) : (byte)255);//assume fully visible unless specified in hex
                return new Color32(r, g, b, a);
            }
            catch { return HexToColor("C0B154"); }
        }

        public void Save()
        {
            if (Preferences.IsPrefChanged(Preferences.PrefColor) || (Preferences.IsPrefChanged(Preferences.PrefRandomColor) && (!Preferences.PrefRandomColor.Value)))
            {
                grassColor = HexToColor(Preferences.PrefColor.Value);
                List<MeshRenderer> renderers = new List<MeshRenderer>();
                renderers.AddRange(grassLong.transform.GetComponentsInChildren<MeshRenderer>());
                renderers.AddRange(grassShort.transform.GetComponentsInChildren<MeshRenderer>());
                renderers.AddRange(grassLongNoCollider.transform.GetComponentsInChildren<MeshRenderer>());
                renderers.AddRange(grassShortNoCollider.transform.GetComponentsInChildren<MeshRenderer>());
                foreach (MeshRenderer renderer in renderers) { renderer.material.color = grassColor; }
            }
            if (Preferences.IsPrefChanged(Preferences.PrefEnabled)
                || (Preferences.IsPrefChanged(Preferences.PrefRingCount) && (currentScene == "Map0"))
                || (Preferences.IsPrefChanged(Preferences.PrefPitCount) && (currentScene == "Map1"))
                || (Preferences.IsPrefChanged(Preferences.PrefUpperParkCount) && (currentScene == "Park"))
                || (Preferences.IsPrefChanged(Preferences.PrefLowerParkCount) && (currentScene == "Park"))
                || (Preferences.IsPrefChanged(Preferences.PrefFlatLandCount) && (currentScene == "Gym"))
                || Preferences.IsPrefChanged(Preferences.PrefRemoval)
                || Preferences.IsPrefChanged(Preferences.PrefColor)
                || Preferences.IsPrefChanged(Preferences.PrefHeight)
                || Preferences.IsPrefChanged(Preferences.PrefWidth)
                || Preferences.IsPrefChanged(Preferences.PrefRandomColor))
            {
                if (storedGrass != null) { GameObject.DestroyImmediate(storedGrass); }
                if (storedLowerParkGrass != null) { GameObject.DestroyImmediate(storedLowerParkGrass); }
                if (Preferences.PrefEnabled.Value) { MelonCoroutines.Start(SetupStoredGrass()); }
            }
            Preferences.StoreLastSavedPrefs();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            currentScene = sceneName;
            flatLandActive = false;
            if (flatLandModFound && (currentScene == "Gym")) { MelonCoroutines.Start(InitFlatLandFound()); }
        }

        private void MapLoaded(string map)
        {
            if (!init || !Preferences.PrefEnabled.Value) { return; }
            MelonCoroutines.Start(SetupStoredGrass());
        }

        public IEnumerator InitFlatLandFound()
        {
            yield return new WaitForSeconds(1f);
            GameObject.Find("FlatLand/FlatLandButton/Button").GetComponent<InteractionButton>().onPressed.AddListener(new Action(() =>
            {
                flatLandActive = true;
                MelonCoroutines.Start(SetupFlatLand());
            }));
            yield break;
        }

        public static IEnumerator RegrowGrass(GameObject grass)
        {
            yield return new WaitForSeconds(Preferences.PrefTimeBeforeRegrow.Value);
            if (grass == null) { yield break; }
            grass.transform.localScale = new Vector3(Preferences.PrefWidth.Value, 0, Preferences.PrefWidth.Value);
            grass.SetActive(true);
            while ((grass != null) && (grass.transform.localScale.y <= Preferences.PrefHeight.Value))
            {
                try
                {
                    if (Preferences.PrefGrowthTime.Value == 0) { break; }
                    grass.transform.localScale = new Vector3(Preferences.PrefWidth.Value, grass.transform.localScale.y + (Preferences.PrefHeight.Value / (Preferences.PrefGrowthTime.Value * 50f)), Preferences.PrefWidth.Value);
                } catch { yield break; }
                yield return new WaitForFixedUpdate();
                if (grass == null) { yield break; }
            }
            if (grass != null) { grass.transform.localScale = new Vector3(Preferences.PrefWidth.Value, Preferences.PrefHeight.Value, Preferences.PrefWidth.Value); }
            yield break;
        }

        private Vector3 CalculatePoint(float radius, float height)
        {
            var angle = random.NextDouble() * Math.PI * 2;
            var distance = Math.Sqrt(random.NextDouble()) * radius;
            var x = (distance * Math.Cos(angle));
            var z = (distance * Math.Sin(angle));
            return new Vector3((float)x, height, (float)z);
        }

        private IEnumerator SetupStoredGrass()
        {
            if (storedGrass != null) { GameObject.Destroy(storedGrass); }
            storedGrass = new GameObject();
            storedGrass.name = "Grass";
            storedGrass.SetActive(false);

            for (int i = 0; i < (currentScene == "Map0" ? Preferences.PrefRingCount.Value : (currentScene == "Map1" ? Preferences.PrefPitCount.Value : (currentScene == "Park" ? Preferences.PrefUpperParkCount.Value : /*Gym*/Preferences.PrefFlatLandCount.Value))); i++)
            {
                int pickedGrass = random.Next(0, 2);
                GameObject whichGrass = (Preferences.PrefRemoval.Value ? ((pickedGrass == 0) ? grassShort : grassLong) : ((pickedGrass == 0) ? grassShortNoCollider : grassLongNoCollider));
                GameObject grass = GameObject.Instantiate(whichGrass, storedGrass.transform);
                if (Preferences.PrefRandomColor.Value) { RecolorGrassToRandom(grass); }
                grass.transform.position = new Vector3(0, 0, 0);
                grass.transform.rotation = Quaternion.EulerAngles(0, 0, 0);
                grass.transform.localScale = new Vector3(Preferences.PrefWidth.Value, Preferences.PrefHeight.Value, Preferences.PrefWidth.Value);
                if (i % 1000 == 0) { yield return new WaitForFixedUpdate(); }
            }
            if (flatLandActive) { MelonCoroutines.Start(SetupFlatLand()); }
            else if (currentScene == "Park")
            {
                if (storedLowerParkGrass != null) { GameObject.Destroy(storedLowerParkGrass); }
                storedLowerParkGrass = new GameObject();
                storedLowerParkGrass.name = "Grass Lower Park";
                storedLowerParkGrass.SetActive(false);
                for (int i = 0; i < Preferences.PrefLowerParkCount.Value; i++)
                {
                    int pickedGrass = random.Next(0, 2);
                    GameObject whichGrass;
                    if (Preferences.PrefRemoval.Value) { whichGrass = (pickedGrass == 0) ? grassShort : grassLong; }
                    else { whichGrass = (pickedGrass == 0) ? grassShortNoCollider : grassLongNoCollider; }
                    GameObject grass = GameObject.Instantiate(whichGrass, storedLowerParkGrass.transform);
                    grass.transform.position = new Vector3(0, 0, 0);
                    grass.transform.rotation = Quaternion.EulerAngles(0, 0, 0);
                    grass.transform.localScale = new Vector3(Preferences.PrefWidth.Value, Preferences.PrefHeight.Value, Preferences.PrefWidth.Value);
                    if (i % 1000 == 0) { yield return new WaitForFixedUpdate(); }
                }
                MelonCoroutines.Start(SetupParkUpperGrass());
                MelonCoroutines.Start(SetupParkLowerGrass());
            }
            else if (currentScene == "Map0") { MelonCoroutines.Start(SetupMap0()); }
            else if (currentScene == "Map1") { MelonCoroutines.Start(SetupMap1()); }
            yield break;
        }

        private static void RecolorGrassToRandom(GameObject grass)
        {
            MeshRenderer[] renderers = grass.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.material.color = new Color(((float)Main.random.Next(0, 255)) / 255f, ((float)random.Next(0, 255)) / 255f, ((float)random.Next(0, 255)) / 255f);
            }
        }

        private void SetupColliders(GameObject grass)
        {
            grass.layer = 14; //move
            BoxCollider box = grass.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(0.1f, 1f, 0.1f);
            grass.AddComponent<ColliderCheck>();
        }

        private IEnumerator MapStartGrassGrow()
        {
            yield return new WaitForSeconds(0.25f);
            bool isPark = false;
            if (storedGrass == null) { yield break; }
            if (currentScene == "Gym")
            {
                storedGrass.transform.position = new Vector3(0, -0.01f - (0.039f * Preferences.PrefHeight.Value), 0); //position for flatland
            }
            else if (currentScene == "Park")
            {
                isPark = true;
                storedGrass.transform.position = new Vector3(-3.1f, -5.88f - (0.039f * Preferences.PrefHeight.Value), 0.4f);
                storedLowerParkGrass.transform.position = new Vector3(1.5778f, -20.9404f - (0.039f * Preferences.PrefHeight.Value), 4.9215f);
            }
            else if (currentScene == "Map0")
            {
                storedGrass.transform.position = new Vector3(0, -0.23f - (0.039f * Preferences.PrefHeight.Value), 0);
            }
            else if (currentScene == "Map1")
            {
                storedGrass.transform.position = new Vector3(0, -(0.039f * Preferences.PrefHeight.Value), 0);
            }
            else { yield break; }
            storedGrass.SetActive(true);
            if (Preferences.PrefRegrow.Value)
            {
                storedGrass.transform.localScale = new Vector3(1, 0, 1);
                if (isPark)
                {
                    storedLowerParkGrass.transform.localScale = new Vector3(1, 0, 1);
                    storedLowerParkGrass.SetActive(true);
                }
                while ((storedGrass != null) && (storedGrass.transform.localScale.y < 1))
                {
                    try
                    {
                        if (Preferences.PrefGrowthTime.Value == 0) { break; }
                        storedGrass.transform.localScale = new Vector3(1, storedGrass.transform.localScale.y + (1f / (Preferences.PrefGrowthTime.Value * 50f)), 1);
                        if (isPark) { storedLowerParkGrass.transform.localScale = new Vector3(1, storedLowerParkGrass.transform.localScale.y + (1f / (Preferences.PrefGrowthTime.Value * 50f)), 1); }
                    }
                    catch { break; }
                    yield return new WaitForFixedUpdate();
                    if (storedGrass == null) { yield break; }
                }
            }
            else { if (isPark) { storedLowerParkGrass.SetActive(true); } }
            try
            {
                if (storedGrass != null) { storedGrass.transform.localScale = new Vector3(1, 1, 1); }
                if (isPark) { storedLowerParkGrass.transform.localScale = new Vector3(1, 1, 1); }
            }
            catch { }
            yield break;
        }

        private IEnumerator SetupFlatLand()
        {
            yield return new WaitForSeconds(1f);
            GameObject floor = GameObject.Find("Floor");
            flatLandSize = (int)floor.transform.localScale.x / 2;
            for (int i = 0; i < Preferences.PrefUpperParkCount.Value; i++)
            {
                float x = floor.transform.position.x + random.Next(-flatLandSize, flatLandSize + 1);
                if (x < floor.transform.position.x) { x += (float)random.NextDouble(); }
                else if (x > floor.transform.position.x) { x -= (float)random.NextDouble(); }
                else
                {
                    if (random.Next(0, 2) == 0) { x += (float)random.NextDouble(); }
                    else { x -= (float)random.NextDouble(); }
                }
                float z = floor.transform.position.z + random.Next(-flatLandSize, flatLandSize + 1);
                if (z < floor.transform.position.z) { z += (float)random.NextDouble(); }
                else if (z > floor.transform.position.z) { z -= (float)random.NextDouble(); }
                else
                {
                    if (random.Next(0, 2) == 0) { z += (float)random.NextDouble(); }
                    else { z -= (float)random.NextDouble(); }
                }
                Vector3 grassSpot = new Vector3(x, 0, z);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = grassSpot;
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(Preferences.PrefWidth.Value, Preferences.PrefHeight.Value, Preferences.PrefWidth.Value);
                grass.SetActive(true);
                if (i % 100 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            if (currentScene == "Gym"){ MelonCoroutines.Start(MapStartGrassGrow()); } //checks because time passed
            MelonCoroutines.Start(WatchFloorSize(floor));
            yield break;
        }

        private IEnumerator WatchFloorSize(GameObject floor)
        {
            while (floor != null) { yield return new WaitForSeconds(1f); }
            storedGrass.SetActive(false);
            if (flatLandActive) { MelonCoroutines.Start(SetupFlatLand()); }
            yield break;
        }

        private IEnumerator SetupMap0()
        {
            for (int i = 0; i < Preferences.PrefRingCount.Value; i++)
            {
                Vector3 pickedSpot = CalculatePoint(11, -0.31f);
                Vector3 grassSpot = new Vector3(pickedSpot.x, 0, pickedSpot.z);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = grassSpot;
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(Preferences.PrefWidth.Value, Preferences.PrefHeight.Value, Preferences.PrefWidth.Value);
                grass.SetActive(true);
                if (i % 100 == 0) { yield return new WaitForFixedUpdate(); }
            }
            if (currentScene == "Map0") { MelonCoroutines.Start(MapStartGrassGrow()); } //checks because time passed
            yield break;
        }

        private IEnumerator SetupMap1()
        {
            for (int i = 0; i < Preferences.PrefPitCount.Value; i++)
            {
                Vector3 pickedSpot = CalculatePoint(10, -0.03f);
                Vector3 grassSpot = new Vector3(pickedSpot.x * 1.5f, 0, pickedSpot.z * 1.1f);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = grassSpot;
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(Preferences.PrefWidth.Value, Preferences.PrefHeight.Value, Preferences.PrefWidth.Value);
                grass.SetActive(true);
                if (i % 100 == 0) { yield return new WaitForFixedUpdate(); }
            }
            if (currentScene == "Map1") { MelonCoroutines.Start(MapStartGrassGrow()); } //checks because time passed
            yield break;
        }
        
        private IEnumerator SetupParkUpperGrass()
        {
            for (int i = 0; i < Preferences.PrefUpperParkCount.Value; i++)
            {
                Vector3 ringSpot = new Vector3(0f, -5.88f, 0f);
                Vector3 grassSpot = CalculatePoint(11, -0.31f);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = new Vector3(ringSpot.x + grassSpot.x, 0, ringSpot.z + grassSpot.z);
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(Preferences.PrefWidth.Value, Preferences.PrefHeight.Value, Preferences.PrefWidth.Value);
                grass.SetActive(true);
                if (i % 100 == 0) { yield return new WaitForFixedUpdate(); }
            }
            //lower park will trigger regrow
            yield break;
        }

        private IEnumerator SetupParkLowerGrass()
        {
            for (int i = 0; i < Preferences.PrefLowerParkCount.Value; i++)
            {
                Vector3 ringSpot = new Vector3(0f, -5.88f, 0f);
                Vector3 grassSpot = CalculatePoint(25, -0.31f);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedLowerParkGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = new Vector3(ringSpot.x + grassSpot.x, 0, ringSpot.z + grassSpot.z);
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(Preferences.PrefWidth.Value, Preferences.PrefHeight.Value, Preferences.PrefWidth.Value);
                grass.SetActive(true);
                if (i % 100 == 0) { yield return new WaitForFixedUpdate(); }
            }
            if (currentScene == "Park") { MelonCoroutines.Start(MapStartGrassGrow()); } //checks because time passed and runs when both park things are done
            yield break;
        }
    }
}

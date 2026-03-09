using Il2CppRUMBLE.Interactions.InteractionBase;
using Il2CppRUMBLE.MoveSystem;
using MelonLoader;
using RumbleModdingAPI.RMAPI;
using RumbleModUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace GrassEnabler
{
    public static class ModBuildInfo
    {
        public const string Version = "3.0.0";
    }

    public class main : MelonMod
    {
        private string currentScene = "Loader";
        private bool init = false;
        private GameObject grassParent, grassShort, grassLong, grassShortNoCollider, grassLongNoCollider, storedGrass;
        System.Random random = new System.Random();
        private int grassCount;
        private static float grassHeight;
        private static float grassWidth;
        public static bool grassRemoval;
        public static bool grassGrowth;
        public static float preGrowthTime;
        public static int growthTime;
        public static string grassColorInput;
        public static Color grassColor = hexToColor("c0b154");
        private Mod GrassEnabler = new Mod();
        private bool flatLandModFound = false;
        private bool flatLandActive = false;
        private int flatLandSize = 125;

        public override void OnLateInitializeMelon()
        {
            GrassEnabler.ModName = "GrassEnabler";
            GrassEnabler.ModVersion = ModBuildInfo.Version;
            GrassEnabler.SetFolder("GrassEnabler");
            GrassEnabler.AddToList("Grass Count", 5000, "Adds Grass to Maps. Can change Grass Count. Default: 5000", new Tags { });
            GrassEnabler.AddToList("Grass Height", 0.5f, "Changes Grass Height. Default: 0.5", new Tags { });
            GrassEnabler.AddToList("Grass Width", 0.5f, "Changes Grass Width. Default: 0.5", new Tags { });
            GrassEnabler.AddToList("Grass Color", "c0b154", "Sets the Color of the Grass. Supports 0x(HexCode), #(HexCode), FFF, and FFFFFF style inputs. Alpha included. Default: c0b154", new Tags { });
            GrassEnabler.AddToList("Grass Removal", true, 0, "Removes the Grass with Spawned and Grounded Structures", new Tags { });
            GrassEnabler.AddToList("Regrow Grass", true, 0, "Regrows the Grass after it is Destroyed. Default: On", new Tags { });
            GrassEnabler.AddToList("Time Before Regrowth", 5f, "Controls how many Seconds till Grass starts Regrowing. Default: 5", new Tags { });
            GrassEnabler.AddToList("Growth Speed", 10, "Controls how many Seconds till Grass is Fully Grown. Default: 10", new Tags { });
            GrassEnabler.GetFromFile();
            GrassEnabler.ModSaved += Save;
            UI.instance.UI_Initialized += delegate { UI.instance.AddMod(GrassEnabler); };
            Actions.onMapInitialized += Init;
            grassCount = (int)GrassEnabler.Settings[0].SavedValue;
            grassHeight = (float)GrassEnabler.Settings[1].SavedValue;
            grassWidth = (float)GrassEnabler.Settings[2].SavedValue;
            grassColorInput = (string)GrassEnabler.Settings[3].SavedValue;
            grassColor = hexToColor(grassColorInput);
            grassRemoval = (bool)GrassEnabler.Settings[4].SavedValue;
            grassGrowth = (bool)GrassEnabler.Settings[5].SavedValue;
            preGrowthTime = (float)GrassEnabler.Settings[6].SavedValue;
            growthTime = (int)GrassEnabler.Settings[7].SavedValue;
        }

        public static Color hexToColor(string hex)
        {
            try
            {
                hex = hex.Replace("0x", "").Replace("#", "");//in case the string is formatted 0xFFFFFF or #FFFFFF
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
            catch
            {
                return hexToColor("c0b154");
            }
        }

        public void Save()
        {
            int lastGrassCount = grassCount;
            grassCount = (int)GrassEnabler.Settings[0].SavedValue;
            float lastGrassHeight = grassHeight;
            grassHeight = (float)GrassEnabler.Settings[1].SavedValue;
            float lastGrassWidth = grassWidth;
            grassWidth = (float)GrassEnabler.Settings[2].SavedValue;
            string lastGrassColorInput = grassColorInput;
            grassColorInput = (string)GrassEnabler.Settings[3].SavedValue;
            bool lastGrassRemoval = grassRemoval;
            grassRemoval = (bool)GrassEnabler.Settings[4].SavedValue;
            grassGrowth = (bool)GrassEnabler.Settings[5].SavedValue;
            preGrowthTime = (float)GrassEnabler.Settings[6].SavedValue;
            growthTime = (int)GrassEnabler.Settings[7].SavedValue;
            if (lastGrassColorInput != grassColorInput)
            {
                grassColor = hexToColor(grassColorInput);
                List<MeshRenderer> renderers = new List<MeshRenderer>();
                renderers.AddRange(grassLong.transform.GetComponentsInChildren<MeshRenderer>());
                renderers.AddRange(grassShort.transform.GetComponentsInChildren<MeshRenderer>());
                renderers.AddRange(grassLongNoCollider.transform.GetComponentsInChildren<MeshRenderer>());
                renderers.AddRange(grassShortNoCollider.transform.GetComponentsInChildren<MeshRenderer>());
                foreach (MeshRenderer renderer in renderers)
                {
                    renderer.material.color = grassColor;
                }
            }
            if ((storedGrass != null) && ((lastGrassCount != grassCount)
                                        || (lastGrassRemoval != grassRemoval)
                                        || (lastGrassColorInput != grassColorInput)
                                        || (lastGrassHeight != grassHeight)
                                        || (lastGrassWidth != grassWidth)))
            {
                GameObject.DestroyImmediate(storedGrass);
                MelonCoroutines.Start(SetupStoredGrass());
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            currentScene = sceneName;
            if (init)
            {
                storedGrass.SetActive(false);
            }
            flatLandActive = false;
        }

        private void Init(string map)
        {
            if (!init && (map == "Gym"))
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
                foreach (MeshRenderer renderer in renderers)
                {
                    renderer.material.color = grassColor;
                }
                
                GameObject.DontDestroyOnLoad(grassParent);
                grassParent.SetActive(false);
                MelonCoroutines.Start(SetupStoredGrass());
                flatLandModFound = Calls.Mods.findOwnMod("FlatLand", "1.0.0", false);
                init = true;
            }
            if (map == "Gym")
            {
                storedGrass.SetActive(false);
                if (flatLandModFound)
                {
                    MelonCoroutines.Start(InitFlatLandFound());
                }
            }
            else if (map == "Map0") { MelonCoroutines.Start(SetupMap0()); }
            else if (map == "Map1") { MelonCoroutines.Start(SetupMap1()); }
            else if (map == "Park") { MelonCoroutines.Start(SetupPark()); }
        }

        public IEnumerator InitFlatLandFound()
        {
            yield return new WaitForSeconds(1f);
            GameObject.Find("FlatLand/FlatLandButton/Button").GetComponent<InteractionButton>().onPressed.AddListener(new System.Action(() =>
            {
                flatLandActive = true;
                MelonCoroutines.Start(SetupFlatLand());
            }));
            yield break;
        }

        public static IEnumerator RegrowGrass(GameObject grass)
        {
            yield return new WaitForSeconds(preGrowthTime);
            grass.transform.localScale = new Vector3(1, 0, 1);
            grass.active = true;
            while (grass.transform.localScale.y <= grassHeight)
            {
                if (growthTime == 0) { break; }
                grass.transform.localScale = new Vector3(1, grass.transform.localScale.y + (grassHeight / ((float)growthTime * 50)), 1);
                yield return new WaitForFixedUpdate();
            }
            grass.transform.localScale = new Vector3(1, grassHeight, 1);
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
            storedGrass = new GameObject();
            storedGrass.name = "Grass";
            storedGrass.SetActive(false);
            for (int i = 0; i < grassCount; i++)
            {
                int pickedGrass = random.Next(0, 2);
                GameObject whichGrass;
                if (grassRemoval)
                {
                    if (pickedGrass == 0) { whichGrass = grassShort; }
                    else { whichGrass = grassLong; }
                }
                else
                {
                    if (pickedGrass == 0) { whichGrass = grassShortNoCollider; }
                    else { whichGrass = grassLongNoCollider; }
                }
                GameObject grass = GameObject.Instantiate(whichGrass, storedGrass.transform);
                grass.transform.position = new Vector3(0, 0, 0);
                grass.transform.rotation = Quaternion.EulerAngles(0, 0, 0);
                grass.transform.localScale = new Vector3(1, 1, 1);
                if (i % 1000 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            GameObject.DontDestroyOnLoad(storedGrass);
            if (currentScene == "Gym") 
            {
                storedGrass.SetActive(false);
            }
            else if (currentScene == "Map0") { MelonCoroutines.Start(SetupMap0()); }
            else if (currentScene == "Map1") { MelonCoroutines.Start(SetupMap1()); }
            else if (currentScene == "Park") { MelonCoroutines.Start(SetupPark()); }
            yield break;
        }

        private void SetupColliders(GameObject grass)
        {
            grass.layer = 15;
            BoxCollider box = grass.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(0.1f, 0.5f, 0.1f);
            grass.AddComponent<ColliderCheck>();
        }

        private IEnumerator MapStartGrassGrow()
        {
            yield return new WaitForSeconds(0.25f);
            if (currentScene == "Gym")
            {
                storedGrass.transform.position = new Vector3(0, -0.01f - (0.039f * grassHeight), 0);
            }
            else if (currentScene == "Park")
            {
                storedGrass.transform.position = new Vector3(-3.1f, -5.88f - (0.039f * grassHeight), 0.4f);
            }
            else if (currentScene == "Map0")
            {
                storedGrass.transform.position = new Vector3(0, -0.25f - (0.039f * grassHeight), 0);
            }
            else
            {
                storedGrass.transform.position = new Vector3(0, -(0.039f * grassHeight), 0);
            }
            if (grassGrowth)
            {
                storedGrass.transform.localScale = new Vector3(1, 0, 1);
                storedGrass.active = true;
                while (storedGrass.transform.localScale.y < 1)
                {
                    if (growthTime == 0) { break; }
                    storedGrass.transform.localScale = new Vector3(1, storedGrass.transform.localScale.y + (1 / ((float)growthTime * 50)), 1);
                    yield return new WaitForFixedUpdate();
                }
            }
            else
            {
                storedGrass.active = true;
            }
            storedGrass.transform.localScale = new Vector3(1, 1, 1);
            yield break;
        }

        private IEnumerator SetupFlatLand()
        {
            yield return new WaitForSeconds(1f);
            GameObject floor = GameObject.Find("Floor");
            flatLandSize = (int)floor.transform.localScale.x / 2;
            for (int i = 0; i < grassCount; i++)
            {
                float x = floor.transform.position.x + random.Next(-flatLandSize, flatLandSize + 1);
                if (x < floor.transform.position.x)
                {
                    x += (float)random.NextDouble();
                }
                else if (x > floor.transform.position.x)
                {
                    x -= (float)random.NextDouble();
                }
                else
                {
                    if (random.Next(0, 2) == 0)
                    {
                        x += (float)random.NextDouble();
                    }
                    else
                    {
                        x -= (float)random.NextDouble();
                    }
                }
                float z = floor.transform.position.z + random.Next(-flatLandSize, flatLandSize + 1);
                if (z < floor.transform.position.z)
                {
                    z += (float)random.NextDouble();
                }
                else if (z > floor.transform.position.z)
                {
                    z -= (float)random.NextDouble();
                }
                else
                {
                    if (random.Next(0, 2) == 0)
                    {
                        z += (float)random.NextDouble();
                    }
                    else
                    {
                        z -= (float)random.NextDouble();
                    }
                }
                Vector3 grassSpot = new Vector3(x, 0, z);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = grassSpot;
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(1, grassHeight, 1);
                grass.active = true;
                if (i % 100 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            if (currentScene == "Gym")
            {
                MelonCoroutines.Start(MapStartGrassGrow());
            }
            MelonCoroutines.Start(WatchFloorSize(floor));
            yield break;
        }

        private IEnumerator WatchFloorSize(GameObject floor)
        {
            while (floor != null)
            {
                yield return new WaitForSeconds(1f);
            }
            storedGrass.active = false;
            if (flatLandActive)
            {
                MelonCoroutines.Start(SetupFlatLand());
            }
            yield break;
        }

        private IEnumerator SetupMap0()
        {
            for (int i = 0; i < grassCount; i++)
            {
                Vector3 pickedSpot = CalculatePoint(11, -0.31f);
                Vector3 grassSpot = new Vector3(pickedSpot.x, 0, pickedSpot.z);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = grassSpot;
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(grassWidth, grassHeight, grassWidth);
                grass.active = true;
                if (i % 100 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            if (currentScene == "Map0")
            {
                MelonCoroutines.Start(MapStartGrassGrow());
            }
            yield break;
        }

        private IEnumerator SetupMap1()
        {
            for (int i = 0; i < grassCount; i++)
            {
                Vector3 pickedSpot = CalculatePoint(10, -0.03f);
                Vector3 grassSpot = new Vector3(pickedSpot.x * 1.5f, 0, pickedSpot.z * 1.1f);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = grassSpot;
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(1, grassHeight, 1);
                grass.active = true;
                if (i % 100 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            if (currentScene == "Map1")
            {
                MelonCoroutines.Start(MapStartGrassGrow());
            }
            yield break;
        }

        private IEnumerator SetupPark()
        {
            for (int i = 0; i < grassCount; i++)
            {
                Vector3 ringSpot = new Vector3(0f, -5.88f, 0f);
                Vector3 grassSpot = CalculatePoint(11, -0.31f);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = new Vector3(ringSpot.x + grassSpot.x, 0, ringSpot.z + grassSpot.z);
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(1, grassHeight, 1);
                grass.active = true;
                if (i % 100 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            if (currentScene == "Park")
            {
                MelonCoroutines.Start(MapStartGrassGrow());
            }
            yield break;
        }
    }
}

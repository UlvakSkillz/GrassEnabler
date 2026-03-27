using Il2CppRUMBLE.Interactions.InteractionBase;
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
        public const string Version = "3.1.1";
    }
    
    public class main : MelonMod
    {
        private string currentScene = "Loader";
        private bool init = false;
        private GameObject grassParent, grassShort, grassLong, grassShortNoCollider, grassLongNoCollider, storedGrass, storedLowerParkGrass;
        internal static System.Random random = new System.Random();
        private int grassCountRing;
        private int grassCountPit;
        private int grassCountUpperPark;
        private int grassCountLowerPark;
        private int grassCountFlatLand = 0;
        private static float grassHeight;
        private static float grassWidth;
        public static bool grassRemoval;
        public static bool grassGrowth;
        public static float preGrowthTime;
        public static int growthTime;
        public static string grassColorInput;
        public static Color grassColor = hexToColor("c0b154");
        public static bool isRandomColoredGrass;
        private Mod GrassEnabler = new Mod();
        private bool flatLandModFound = false;
        private bool flatLandActive = false;
        private int flatLandSize = 125;

        public override void OnLateInitializeMelon()
        {
            GrassEnabler.ModName = "GrassEnabler";
            GrassEnabler.ModVersion = ModBuildInfo.Version;
            GrassEnabler.SetFolder("GrassEnabler");
            GrassEnabler.AddToList("Ring Grass Count", 5000, "Adds Grass to Ring. Can change Grass Count. Default: 5000", new Tags { });
            GrassEnabler.AddToList("Pit Grass Count", 5000, "Adds Grass to Pit. Can change Grass Count. Default: 5000", new Tags { });
            GrassEnabler.AddToList("Upper Park Grass Count", 5000, "Adds Grass to the Upper Park Area. Can change Grass Count. Default: 5000", new Tags { });
            GrassEnabler.AddToList("Lower Park Grass Count", 5000, "Adds Grass to the Lower Park Area. Can change Grass Count. Default: 5000", new Tags { });
            GrassEnabler.AddToList("Grass Height", 0.5f, "Changes Grass Height. Default: 0.5", new Tags { });
            GrassEnabler.AddToList("Grass Width", 0.5f, "Changes Grass Width. Default: 0.5", new Tags { });
            GrassEnabler.AddToList("Grass Color", "c0b154", "Sets the Color of the Grass. Supports 0x(HexCode), #(HexCode), FFF, and FFFFFF style inputs. Alpha included. Default: c0b154", new Tags { });
            GrassEnabler.AddToList("Random Colored Grass", false, 0, "Recolors each Grass to a Random Color.", new Tags { });
            GrassEnabler.AddToList("Grass Removal", true, 0, "Removes the Grass with Spawned and Grounded Structures", new Tags { });
            GrassEnabler.AddToList("Regrow Grass", true, 0, "Regrows the Grass after it is Destroyed. Default: On", new Tags { });
            GrassEnabler.AddToList("Time Before Regrowth", 5f, "Controls how many Seconds till Grass starts Regrowing. Default: 5", new Tags { });
            GrassEnabler.AddToList("Growth Speed", 10, "Controls how many Seconds till Grass is Fully Grown. Default: 10", new Tags { });
            GrassEnabler.GetFromFile();
            GrassEnabler.ModSaved += Save;
            UI.instance.UI_Initialized += delegate { UI.instance.AddMod(GrassEnabler); };
            Actions.onMapInitialized += MapLoaded;
            grassCountRing = (int)GrassEnabler.Settings[0].SavedValue;
            grassCountPit = (int)GrassEnabler.Settings[1].SavedValue;
            grassCountUpperPark = (int)GrassEnabler.Settings[2].SavedValue;
            grassCountLowerPark = (int)GrassEnabler.Settings[3].SavedValue;
            grassHeight = (float)GrassEnabler.Settings[4].SavedValue;
            grassWidth = (float)GrassEnabler.Settings[5].SavedValue;
            grassColorInput = (string)GrassEnabler.Settings[6].SavedValue;
            grassColor = hexToColor(grassColorInput);
            isRandomColoredGrass = (bool)GrassEnabler.Settings[7].SavedValue;
            grassRemoval = (bool)GrassEnabler.Settings[8].SavedValue;
            grassGrowth = (bool)GrassEnabler.Settings[9].SavedValue;
            preGrowthTime = (float)GrassEnabler.Settings[10].SavedValue;
            growthTime = (int)GrassEnabler.Settings[11].SavedValue;
            Actions.onMyModsGathered += SetupDDOLGrass;
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
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.material.color = grassColor;
            }

            GameObject.DontDestroyOnLoad(grassParent);
            grassParent.SetActive(false);
            flatLandModFound = Calls.Mods.findOwnMod("FlatLand", "1.0.0", false);
            if (flatLandModFound)
            {
                GrassEnabler.AddToList("FlatLand Grass Count", 5000, "Controls how much Grass is in FlatLand.", new Tags { });
                GrassEnabler.GetFromFile();
                grassCountFlatLand = (int)GrassEnabler.Settings[12].SavedValue;
            }
            init = true;
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
            int lastGrassCountRing = grassCountRing;
            grassCountRing = (int)GrassEnabler.Settings[0].SavedValue;
            int lastGrassCountPit = grassCountPit;
            grassCountPit = (int)GrassEnabler.Settings[1].SavedValue;
            int lastGrassCountUpperPark = grassCountUpperPark;
            grassCountUpperPark = (int)GrassEnabler.Settings[2].SavedValue;
            int lastGrassCountLowerPark = grassCountLowerPark;
            grassCountLowerPark = (int)GrassEnabler.Settings[3].SavedValue;
            float lastGrassHeight = grassHeight;
            grassHeight = (float)GrassEnabler.Settings[4].SavedValue;
            float lastGrassWidth = grassWidth;
            grassWidth = (float)GrassEnabler.Settings[5].SavedValue;
            string lastGrassColorInput = grassColorInput;
            grassColorInput = (string)GrassEnabler.Settings[6].SavedValue;
            bool lastIsRandomColoredGrass = isRandomColoredGrass;
            isRandomColoredGrass = (bool)GrassEnabler.Settings[7].SavedValue;
            bool lastGrassRemoval = grassRemoval;
            grassRemoval = (bool)GrassEnabler.Settings[8].SavedValue;
            grassGrowth = (bool)GrassEnabler.Settings[9].SavedValue;
            preGrowthTime = (float)GrassEnabler.Settings[10].SavedValue;
            growthTime = (int)GrassEnabler.Settings[11].SavedValue;
            int lastGrassCountFlatLand = grassCountFlatLand;
            if (flatLandModFound)
            {
                grassCountFlatLand = (int)GrassEnabler.Settings[12].SavedValue;
            }
            if ((lastGrassColorInput != grassColorInput)
                || ((lastIsRandomColoredGrass != isRandomColoredGrass) && (!isRandomColoredGrass)))
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
            if (((lastGrassCountRing != grassCountRing) && (currentScene == "Map0"))
                || ((lastGrassCountPit != grassCountPit) && (currentScene == "Map1"))
                || ((lastGrassCountUpperPark != grassCountUpperPark) && (currentScene == "Park"))
                || ((lastGrassCountLowerPark != grassCountLowerPark) && (currentScene == "Park"))
                || ((lastGrassCountFlatLand != grassCountFlatLand) && (currentScene == "Gym"))
                || (lastGrassRemoval != grassRemoval)
                || (lastGrassColorInput != grassColorInput)
                || (lastGrassHeight != grassHeight)
                || (lastGrassWidth != grassWidth)
                || (lastIsRandomColoredGrass != isRandomColoredGrass))
            {
                if (storedGrass != null)
                {
                    GameObject.DestroyImmediate(storedGrass);
                }
                if (storedLowerParkGrass != null)
                {
                    GameObject.DestroyImmediate(storedLowerParkGrass);
                }
                MelonCoroutines.Start(SetupStoredGrass());
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            currentScene = sceneName;
            flatLandActive = false;
            if (flatLandModFound && (currentScene == "Gym"))
            {
                MelonCoroutines.Start(InitFlatLandFound());
            }
        }

        private void MapLoaded(string map)
        {
            if (!init) { return; }
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
            yield return new WaitForSeconds(preGrowthTime);
            if (grass == null) { yield break; }
            grass.transform.localScale = new Vector3(grassWidth, 0, grassWidth);
            grass.SetActive(true);
            while (grass.transform.localScale.y <= grassHeight)
            {
                try
                {
                    if (growthTime == 0) { break; }
                    grass.transform.localScale = new Vector3(grassWidth, grass.transform.localScale.y + (grassHeight / ((float)growthTime * 50)), grassWidth);
                } catch { yield break; }
                yield return new WaitForFixedUpdate();
                if (grass == null) { yield break; }
            }
            if (grass != null)
            {
                grass.transform.localScale = new Vector3(grassWidth, grassHeight, grassWidth);
            }
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
            if (storedGrass != null)
            {
                GameObject.Destroy(storedGrass);
            }
            storedGrass = new GameObject();
            storedGrass.name = "Grass";
            storedGrass.SetActive(false);

            for (int i = 0; i < (currentScene == "Map0" ? grassCountRing : (currentScene == "Map1" ? grassCountPit : (currentScene == "Park" ? grassCountUpperPark : /*Gym*/grassCountFlatLand))); i++)
            {
                int pickedGrass = random.Next(0, 2);
                GameObject whichGrass = (grassRemoval ? ((pickedGrass == 0) ? grassShort : grassLong) : ((pickedGrass == 0) ? grassShortNoCollider : grassLongNoCollider));
                GameObject grass = GameObject.Instantiate(whichGrass, storedGrass.transform);
                if (isRandomColoredGrass)
                {
                    RecolorGrassToRandom(grass);
                }
                grass.transform.position = new Vector3(0, 0, 0);
                grass.transform.rotation = Quaternion.EulerAngles(0, 0, 0);
                grass.transform.localScale = new Vector3(grassWidth, grassHeight, grassWidth);
                if (i % 1000 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            if (flatLandActive)
            {
                MelonCoroutines.Start(SetupFlatLand());
            }
            else if (currentScene == "Park")
            {
                if (storedLowerParkGrass != null)
                {
                    GameObject.Destroy(storedLowerParkGrass);
                }
                storedLowerParkGrass = new GameObject();
                storedLowerParkGrass.name = "Grass Lower Park";
                storedLowerParkGrass.SetActive(false);
                for (int i = 0; i < grassCountLowerPark; i++)
                {
                    int pickedGrass = random.Next(0, 2);
                    GameObject whichGrass;
                    if (grassRemoval)
                    {
                        whichGrass = (pickedGrass == 0) ? grassShort : grassLong;
                    }
                    else
                    {
                        whichGrass = (pickedGrass == 0) ? grassShortNoCollider : grassLongNoCollider;
                    }
                    GameObject grass = GameObject.Instantiate(whichGrass, storedLowerParkGrass.transform);
                    grass.transform.position = new Vector3(0, 0, 0);
                    grass.transform.rotation = Quaternion.EulerAngles(0, 0, 0);
                    grass.transform.localScale = new Vector3(grassWidth, grassHeight, grassWidth);
                    if (i % 1000 == 0)
                    {
                        yield return new WaitForFixedUpdate();
                    }
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
                renderer.material.color = new Color(((float)main.random.Next(0, 255)) / 255f, ((float)random.Next(0, 255)) / 255f, ((float)random.Next(0, 255)) / 255f);
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
                storedGrass.transform.position = new Vector3(0, -0.01f - (0.039f * grassHeight), 0); //position for flatland
            }
            else if (currentScene == "Park")
            {
                isPark = true;
                storedGrass.transform.position = new Vector3(-3.1f, -5.88f - (0.039f * grassHeight), 0.4f);
                storedLowerParkGrass.transform.position = new Vector3(1.5778f, -20.9404f - (0.039f * grassHeight), 4.9215f);
            }
            else if (currentScene == "Map0")
            {
                storedGrass.transform.position = new Vector3(0, -0.23f - (0.039f * grassHeight), 0);
            }
            else //Map1
            {
                storedGrass.transform.position = new Vector3(0, -(0.039f * grassHeight), 0);
            }
            storedGrass.SetActive(true);
            if (grassGrowth)
            {
                storedGrass.transform.localScale = new Vector3(1, 0, 1);
                if (isPark)
                {
                    storedLowerParkGrass.transform.localScale = new Vector3(1, 0, 1);
                    storedLowerParkGrass.SetActive(true);
                }
                while (storedGrass.transform.localScale.y < 1)
                {
                    if (growthTime == 0) { break; }
                    storedGrass.transform.localScale = new Vector3(1, storedGrass.transform.localScale.y + (1 / ((float)growthTime * 50)), 1);
                    if (isPark)
                    {
                        storedLowerParkGrass.transform.localScale = new Vector3(1, storedLowerParkGrass.transform.localScale.y + (1 / ((float)growthTime * 50)), 1);
                    }
                    yield return new WaitForFixedUpdate();
                    if (storedGrass == null) { yield break; }
                }
            }
            else
            {
                if (isPark)
                {
                    storedLowerParkGrass.SetActive(true);
                }
            }
            storedGrass.transform.localScale = new Vector3(1, 1, 1);
            if (isPark)
            {
                storedLowerParkGrass.transform.localScale = new Vector3(1, 1, 1);
            }
            yield break;
        }

        private IEnumerator SetupFlatLand()
        {
            yield return new WaitForSeconds(1f);
            GameObject floor = GameObject.Find("Floor");
            flatLandSize = (int)floor.transform.localScale.x / 2;
            for (int i = 0; i < grassCountUpperPark; i++)
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
                grass.transform.localScale = new Vector3(grassWidth, grassHeight, grassWidth);
                grass.SetActive(true);
                if (i % 100 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            if (currentScene == "Gym") //checks because time passed
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
            storedGrass.SetActive(false);
            if (flatLandActive)
            {
                MelonCoroutines.Start(SetupFlatLand());
            }
            yield break;
        }

        private IEnumerator SetupMap0()
        {
            for (int i = 0; i < grassCountRing; i++)
            {
                Vector3 pickedSpot = CalculatePoint(11, -0.31f);
                Vector3 grassSpot = new Vector3(pickedSpot.x, 0, pickedSpot.z);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = grassSpot;
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(grassWidth, grassHeight, grassWidth);
                grass.SetActive(true);
                if (i % 100 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            if (currentScene == "Map0") //checks because time passed
            {
                MelonCoroutines.Start(MapStartGrassGrow());
            }
            yield break;
        }

        private IEnumerator SetupMap1()
        {
            for (int i = 0; i < grassCountPit; i++)
            {
                Vector3 pickedSpot = CalculatePoint(10, -0.03f);
                Vector3 grassSpot = new Vector3(pickedSpot.x * 1.5f, 0, pickedSpot.z * 1.1f);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = grassSpot;
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(grassWidth, grassHeight, grassWidth);
                grass.SetActive(true);
                if (i % 100 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            if (currentScene == "Map1") //checks because time passed
            {
                MelonCoroutines.Start(MapStartGrassGrow());
            }
            yield break;
        }
        
        private IEnumerator SetupParkUpperGrass()
        {
            for (int i = 0; i < grassCountUpperPark; i++)
            {
                Vector3 ringSpot = new Vector3(0f, -5.88f, 0f);
                Vector3 grassSpot = CalculatePoint(11, -0.31f);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = new Vector3(ringSpot.x + grassSpot.x, 0, ringSpot.z + grassSpot.z);
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(grassWidth, grassHeight, grassWidth);
                grass.SetActive(true);
                if (i % 100 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            yield break;
        }

        private IEnumerator SetupParkLowerGrass()
        {
            for (int i = 0; i < grassCountLowerPark; i++)
            {
                Vector3 ringSpot = new Vector3(0f, -5.88f, 0f);
                Vector3 grassSpot = CalculatePoint(25, -0.31f);
                Quaternion grassRotation = Quaternion.EulerAngles(0, random.Next(0, 361), 0);
                GameObject grass = storedLowerParkGrass.transform.GetChild(i).gameObject;
                grass.transform.localPosition = new Vector3(ringSpot.x + grassSpot.x, 0, ringSpot.z + grassSpot.z);
                grass.transform.rotation = grassRotation;
                grass.transform.localScale = new Vector3(grassWidth, grassHeight, grassWidth);
                grass.SetActive(true);
                if (i % 100 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            if (currentScene == "Park") //checks because time passed and runs when both park things are done
            {
                MelonCoroutines.Start(MapStartGrassGrow());
            }
            yield break;
        }
    }
}

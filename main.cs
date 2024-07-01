using MelonLoader;
using System;
using UnityEngine;
using RumbleModdingAPI;
using System.Collections;
using RumbleModUI;
using Il2CppRUMBLE.MoveSystem;
using Il2CppRUMBLE.Managers;
using NAudio.Wave;
using MelonLoader.Utils;

namespace GrassEnabler
{
    [RegisterTypeInIl2Cpp]
    public class ColliderCheck : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (!main.grassRemoval)
            {
                return;
            }
            Structure structure;
            if (other.name != "Mower Base")
            {
                if ((other.transform.gameObject.name != "LargeRock") && (other.transform.gameObject.name != "SmallRock"))
                {
                    structure = other.transform.gameObject.transform.parent.GetComponent<Structure>();
                }
                else
                {
                    structure = other.transform.gameObject.GetComponent<Structure>();
                }
                if (structure == null)
                {
                    return;
                }
                if (structure.IsGrounded || structure.IsSpawning)
                {
                    this.transform.gameObject.active = false;
                    if (main.grassGrowth)
                    {
                        MelonCoroutines.Start(main.RegrowGrass(this.transform.gameObject));
                    }
                }
            }
            else
            {
                this.transform.gameObject.active = false;
                if (main.grassGrowth)
                {
                    MelonCoroutines.Start(main.RegrowGrass(this.transform.gameObject));
                }
            }
        }
    }

    [RegisterTypeInIl2Cpp]
    public class HandColliderCheck : MonoBehaviour
    {
        bool leftHandIn = false;
        bool rightHandIn = false;
        
        void OnTriggerEnter(Collider other)
        {
            if ((other.gameObject.name != "Bone_Hand_L") && (other.gameObject.name != "Bone_Hand_R"))
            {
                return;
            }
            if ((other.gameObject.name == "Bone_Hand_L") && (IsWithinRotation(other.gameObject.transform.localEulerAngles, new Vector3(345f, 305f, 350f))))
            {
                leftHandIn = true;
            }
            if ((other.gameObject.name == "Bone_Hand_R") && (IsWithinRotation(other.gameObject.transform.localEulerAngles, new Vector3(350f, 50f, 25f))))
            {
                rightHandIn = true;
            }
            if (leftHandIn && rightHandIn)
            {
                int i = 2;
                if (other.gameObject.transform.parent.parent.parent.parent.parent.parent.parent.parent.parent.GetChild(2).gameObject.name == "Health")
                {
                    i = 3;
                    if (!main.LawnMowerOthersActive)
                    {
                        return;
                    }
                }
                other.gameObject.transform.parent.parent.parent.parent.parent.parent.parent.parent.parent.GetChild(i).GetChild(13).GetChild(0).GetChild(0).GetChild(1).GetChild(0).gameObject.active = true;
                if (main.playMowerSound && (i == 2))
                {
                    main.PlaySoundIfFileExists(@"\GrassEnabler\LawnMower.mp3");
                }
            }
        }

        private bool IsWithinRotation(Vector3 angle, Vector3 pose)
        {
            Vector3 handRot, poseRot;
            float acceptance = 45f;
            handRot = angle;
            poseRot = pose;
            if (angle.x - acceptance < 0)
            {
                poseRot.x += 180;
                handRot.x += 180;
            }
            else if (angle.x + acceptance > 360)
            {
                poseRot.x -= 180;
                handRot.x -= 180;
            }
            if (angle.y - acceptance < 0)
            {
                poseRot.y += 180;
                handRot.y += 180;
            }
            else if (angle.y + acceptance > 360)
            {
                poseRot.y -= 180;
                handRot.y -= 180;
            }
            if (angle.z - acceptance < 0)
            {
                poseRot.z += 180;
                handRot.z += 180;
            }
            else if (angle.z + acceptance > 360)
            {
                poseRot.z -= 180;
                handRot.z -= 180;
            }
            if (poseRot.x < 0)
            {
                poseRot.x += 360;
            }
            else if (poseRot.x > 360)
            {
                poseRot.x -= 360;
            }
            if (poseRot.y < 0)
            {
                poseRot.y += 360;
            }
            else if (poseRot.y > 360)
            {
                poseRot.y -= 360;
            }
            if (poseRot.z < 0)
            {
                poseRot.z += 360;
            }
            else if (poseRot.z > 360)
            {
                poseRot.z -= 360;
            }
            if ((handRot.x + acceptance > poseRot.x) && (handRot.x - acceptance < poseRot.x) &&
                (handRot.y + acceptance > poseRot.y) && (handRot.y - acceptance < poseRot.y) &&
                (handRot.z + acceptance > poseRot.z) && (handRot.z - acceptance < poseRot.z))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if ((other.gameObject.name != "Bone_Hand_L") && (other.gameObject.name != "Bone_Hand_R"))
            {
                return;
            }
            if (other.gameObject.name == "Bone_Hand_L")
            {
                leftHandIn = false;
            }
            if (other.gameObject.name == "Bone_Hand_R")
            {
                rightHandIn = false;
            }
            if (!leftHandIn || !rightHandIn)
            {
                int i = 2;
                if (other.gameObject.transform.parent.parent.parent.parent.parent.parent.parent.parent.parent.GetChild(2).gameObject.name == "Health")
                {
                    i = 3;
                }
                other.gameObject.transform.parent.parent.parent.parent.parent.parent.parent.parent.parent.GetChild(i).GetChild(13).GetChild(0).GetChild(0).GetChild(1).GetChild(0).gameObject.active = false;
            }
        }
    }

    public class main : MelonMod
    {
        private string currentScene = "Loader";
        private bool init = false;
        private GameObject grassParent, grassShort, grassLong, storedGrass;
        System.Random random = new System.Random();
        private int grassCount;
        private static float grassHeight;
        public static bool grassRemoval;
        public static bool grassGrowth;
        public static float preGrowthTime;
        public static int growthTime;
        public static bool playMowerSound;
        private GameObject lawnMower;
        public static GameObject lawnMowerScene;
        public static GameObject[] lawnMowerSceneOther;
        private bool LawnMowerActive = true;
        public static bool LawnMowerOthersActive = true;
        public static bool LawnMowerIsActive = false;
        public static bool mowerSoundPlaying = false;
        public bool spawningOthers = false;
        private int playerCount;
        UI UI = UI.instance;
        private Mod GrassEnabler = new Mod();

        public override void OnLateInitializeMelon()
        {
            GrassEnabler.ModName = "GrassEnabler";
            GrassEnabler.ModVersion = "2.8.2";
            GrassEnabler.SetFolder("GrassEnabler");
            GrassEnabler.AddToList("Grass Count", 5000, "Adds Grass to Maps. Can change Grass Count", new Tags { });
            GrassEnabler.AddToList("Grass Height", 1f, "Changes Grass Height", new Tags { });
            GrassEnabler.AddToList("Grass Removal", true, 0, "Removes the Grass with Spawned and Grounded Structures", new Tags { });
            GrassEnabler.AddToList("Regrow Grass", true, 0, "Regrows the Grass after it is Destroyed", new Tags { });
            GrassEnabler.AddToList("Time Before Regrowth", 5f, "Controls how many Seconds till Grass starts Regrowing", new Tags { });
            GrassEnabler.AddToList("Growth Speed", 10, "Controls how many Seconds till Grass is Fully Grown", new Tags { });
            GrassEnabler.AddToList("Lawn Mower Active", true, 0, "Summons the Lawn Mower (Both Palms Facing the Floor, Fingers Pointing Forwards)", new Tags { });
            GrassEnabler.AddToList("Lawn Mower For Others", true, 0, "Summons the Lawn Mower for Others when they do the pose", new Tags { });
            GrassEnabler.AddToList("Play Mower Sound", true, 0, "Toggle for Enabling Lawn Mower Sound", new Tags { });
            GrassEnabler.GetFromFile();
            GrassEnabler.ModSaved += Save;
            UI.instance.UI_Initialized += UIInit;
            Calls.onMapInitialized += Init;
            grassCount = (int)GrassEnabler.Settings[0].SavedValue;
            grassHeight = (float)GrassEnabler.Settings[1].SavedValue;
            grassRemoval = (bool)GrassEnabler.Settings[2].SavedValue;
            grassGrowth = (bool)GrassEnabler.Settings[3].SavedValue;
            preGrowthTime = (float)GrassEnabler.Settings[4].SavedValue;
            growthTime = (int)GrassEnabler.Settings[5].SavedValue;
            LawnMowerActive = (bool)GrassEnabler.Settings[6].SavedValue;
            LawnMowerOthersActive = (bool)GrassEnabler.Settings[7].SavedValue;
            playMowerSound = (bool)GrassEnabler.Settings[8].SavedValue;
        }

        public void UIInit()
        {
            UI.AddMod(GrassEnabler);
        }

        public void Save()
        {
            int lastGrassCount = grassCount;
            grassCount = (int)GrassEnabler.Settings[0].SavedValue;
            grassHeight = (float)GrassEnabler.Settings[1].SavedValue;
            grassRemoval = (bool)GrassEnabler.Settings[2].SavedValue;
            grassGrowth = (bool)GrassEnabler.Settings[3].SavedValue;
            preGrowthTime = (float)GrassEnabler.Settings[4].SavedValue;
            growthTime = (int)GrassEnabler.Settings[5].SavedValue;
            LawnMowerActive = (bool)GrassEnabler.Settings[6].SavedValue;
            LawnMowerOthersActive = (bool)GrassEnabler.Settings[7].SavedValue;
            playMowerSound = (bool)GrassEnabler.Settings[8].SavedValue;
            if (lawnMowerScene != null)
            {
                MelonCoroutines.Start(SpawnLawnMower(false));
            }
            if ((lastGrassCount != grassCount) && (storedGrass != null))
            {
                GameObject.DestroyImmediate(storedGrass);
                MelonCoroutines.Start(SetupStoredGrass());
            }
            if (init)
            {
                storedGrass.active = false;
                if (currentScene == "Gym")
                {
                    Calls.GameObjects.Gym.Scene.Grass.GetGameObject().SetActive(true);
                }
                else
                {
                    if (currentScene == "Map0") { MelonCoroutines.Start(SetupMap0()); }
                    else if (currentScene == "Map1") { MelonCoroutines.Start(SetupMap1()); }
                    else if (currentScene == "Park") { MelonCoroutines.Start(SetupPark()); }
                    if (LawnMowerActive)
                    {
                        MelonCoroutines.Start(SpawnLawnMower(LawnMowerActive));
                    }
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            currentScene = sceneName;
            if (init)
            {
                storedGrass.active = false;
            }
            LawnMowerIsActive = false;
        }

        public override void OnFixedUpdate()
        {
            if (lawnMowerScene != null)
            {
                lawnMowerScene.transform.rotation = Quaternion.Euler(0, PlayerManager.instance.localPlayer.Controller.gameObject.transform.GetChild(1).GetChild(0).GetChild(0).rotation.eulerAngles.y + 90f, 0);

                if (!spawningOthers && (playerCount != PlayerManager.instance.AllPlayers.Count - 1))
                {
                    for (int i = 0; i < lawnMowerSceneOther.Length; i++)
                    {
                        if (lawnMowerSceneOther[i] != null)
                        {
                            GameObject.DestroyImmediate(lawnMowerSceneOther[i]);
                        }
                    }
                    MelonCoroutines.Start(SpawnLawnMowerOther());
                }
                if (lawnMowerSceneOther != null)
                for (int i = 0; i < lawnMowerSceneOther.Length; i++)
                {
                    if (lawnMowerSceneOther[i] != null)
                    {
                        lawnMowerSceneOther[i].transform.rotation = Quaternion.Euler(0, PlayerManager.instance.AllPlayers[i + 1].Controller.gameObject.transform.GetChild(1).GetChild(0).GetChild(0).rotation.eulerAngles.y + 90f, 0);
                    }
                }
            }
        }

        private void Init()
        {
            if (!init && (currentScene == "Gym"))
            {
                grassParent = new GameObject();
                grassLong = GameObject.Instantiate(Calls.GameObjects.Gym.Scene.Grass.GetGameObject().transform.GetChild(0).gameObject, new Vector3(0, 0, 0), Quaternion.EulerAngles(0, 0, 0));
                grassShort = GameObject.Instantiate(Calls.GameObjects.Gym.Scene.Grass.GetGameObject().transform.GetChild(4).gameObject, new Vector3(0, 0, 0), Quaternion.EulerAngles(0, 0, 0));
                grassParent.name = "GrassParent";
                grassLong.name = "GrassLong";
                grassShort.name = "GrassShort";
                grassLong.transform.parent = grassParent.transform;
                grassShort.transform.parent = grassParent.transform;
                SetupColliders(grassLong);
                SetupColliders(grassShort);
                GameObject.DontDestroyOnLoad(grassParent);
                grassParent.active = false;
                MelonCoroutines.Start(SetupStoredGrass());
                BuildLawnMower();
                init = true;
            }
            if (init)
            {
                if (currentScene == "Gym") 
                {
                    storedGrass.active = false;
                    Calls.GameObjects.Gym.Scene.Grass.GetGameObject().SetActive(true);
                }
                else
                {
                    if (currentScene == "Map0") { MelonCoroutines.Start(SetupMap0()); }
                    else if (currentScene == "Map1") { MelonCoroutines.Start(SetupMap1()); }
                    else if (currentScene == "Park") { MelonCoroutines.Start(SetupPark()); }
                    if (LawnMowerActive)
                    {
                        MelonCoroutines.Start(SpawnLawnMower(!LawnMowerIsActive));
                        MelonCoroutines.Start(SpawnLawnMowerOther());
                    }
                }
            }
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
            storedGrass.active = false;
            for (int i = 0; i < grassCount; i++)
            {
                int pickedGrass = random.Next(0, 2);
                GameObject whichGrass;
                if (pickedGrass == 0) { whichGrass = grassShort; }
                else { whichGrass = grassLong; }
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
                storedGrass.active = false;
                Calls.GameObjects.Gym.Scene.Grass.GetGameObject().SetActive(true);
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
                yield break;
            }
            storedGrass.active = true;
            if (currentScene == "Park")
            {
                storedGrass.transform.position = new Vector3(0, Calls.GameObjects.Park.Scene.Park.MainStaticGroup.Arenas.GymArena0.RingClamp.GetGameObject().transform.position.y - (0.039f * grassHeight), 0);
            }
            else if (currentScene == "Map0")
            {
                storedGrass.transform.position = new Vector3(0, -0.27f - (0.039f * grassHeight), 0);
            }
            else
            {
                storedGrass.transform.position = new Vector3(0, -0.01f - (0.039f * grassHeight), 0);
            }
            if (grassGrowth)
            {
                storedGrass.transform.localScale = new Vector3(1, 0, 1);
                while (storedGrass.transform.localScale.y < 1)
                {
                    if (growthTime == 0) { break; }
                    storedGrass.transform.localScale = new Vector3(1, storedGrass.transform.localScale.y + (1 / ((float)growthTime * 50)), 1);
                    yield return new WaitForFixedUpdate();
                }
            }
            storedGrass.transform.localScale = new Vector3(1, 1, 1);
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
                grass.transform.localScale = new Vector3(1, grassHeight, 1);
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
                Vector3 ringSpot = Calls.GameObjects.Park.Scene.Park.MainStaticGroup.Arenas.GymArena0.RingClamp.GetGameObject().transform.position;
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

        private void BuildLawnMower()
        {
            lawnMower = new GameObject();
            lawnMower.name = "LawnMower";

            GameObject mowerHandleTrigger = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mowerHandleTrigger.name = "Mower Handle Trigger";
            mowerHandleTrigger.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            mowerHandleTrigger.transform.parent = lawnMower.transform;
            mowerHandleTrigger.transform.localScale = new Vector3(0.22f, 0.4f, 0.22f);
            mowerHandleTrigger.transform.localPosition = new Vector3(-0.2f, 0.85f, 0f);
            mowerHandleTrigger.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            mowerHandleTrigger.layer = 22;
            mowerHandleTrigger.GetComponent<CapsuleCollider>().isTrigger = true;
            mowerHandleTrigger.AddComponent<HandColliderCheck>();
            Component.DestroyImmediate(mowerHandleTrigger.GetComponent<MeshRenderer>());
            Component.DestroyImmediate(mowerHandleTrigger.GetComponent<MeshFilter>());

            GameObject lawnMowerOffset2 = new GameObject();
            lawnMowerOffset2.name = "Mower Offset";
            lawnMowerOffset2.transform.parent = lawnMower.transform;
            lawnMowerOffset2.transform.localScale = new Vector3(1 / 0.11f + 0.3f, 1 / 0.25f, 1 / 0.11f);
            lawnMowerOffset2.transform.localPosition = new Vector3(-3.4f, 0f, 0f);
            lawnMowerOffset2.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            GameObject lawnMowerToggle = new GameObject();
            lawnMowerToggle.name = "Lawn Mower Toggle";
            lawnMowerToggle.transform.parent = lawnMowerOffset2.transform;
            lawnMowerToggle.transform.localPosition = new Vector3(0.23f, 0f, 0f);
            lawnMowerToggle.active = false;

            GameObject mowerBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mowerBase.name = "Mower Base";
            mowerBase.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            mowerBase.GetComponent<Renderer>().material.color = new Color(0f, 1f, 0f);
            mowerBase.transform.parent = lawnMowerToggle.transform;
            mowerBase.transform.localPosition = new Vector3(0f, 0f, 0f);
            mowerBase.transform.localScale = new Vector3(1f, 0.2f, 0.75f);
            mowerBase.GetComponent<BoxCollider>().isTrigger = true;
            mowerBase.layer = 14;

            GameObject mowerMotor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mowerMotor.name = "Mower Motor";
            mowerMotor.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            mowerMotor.GetComponent<Renderer>().material.color = new Color(0.75f, 0.75f, 0.75f);
            mowerMotor.transform.parent = lawnMowerToggle.transform;
            mowerMotor.transform.localScale = new Vector3(0.4f, 0.2f, 0.4f);
            mowerMotor.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            Component.Destroy(mowerMotor.GetComponent<BoxCollider>());

            GameObject mowerWheel1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mowerWheel1.name = "Mower Wheel 1";
            mowerWheel1.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            mowerWheel1.GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f);
            mowerWheel1.transform.parent = lawnMowerToggle.transform;
            mowerWheel1.transform.localScale = new Vector3(0.25f, 0.02f, 0.25f);
            mowerWheel1.transform.localPosition = new Vector3(-0.4f, 0f, -0.4f);
            mowerWheel1.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            Component.Destroy(mowerWheel1.GetComponent<CapsuleCollider>());

            GameObject mowerWheel2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mowerWheel2.name = "Mower Wheel 2";
            mowerWheel2.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            mowerWheel2.GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f);
            mowerWheel2.transform.parent = lawnMowerToggle.transform;
            mowerWheel2.transform.localScale = new Vector3(0.25f, 0.02f, 0.25f);
            mowerWheel2.transform.localPosition = new Vector3(-0.4f, 0f, 0.4f);
            mowerWheel2.transform.rotation = Quaternion.Euler(90f, 0, 0);
            Component.Destroy(mowerWheel2.GetComponent<CapsuleCollider>());

            GameObject mowerWheel3 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mowerWheel3.name = "Mower Motor 3";
            mowerWheel3.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            mowerWheel3.GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f);
            mowerWheel3.transform.parent = lawnMowerToggle.transform;
            mowerWheel3.transform.localScale = new Vector3(0.25f, 0.02f, 0.25f);
            mowerWheel3.transform.localPosition = new Vector3(0.4f, 0f, -0.4f);
            mowerWheel3.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Component.Destroy(mowerWheel3.GetComponent<CapsuleCollider>());

            GameObject mowerWheel4 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mowerWheel4.name = "Mower Motor 4";
            mowerWheel4.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            mowerWheel4.GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f);
            mowerWheel4.transform.parent = lawnMowerToggle.transform;
            mowerWheel4.transform.localScale = new Vector3(0.25f, 0.02f, 0.25f);
            mowerWheel4.transform.localPosition = new Vector3(0.4f, 0f, 0.4f);
            mowerWheel4.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Component.Destroy(mowerWheel4.GetComponent<CapsuleCollider>());

            GameObject mowerHandleL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mowerHandleL.name = "Mower Handle Left";
            mowerHandleL.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            mowerHandleL.GetComponent<Renderer>().material.color = new Color(0.75f, 0.75f, 0.75f);
            mowerHandleL.transform.parent = lawnMowerToggle.transform;
            mowerHandleL.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
            mowerHandleL.transform.localPosition = new Vector3(0.7f, 0.4f, -0.3f);
            mowerHandleL.transform.localRotation = Quaternion.Euler(0f, 0f, 315f);
            Component.Destroy(mowerHandleL.GetComponent<CapsuleCollider>());

            GameObject mowerHandleR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mowerHandleR.name = "Mower Handle Right";
            mowerHandleR.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            mowerHandleR.GetComponent<Renderer>().material.color = new Color(0.75f, 0.75f, 0.75f);
            mowerHandleR.transform.parent = lawnMowerToggle.transform;
            mowerHandleR.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
            mowerHandleR.transform.localPosition = new Vector3(0.7f, 0.4f, 0.3f);
            mowerHandleR.transform.localRotation = Quaternion.Euler(0f, 0f, 315f);
            Component.Destroy(mowerHandleR.GetComponent<CapsuleCollider>());

            GameObject mowerHandleCenter = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mowerHandleCenter.name = "Mower Handle Center";
            mowerHandleCenter.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            mowerHandleCenter.GetComponent<Renderer>().material.color = new Color(0.75f, 0.75f, 0.75f);
            mowerHandleCenter.transform.parent = lawnMowerToggle.transform;
            mowerHandleCenter.transform.localScale = new Vector3(0.1f, 0.35f, 0.1f);
            mowerHandleCenter.transform.localPosition = new Vector3(1.05f, 0.75f, 0f);
            mowerHandleCenter.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Component.Destroy(mowerHandleCenter.GetComponent<CapsuleCollider>());

            GameObject mowerHandleCenterGrip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mowerHandleCenterGrip.name = "Mower Handle Center Grip";
            mowerHandleCenterGrip.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            mowerHandleCenterGrip.GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f);
            mowerHandleCenterGrip.transform.parent = lawnMowerToggle.transform;
            mowerHandleCenterGrip.transform.localScale = new Vector3(0.11f, 0.25f, 0.11f);
            mowerHandleCenterGrip.transform.localPosition = new Vector3(1.05f, 0.75f, 0f);
            mowerHandleCenterGrip.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Component.Destroy(mowerHandleCenterGrip.GetComponent<CapsuleCollider>());

            lawnMower.active = false;
            GameObject.DontDestroyOnLoad(lawnMower);
        }

        private IEnumerator SpawnLawnMower(bool activate)
        {
            if (activate)
            {
                yield return new WaitForSeconds(1f);
                if (currentScene == "Gym")
                {
                    yield break;
                }
                LawnMowerIsActive = true;
                lawnMowerScene = GameObject.Instantiate(lawnMower);
                lawnMowerScene.transform.parent = PlayerManager.instance.localPlayer.Controller.gameObject.transform.GetChild(2).GetChild(13).GetChild(0);
                lawnMowerScene.transform.localPosition = new Vector3(0f, 0f, 0f);
                lawnMowerScene.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                lawnMowerScene.active = true;
            }
            else
            {
                GameObject.DestroyImmediate(lawnMowerScene);
                LawnMowerIsActive = false;
            }
            yield break;
        }

        private IEnumerator SpawnLawnMowerOther()
        {
            spawningOthers = true;
            yield return new WaitForSeconds(1f);
            if (currentScene == "Gym")
            {
                yield break;
            }
            playerCount = PlayerManager.instance.AllPlayers.Count - 1;
            lawnMowerSceneOther = new GameObject[playerCount];
            for (int i = 0; i < PlayerManager.instance.AllPlayers.Count - 1; i++)
            {
                lawnMowerSceneOther[i] = GameObject.Instantiate(lawnMower);
                lawnMowerSceneOther[i].transform.parent = PlayerManager.instance.AllPlayers[i + 1].Controller.gameObject.transform.GetChild(3).GetChild(13).GetChild(0);
                lawnMowerSceneOther[i].transform.localPosition = new Vector3(0f, 0f, 0f);
                lawnMowerSceneOther[i].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                lawnMowerSceneOther[i].active = true;
            }
            spawningOthers = false;
            yield break;
        }

        private static IEnumerator PlaySound(string FilePath)
        {
            mowerSoundPlaying = true;
            var reader = new Mp3FileReader(FilePath);
            var waveOut = new WaveOutEvent();
            waveOut.Init(reader);
            waveOut.Play();
            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                if ((lawnMowerScene == null) || !playMowerSound || !lawnMowerScene.transform.GetChild(1).GetChild(0).gameObject.active)
                {
                    waveOut.Stop();
                    break;
                }
                yield return new WaitForFixedUpdate();
            }
            mowerSoundPlaying = false;
            reader.Dispose();
            waveOut.Dispose();
            if ((lawnMowerScene != null) && lawnMowerScene.transform.GetChild(1).GetChild(0).gameObject.active)
            {
                MelonCoroutines.Start(PlaySound(FilePath));
            }
            yield break;
        }

        //Plays the File Sound if it Exists
        public static void PlaySoundIfFileExists(string soundFilePath)
        {
            //Check if the sound file exists
            if (System.IO.File.Exists(MelonEnvironment.UserDataDirectory + soundFilePath))
            {
                //Ensure that only one sound is playing at a time
                if (mowerSoundPlaying)
                {
                    return;
                }
                try
                {
                    MelonCoroutines.Start(PlaySound(MelonEnvironment.UserDataDirectory + soundFilePath));
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error playing sound:{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{ex.InnerException}");
                }
            }
        }
    }
}

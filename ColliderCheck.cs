using Il2CppRUMBLE.MoveSystem;
using MelonLoader;
using UnityEngine;

namespace GrassEnabler
{

    [RegisterTypeInIl2Cpp]
    public class ColliderCheck : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            Structure structure = other.gameObject.GetComponent<Structure>();
            if (structure == null)
            {
                structure = other.GetComponentInParent<Structure>();
                if (structure == null)
                {
                    structure = other.GetComponentInChildren<Structure>();
                }
            }
            if ((structure == null) || !Preferences.PrefRemoval.Value)
            {
                return;
            }
            if (structure.IsGrounded || structure.IsSpawning)
            {
                this.transform.gameObject.SetActive(false);
                if (Preferences.PrefRegrow.Value)
                {
                    MelonCoroutines.Start(Main.RegrowGrass(this.transform.gameObject));
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!this.transform.gameObject.activeSelf) { return; }
            Structure structure = other.gameObject.GetComponent<Structure>();
            if (structure == null)
            {
                structure = other.GetComponentInParent<Structure>();
                if (structure == null)
                {
                    structure = other.GetComponentInChildren<Structure>();
                }
            }
            if ((structure == null) || !Preferences.PrefRemoval.Value)
            {
                return;
            }
            if (structure.IsGrounded || structure.IsSpawning)
            {
                this.transform.gameObject.SetActive(false);
                if (Preferences.PrefRegrow.Value)
                {
                    MelonCoroutines.Start(Main.RegrowGrass(this.transform.gameObject));
                }
            }
        }
    }
}

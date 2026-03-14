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
            if ((structure == null) || !main.grassRemoval)
            {
                return;
            }
            if (structure.IsGrounded || structure.IsSpawning)
            {
                this.transform.gameObject.SetActive(false);
                if (main.grassGrowth)
                {
                    MelonCoroutines.Start(main.RegrowGrass(this.transform.gameObject));
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
            if ((structure == null) || !main.grassRemoval)
            {
                return;
            }
            if (structure.IsGrounded || structure.IsSpawning)
            {
                this.transform.gameObject.SetActive(false);
                if (main.grassGrowth)
                {
                    MelonCoroutines.Start(main.RegrowGrass(this.transform.gameObject));
                }
            }
        }
    }
}

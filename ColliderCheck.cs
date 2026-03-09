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
            Structure structure = other.GetComponent<Structure>();
            if (structure == null)
            {
                structure = other.GetComponentInParent<Structure>();
            }
            if ((structure == null) || !main.grassRemoval)
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
    }
}

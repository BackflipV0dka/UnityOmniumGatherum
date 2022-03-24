using UnityEngine;

namespace UnityOmniumGatherum
{
    [RequireComponent(typeof(Collider2D))]
    public class PHY_SnappingBase : MonoBehaviour
    {
        // INSPECTOR
        [Tooltip("If base tag is empty any object snaps, else only objects with specified tag.")]
        public string CTRL_tag = "";

        [SerializeField,
        Tooltip(@"If empty, position of snapped object will be aligned with the position of the base.
                Else, the position and rotation are aligned to given transform.
                NOTE: you can also use base's transform to force rotation alignment with base.")
        ]
        private Transform TRNSF_snapTo = null;


        // CODE
        private PHY_SnappingObject CTRL_occupier;

        public bool CTRL_alignRot() => TRNSF_snapTo != null;
        public Transform TRNSF_getAlign() => TRNSF_snapTo ? TRNSF_snapTo : this.transform;

        public bool CTRL_checkFit(PHY_SnappingObject toCheck) { return !CTRL_occupier && (CTRL_tag == "" || CTRL_tag == toCheck.CTRL_tag); }
        public bool CTRL_snap(PHY_SnappingObject toSnap)
        {
            if (!CTRL_checkFit(toSnap)) return false;
            EVNT_snap.Invoke(toSnap);
            return true;
        }
        public void CTRL_release(PHY_SnappingObject toRel)
        {
            if (!CTRL_occupier || toRel != CTRL_occupier) return;
            EVNT_release.Invoke(toRel);
        }


        // EVENTS
        public delegate void BaseSnapEvent(PHY_SnappingObject obj);
        public BaseSnapEvent EVNT_snap;
        public BaseSnapEvent EVNT_release;


        // GAME LOGIC
        private void Awake()
        {
            foreach (Collider2D c in GetComponentsInChildren<Collider2D>()) c.isTrigger = true;
            EVNT_snap += (PHY_SnappingObject toSnap) => { CTRL_occupier = toSnap; };
            EVNT_release += (PHY_SnappingObject toSnap) => { CTRL_occupier = null; };
        }
    }
}
using UnityEngine;

namespace UnityOmniumGatherum
{
    [RequireComponent(typeof(PHY_Draggable))]
    public class PHY_SnappingObject : MonoBehaviour
    {
        // INSPECTOR
        [Tooltip("If tag is empty any object snaps, else only objects with specified tag.")]
        public string CTRL_tag = "";
        [SerializeField, Tooltip("Use values greater zero if the ")]
        private float PHY_maxPullForce = 0;

        [Space, Tooltip("Snap to a base at awake")]
        public PHY_SnappingBase InitBase;


        // CODE
        private Rigidbody2D PHY_rigid;
        private PHY_Draggable PHY_drag;

        private PHY_SnappingBase CTRL_hover;


        // EVENTS
        public delegate void ObjSnapEvent(PHY_SnappingBase obj);
        public ObjSnapEvent EVNT_snap;
        public ObjSnapEvent EVNT_release;


        // GAME LOGIC
        private void Awake()
        {
            PHY_rigid = GetComponent<Rigidbody2D>();
            PHY_drag = GetComponent<PHY_Draggable>();

            EVNT_release += (PHY_SnappingBase b) =>
            {
                PHY_rigid.constraints = RigidbodyConstraints2D.None;
                b.CTRL_release(this);
            };
            EVNT_snap += (PHY_SnappingBase b) =>
            {
                Transform t = b.TRNSF_getAlign();
                PHY_rigid.position = t.position;
                if (b.CTRL_alignRot()) PHY_rigid.rotation = t.rotation.eulerAngles.z;
                PHY_rigid.constraints = RigidbodyConstraints2D.FreezeAll;
            };

            PHY_drag.EVNT_grabChange += (bool g) =>
            {
                if (!CTRL_hover) return;

                if (g)
                    EVNT_release.Invoke(CTRL_hover);
                else if (CTRL_hover.CTRL_snap(this))
                    EVNT_snap.Invoke(CTRL_hover);
            };
        }

        private void Start()
        {
            if (InitBase && InitBase.CTRL_snap(this)) EVNT_snap.Invoke(CTRL_hover = InitBase);
        }

        private void FixedUpdate()
        {
            if (PHY_maxPullForce <= 0 || !CTRL_hover) return;
            if (PHY_rigid.Joint2DForceSum().magnitude > PHY_maxPullForce) EVNT_release.Invoke(CTRL_hover);
        }


        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!CTRL_hover) CTRL_hover = collision.GetComponent<PHY_SnappingBase>();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.GetComponent<PHY_SnappingBase>() == CTRL_hover) CTRL_hover = null;
        }
    }
}
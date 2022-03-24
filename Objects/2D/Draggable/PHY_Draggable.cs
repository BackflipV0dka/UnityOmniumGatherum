using UnityEngine;

namespace UnityOmniumGatherum
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PHY_Draggable : MonoBehaviour
    {
        const int LAYER_Draggable = 7;

        // INSPECTOR
        [Header("Pulling Joint Settings")]
        public float PHY_spring = 100;
        public float PHY_maxForce = 1000;
        public float PHY_damper = 5;
        [Tooltip("Doesn't actually break the joint, just deactivates it. Values lower/equal zero are ignored")]
        public float PHY_breakForce = 0;

        [Header("General Movement Limits")]
        [SerializeField, Tooltip("Use values higher zero to set a limit")]
        private float PHY_limitVel = 16;
        [SerializeField, Tooltip("Use values higher zero to set a limit")]
        private float PHY_limitAngVel = 270;


        //CODE
        private Rigidbody2D PHY_rigid;
        private TargetJoint2D PHY_pointerPull;

        protected PHY_Pointer CTRL_grabbedBy;

        public bool CTRL_grab(PHY_Pointer p)
        {
            if (CTRL_grabbedBy || !p) return false;
            CTRL_grabbedBy = p;
            EVNT_grabChange.Invoke(true);
            return true;
        }

        public void CTRL_release(PHY_Pointer p)
        {
            if (CTRL_grabbedBy != p) return;
            CTRL_grabbedBy = null;
            EVNT_grabChange.Invoke(false);
        }


        // EVENTS
        public delegate void DraggableEvent(bool d);
        public DraggableEvent EVNT_grabChange;


        // GAME LOGIC
        protected virtual void Start()
        {
#if UNITY_EDITOR
            if (transform.localScale != Vector3.one) Debug.LogWarning("Physics are janky with scales unequal (1,1,1)");
#endif
            PHY_rigid = GetComponent<Rigidbody2D>();

            // Setup Joint
            PHY_pointerPull = gameObject.AddComponent<TargetJoint2D>();
            PHY_pointerPull.enabled = PHY_pointerPull.autoConfigureTarget = false;
            PHY_pointerPull.frequency = PHY_spring;
            PHY_pointerPull.maxForce = PHY_maxForce;
            PHY_pointerPull.dampingRatio = PHY_damper;

            EVNT_grabChange += (bool b) =>
            {
                if (PHY_pointerPull.enabled = b) PHY_pointerPull.anchor = Quaternion.Inverse(transform.rotation) * (CTRL_grabbedBy.transform.position - transform.position);
            };

            gameObject.layer = CNST_Layers.DRAGGABLE;
        }

        private void FixedUpdate()
        {
            // Movement Limits
            if (PHY_limitVel > 0) PHY_rigid.velocity = Vector2.ClampMagnitude(PHY_rigid.velocity, PHY_limitVel);
            if (PHY_limitAngVel > 0) PHY_rigid.angularVelocity = Mathf.Clamp(PHY_rigid.angularVelocity, -PHY_limitAngVel, PHY_limitAngVel);

            // Dragging
            if (!CTRL_grabbedBy) return;
            PHY_pointerPull.target = CTRL_grabbedBy.transform.position;
            if (PHY_breakForce > 0 && PHY_pointerPull.reactionForce.magnitude > PHY_breakForce) CTRL_release(CTRL_grabbedBy);
        }
    }
}
using UnityEngine;

namespace UnityOmniumGatherum
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class PHY_Pointer : MonoBehaviour
    {
        // INSPECTOR
        [Header("Normal State")]
        public Sprite IMG_normal;
        public Color CLR_normal = Color.white;
        public Color CLR_pressed = Color.white;

        [Header("Hovering Draggable")]
        [Tooltip("Optional, none defaults to IMG_normal")]
        public Sprite IMG_draggable;
        public Color CLR_dragHover = Color.white;
        public Color CLR_dragPressed = Color.white;

        [Header("Hovering Interactable")]
        [Tooltip("Optional, none defaults to IMG_draggable")]
        public Sprite IMG_iactable;
        public Color CLR_iactHover = Color.white;
        public Color CLR_iactPressed = Color.white;

        [Header("Inactive State")]
        [Tooltip("Optional, none defaults to IMG_normal")]
        public Sprite IMG_off;
        public Color CLR_off = Color.red;


        // CODE
        private Collider2D[] PHY_triggers;
        private SpriteRenderer IMG;

        private bool INP_enabled;
        private PHY_Draggable INP_hovered;

        public bool INP_leftPressed { get; private set; }
        public bool INP_rightPressed { get; private set; }
        public bool INP_onScreen { get; private set; }


        private void setGrabbing(bool b)
        {
            if (!INP_hovered)
            {
                if (!INP_enabled) return;
                IMG.sprite = IMG_normal;
                IMG.color = b ? CLR_pressed : CLR_normal;
                return;
            }

            if (b)
            {
                if (!INP_hovered.CTRL_grab(this)) return;
                IMG.color = INP_hovered is PHY_IactDraggable ? CLR_iactPressed : CLR_dragPressed;
                INP_hovered.EVNT_grabChange += setGrabbing;
            }
            else
            {
                IMG.color = INP_hovered is PHY_IactDraggable ? CLR_iactHover : CLR_dragHover;
                INP_hovered.EVNT_grabChange -= setGrabbing;
                INP_hovered.CTRL_release(this);
                INP_leftPressed = false;
                unhover(INP_hovered);
            }
        }

        public void setEnabled(bool b)
        {
            if (INP_enabled == b) return;
            foreach (Collider2D c in PHY_triggers) c.enabled = b;

            IMG.sprite = b ? IMG_normal : IMG_off;
            IMG.color = b ? CLR_normal : CLR_off;

            if (INP_enabled = b) return;
            EVNT_leftMouse.Invoke(false);
        }

        private void hover(PHY_Draggable drag)
        {
            if (INP_hovered || !drag) return;

            bool iact = drag is PHY_IactDraggable;
            IMG.sprite = iact ? IMG_iactable : IMG_draggable;
            IMG.color = iact ? CLR_iactHover : CLR_dragHover;

            INP_hovered = drag;
        }

        private void unhover(PHY_Draggable drag)
        {
            if (INP_leftPressed || INP_hovered != drag) return;
            IMG.sprite = IMG_normal;
            IMG.color = CLR_normal;
            INP_hovered = null;
        }


        // EVENTS
        public delegate void InputEvent(bool b);
        public InputEvent EVNT_leftMouse;
        public InputEvent EVNT_rightMouse;
        public InputEvent EVNT_screenLeave;


        // GAME LOGIC
        private void Awake()
        {
#if UNITY_EDITOR
            if (!IMG_normal)
            {
                Debug.LogError("The pointer needs at least the normal sprite assigned! Deactivating.");
                Destroy(this); return;
            }
#endif

            // Graphics
            IMG = GetComponent<SpriteRenderer>();
            // Replace not specified sprites with given ones
            if (!IMG_draggable) IMG_draggable = IMG_normal;
            if (!IMG_iactable) IMG_iactable = IMG_draggable;
            if (!IMG_off) IMG_off = IMG_normal;

            // Physics
            PHY_triggers = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D c in PHY_triggers) c.isTrigger = true;

            // Events
            EVNT_screenLeave += (bool b) =>
            {
                if (!(INP_onScreen = b) && INP_enabled) EVNT_leftMouse.Invoke(false);
            };
            EVNT_leftMouse += (bool b) =>
            {
                INP_leftPressed = b;
                setGrabbing(b);
            };
            EVNT_rightMouse += (bool b) =>
            {
                INP_rightPressed = b;
            };

            setEnabled(!(INP_enabled = false));
#if !UNITY_EDITOR
        Cursor.visible = false;
#endif
        }

        void Update()
        {
            Vector2 pos = Input.mousePosition;
            transform.position = Camera.main.ScreenToWorldPoint(pos) + Vector3.forward * 0.5f;
            if (INP_onScreen == (pos.x < 0 || pos.x > Screen.width || pos.y < 0 || pos.y > Screen.height)) EVNT_screenLeave.Invoke(!INP_onScreen);

            if (!INP_enabled) return;
            if (INP_leftPressed != Input.GetMouseButton(0)) EVNT_leftMouse.Invoke(!INP_leftPressed);
            if (INP_rightPressed != Input.GetMouseButton(1)) EVNT_rightMouse.Invoke(!INP_rightPressed);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            hover(collision.GetComponent<PHY_Draggable>());
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            unhover(collision.GetComponent<PHY_Draggable>());
        }
    }
}
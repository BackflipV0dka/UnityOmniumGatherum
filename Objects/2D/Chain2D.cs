using System.Collections.Generic;
using UnityEngine;

namespace UnityOmniumGatherum
{
    public class Chain2D : MonoBehaviour
    {
        public enum LINKDIR { UPPOS, UPNEG, RIGHTPOS, RIGHTNEG }
        public static Vector3 LinkDirFromTransform(Transform t, LINKDIR dir) => Mathf.Pow(-1, (int)dir % 2) * ((int)dir < 2 ? t.up : t.right);


        //INSPECTOR
        public GameObject PREF_linkPhysical;
        public GameObject PREF_linkRender;

        [Space, SerializeField]
        private uint INIT_linkAmount = 0;
        [SerializeField]
        private float INIT_linkLength = 0.2f;
        public LINKDIR INIT_direction = LINKDIR.UPPOS;

        [Space]
        public Joint2D PHY_attachFirst;
        public Joint2D PHY_attachLast;


        //CODE
        private List<Rigidbody2D> PHY_links;

        // 0 = after first link => max index INIT_linkAmount-2 (cant cut after last element = INIT_linkAmount-1)
        public void CTRL_cutAfter(int l)
        {
            if (l < 0 || l > PHY_links.Count - 2) return;
            foreach (Joint2D j in PHY_links[l].GetComponents<Joint2D>()) Destroy(j);
            Destroy(PHY_links[l + 1].GetComponentInChildren<RNDR_ChainLink2D>());
        }

        // GAME LOGIC
        private void Awake()
        {
#if UNITY_EDITOR
            if (!PREF_linkPhysical || !PREF_linkRender) Debug.LogError("Not all link representations specified!");
            if (!PREF_linkPhysical.GetComponent<Rigidbody2D>() ||
                PREF_linkPhysical.GetComponentsInChildren<Collider2D>().Length == 0 ||
                PREF_linkPhysical.GetComponentsInChildren<Joint2D>().Length == 0) Debug.LogError("Physical representation needs a rigidbody, collider and (not connected) joint!");
            if (!PREF_linkRender.GetComponentInChildren<SpriteRenderer>()) Debug.LogError("Graphical representation needs a sprite renderer.");
#endif

            Rigidbody2D last = null;
            PHY_links = new List<Rigidbody2D>((int)INIT_linkAmount);

            for (int i = 0; i < INIT_linkAmount; i++)
            {
                Transform phlink = Instantiate(PREF_linkPhysical, this.transform).transform;
                phlink.name = "PHY_" + this.name + i;
                phlink.position += LinkDirFromTransform(phlink, INIT_direction) * i * INIT_linkLength;

                Rigidbody2D ph = phlink.GetComponent<Rigidbody2D>();
                PHY_links.Add(ph);

                GameObject render = Instantiate(PREF_linkRender, phlink);
                render.name = "RNDR_" + this.name + i;

                if (i == INIT_linkAmount - 1)
                    foreach (Joint2D j in phlink.GetComponents<Joint2D>()) if (j.enabled) Destroy(j);

                if (i > 0)
                {
                    foreach (Joint2D j in last.GetComponents<Joint2D>()) if (j.enabled) j.connectedBody = ph;
                    render.AddComponent<RNDR_ChainLink2D>().Setup(last.transform, INIT_linkLength, INIT_direction);
                }

                last = ph;
            }

            PREF_linkPhysical.SetActive(false);
            PREF_linkRender.SetActive(false);

            if (PHY_attachFirst) PHY_attachFirst.connectedBody = PHY_links[0];
            if (PHY_attachLast) PHY_attachLast.connectedBody = PHY_links[PHY_links.Count - 1];
        }
    }

    // Helper Component, added to every visual representation of the links
    public class RNDR_ChainLink2D : MonoBehaviour
    {
        private const float tolerance = 1.05f;


        // CODE
        private Transform prevLink;
        private float actualLen;
        private Chain2D.LINKDIR dir;

        private Vector3 origScale;
        private Quaternion origRot;
        private bool origLeft;

        public void Setup(Transform pL, float len, Chain2D.LINKDIR d)
        {
            if (prevLink || !pL) return;
            prevLink = pL;
            actualLen = len;
            dir = d;
        }


        // GAME LOGIC
        private void Start()
        {
            origScale = transform.localScale;
            origRot = transform.localRotation;
        }

        private void Update()
        {
            Vector2 diff = transform.position - prevLink.position;
            bool isBuggy = diff.magnitude > actualLen * tolerance;

            if (origLeft)
            {
                if (!isBuggy)
                {
                    transform.localRotation = origRot;
                    transform.localScale = origScale;
                    origLeft = false;
                }
                else
                {
                    float scale = diff.magnitude / actualLen;
                    bool up = (int)dir < 2;
                    transform.Rotate(Vector3.forward, Vector2.SignedAngle(Chain2D.LinkDirFromTransform(transform, dir), diff));
                    transform.localScale = new Vector3(origScale.x * (up ? 1 : scale), origScale.y * (up ? scale : 1), origScale.z);
                }
            }
            else if (isBuggy)
                origLeft = true;
        }

        private void OnDestroy()
        {
            transform.localRotation = origRot;
            transform.localScale = origScale;
        }
    }
}
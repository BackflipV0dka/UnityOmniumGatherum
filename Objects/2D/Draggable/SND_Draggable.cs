using UnityEngine;

namespace UnityOmniumGatherum
{
    [RequireComponent(typeof(PHY_Draggable))]
    public class SND_Draggable : SND
    {
        // INSPECTOR
        [SerializeField]
        private AudioClip[] SND_grabbed;
        [SerializeField]
        private AudioClip[] SND_dropped;

        // CODE
        private PHY_Draggable OBJ;

        // GAME LOGIC
        protected override void Awake()
        {
#if UNITY_EDITOR
            if (SND_grabbed.Length == 0 && SND_dropped.Length == 0) Debug.LogWarning("Sound component without audio clips is useless.");
#endif
            base.Awake();

            OBJ = GetComponent<PHY_Draggable>();
            OBJ.EVNT_grabChange += (bool g) => { SND.PlayRandom(SND_src, g ? SND_grabbed : SND_dropped); };
        }
    }
}

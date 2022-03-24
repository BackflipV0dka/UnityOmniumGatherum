using UnityEngine;

namespace UnityOmniumGatherum
{
    [RequireComponent(typeof(PHY_SnappingBase))]
    public class SND_SnappingBase : SND
    {
        // INSPECTOR
        [SerializeField]
        private AudioClip[] SND_snap;
        [SerializeField]
        private AudioClip[] SND_release;

        // CODE
        private PHY_SnappingBase OBJ;

        // GAME LOGIC
        protected override void Awake()
        {
#if UNITY_EDITOR
            if (SND_snap.Length == 0 && SND_release.Length == 0) Debug.LogWarning("Sound component without audio clips are useless.");
#endif
            base.Awake();

            OBJ = GetComponent<PHY_SnappingBase>();
            if (SND_snap.Length > 0) OBJ.EVNT_snap += (PHY_SnappingObject o) => { SND.PlayRandom(SND_src, SND_snap); };
            if (SND_release.Length > 0) OBJ.EVNT_release += (PHY_SnappingObject o) => { SND.PlayRandom(SND_src, SND_release); };
        }
    }
}

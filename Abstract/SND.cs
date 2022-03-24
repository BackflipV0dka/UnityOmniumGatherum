using UnityEngine;

namespace UnityOmniumGatherum
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class SND : MonoBehaviour
    {
        // CODE
        protected AudioSource SND_src;

        public static void PlayRandom(AudioSource src, AudioClip[] snds)
        {
            if (snds == null || snds.Length == 0) return;
            src.PlayOneShot(snds[Random.Range(0, snds.Length)]);
        }


        // GAME LOGIC
        protected virtual void Awake()
        {
            SND_src = GetComponent<AudioSource>();
        }
    }
}


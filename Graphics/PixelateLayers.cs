using UnityEngine;
using UnityEngine.UI;

namespace UnityOmniumGatherum
{
    [RequireComponent(typeof(Camera))]
    public class PixelateLayers : MonoBehaviour
    {
        // INSPECTOR
        [Header("Render Texture")]
        public uint verticalPixels;
        public FilterMode filter = FilterMode.Point;
        
        [Header("Affected Layers")]
        public LayerMask PixelLayers;
        public bool excludeFromMainCam;


        // CODE
        private RawImage Overlay;
        private Camera Render;
        public Vector2Int ScreenDimension { get; private set; }


        // EVENTS
        public delegate void ResolutionChangeEvent(Vector2Int dims);
        public ResolutionChangeEvent EVNT_resolutionChanged;


        // GAME LOGIC
        void Start()
        {
#if UNITY_EDITOR
            if (verticalPixels == 0)
            {
                Debug.LogError("RenderTexture height can't be zero!");
                DestroyImmediate(this);
            }
#endif

            Camera cam = GetComponent<Camera>();
            if (excludeFromMainCam) cam.cullingMask = ~PixelLayers;

            GameObject obj = new GameObject("PIXEL_Cam");
            obj.transform.SetParent(this.transform, false);
            Render = obj.AddComponent<Camera>();
            Render.depth = cam.depth - 1;
            Render.cullingMask = PixelLayers;
            Render.orthographic = cam.orthographic;
            Render.orthographicSize = cam.orthographicSize;
            Render.clearFlags = CameraClearFlags.SolidColor;

            obj = new GameObject("PIXEL_Overlay");
            obj.transform.SetParent(this.transform, false);
            Canvas canv = obj.AddComponent<Canvas>();
            canv.renderMode = RenderMode.ScreenSpaceCamera;
            canv.planeDistance = 0.5f;
            canv.worldCamera = cam;

            obj = new GameObject("PIXEL");
            obj.transform.SetParent(canv.transform, false);
            Overlay = obj.AddComponent<RawImage>();
            Overlay.raycastTarget = false;
            Overlay.rectTransform.anchorMin = Vector2.zero;
            Overlay.rectTransform.anchorMax = Vector2.one;
            Overlay.rectTransform.sizeDelta = Vector2.one;

            EVNT_resolutionChanged += (Vector2Int dims) =>
            {
                if (Overlay.texture) Destroy(Overlay.texture);
                Overlay.texture = Render.targetTexture = new RenderTexture((int)(1f * verticalPixels / dims.y * dims.x), (int)verticalPixels, 0);
                Render.targetTexture.filterMode = filter;
                Render.targetTexture.antiAliasing = 1;
                ScreenDimension = dims;
                Debug.Log("Updated screen size and RenderTexture");
            };
        }

        private void Update() {
            Vector2Int dims = new Vector2Int(Screen.width, Screen.height);
            if (dims != ScreenDimension) EVNT_resolutionChanged.Invoke(dims);
        }
    }
}
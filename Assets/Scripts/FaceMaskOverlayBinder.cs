using UnityEngine;
using UnityEngine.UI;

namespace Mediapipe.Unity
{
    [DefaultExecutionOrder(1000)]
    public class FaceMaskOverlayBinder : MonoBehaviour
    {
        [Header("Source (camera preview RawImage)")]
        [SerializeField] public RawImage screenRawImage; // chính _screen trên object Screen

        [Header("Target (overlay RawImage showing RenderTexture)")]
        [SerializeField] public RawImage overlayRawImage;

        [Tooltip("Nếu muốn overlay copy cả color/alpha từ screen (thường không cần)")]
        [SerializeField] private bool syncColor = false;

        private void LateUpdate()
        {
            if (screenRawImage == null || overlayRawImage == null) return;

            // Copy rect size + rotation
            var srt = screenRawImage.rectTransform;
            var ort = overlayRawImage.rectTransform;

            ort.sizeDelta = srt.sizeDelta;
            ort.localEulerAngles = srt.localEulerAngles;

            // Copy uvRect (Screen.cs có thể flip bằng uvRect)【:contentReference[oaicite:2]{index=2}】
            overlayRawImage.uvRect = screenRawImage.uvRect;

            if (syncColor)
            {
                overlayRawImage.color = screenRawImage.color;
            }
        }
    }
}
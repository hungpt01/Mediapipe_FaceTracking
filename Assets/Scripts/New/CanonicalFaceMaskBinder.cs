using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class CanonicalFaceMaskBinder : MonoBehaviour
{
    [Header("Source (camera preview)")]
    [SerializeField] private RawImage screenRawImage; // chính _screen

    [Header("Overlay (shows RenderTexture from FaceMaskCam)")]
    [SerializeField] private RawImage overlayRawImage; // RawImage hiển thị RT

    [Header("Deformer")]
    [SerializeField] private CanonicalFaceMeshDeformer deformer;

    [Header("RenderTexture Info (optional)")]
    [SerializeField] private RenderTexture faceMaskRT;

    private void LateUpdate()
    {
        if (screenRawImage == null) return;

        // 1) Sync overlay UI để đè đúng lên preview
        if (overlayRawImage != null)
        {
            var srt = screenRawImage.rectTransform;
            var ort = overlayRawImage.rectTransform;

            ort.sizeDelta = srt.sizeDelta;
            ort.localEulerAngles = srt.localEulerAngles;
            overlayRawImage.uvRect = screenRawImage.uvRect;
        }

        // 2) Sync uvRect cho deformer để mapping landmark khớp preview
        if (deformer != null)
        {
            deformer.screenUvRect = screenRawImage.uvRect;

            // 3) Set aspect theo RT nếu có để tránh méo
            if (faceMaskRT != null)
            {
                deformer.overrideAspect = (float)faceMaskRT.width / faceMaskRT.height;
            }
        }
    }
}
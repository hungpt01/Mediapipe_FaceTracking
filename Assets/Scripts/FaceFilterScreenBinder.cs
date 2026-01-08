using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mediapipe.Unity
{
    public class FaceFilterScreenBinder : MonoBehaviour
    {
        [SerializeField] private Screen screen;                 // script Screen.cs
        [SerializeField] private RawImage screenRawImage;        // chính _screen trong Screen
        [SerializeField] private FaceFilterRawImage filter;      // rawimage filter
        
        [Header("468 UV (canonical or baked-from-template)")] [SerializeField]
        private TextAsset faceUv468;

        [Header("468 Triangulation indices")] [SerializeField]
        private TextAsset faceTriangles468;

        private void Awake()
        {
            if (filter == null)
            {
                Debug.LogError("[FaceFilterScreenBinder] Filter is null");
                return;
            }
            if (faceUv468 == null || faceTriangles468 == null)
            {
                Debug.LogError("[FaceFilterScreenBinder] Missing faceUv468/faceTriangles468 TextAsset");
                return;
            }
            filter.SetFaceAssets(faceUv468, faceTriangles468);
        }

        private void LateUpdate()
        {
            if (screenRawImage == null || filter == null) return;

            // Match size & rotation (Screen.Rotate + Resize)【:contentReference[oaicite:9]{index=9}】
            filter.rectTransform.sizeDelta = screenRawImage.rectTransform.sizeDelta;
            filter.rectTransform.localEulerAngles = screenRawImage.rectTransform.localEulerAngles;

            // Match uvRect (Screen.ResetUvRect)【:contentReference[oaicite:10]{index=10}】
            filter.screenUvRect = screenRawImage.uvRect;
        }
    }
}
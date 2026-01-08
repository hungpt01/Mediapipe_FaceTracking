using UnityEngine;
using UnityEngine.UI;

namespace Mediapipe.Unity
{
    [DefaultExecutionOrder(1100)]
    public class FaceMaskMeshBinder : MonoBehaviour
    {
        [Header("Source (camera preview RawImage)")]
        [SerializeField] private RawImage screenRawImage;

        [Header("Mesh + Deformer")]
        [SerializeField] private FaceMaskMeshDeformer deformer; // script deform mesh theo landmarks
        [SerializeField] private Transform faceMaskMeshTransform;

        [Header("Render Camera")]
        [SerializeField] private Camera faceMaskCam;

        [Tooltip("Khoảng cách camera tới mesh (camera nhìn -Z -> +Z)")]
        [SerializeField] private float cameraZ = -10f;

        private void Awake()
        {
            if (faceMaskCam != null)
            {
                faceMaskCam.orthographic = true;
                var p = faceMaskCam.transform.position;
                faceMaskCam.transform.position = new Vector3(0, 0, cameraZ);
                faceMaskCam.transform.rotation = Quaternion.identity;
            }

            if (faceMaskMeshTransform != null)
            {
                faceMaskMeshTransform.position = Vector3.zero;
                faceMaskMeshTransform.rotation = Quaternion.identity;
                faceMaskMeshTransform.localScale = Vector3.one;
            }
        }

        private void LateUpdate()
        {
            if (screenRawImage == null) return;

            // 1) Sync uvRect cho deformer để mapping landmark khớp preview
            if (deformer != null)
            {
                deformer.screenUvRect = screenRawImage.uvRect;
            }

            // 2) Setup orthographic size dựa theo scale bạn đang dùng trong deformer
            // Nếu deformer map x/y về [-0.5..0.5] * xyScale, thì orthoSize = 0.5 * xyScale
            if (faceMaskCam != null && deformer != null)
            {
                faceMaskCam.orthographicSize = 0.5f * Mathf.Max(0.0001f, deformer.xyScale);
            }
        }
    }
}
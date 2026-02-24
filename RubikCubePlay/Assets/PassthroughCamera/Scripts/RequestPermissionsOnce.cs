using UnityEngine;
using UnityEngine.Android; // Use the standard Unity Android library instead of Oculus.VR

public class RequestPermissionsOnce : MonoBehaviour
{
    void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        RequestSpatialPermissions();
        #endif
    }

    void RequestSpatialPermissions()
    {
        // These are the standard strings the Quest looks for
        string scenePermission = "com.oculus.permission.USE_SCENE";
        string cameraPermission = Permission.Camera; // Standard Android Camera permission

        // 1. Request Camera for YOLO
        if (!Permission.HasUserAuthorizedPermission(cameraPermission))
        {
            Permission.RequestUserPermission(cameraPermission);
        }

        // 2. Request Scene for MRUK
        if (!Permission.HasUserAuthorizedPermission(scenePermission))
        {
            Permission.RequestUserPermission(scenePermission);
        }
    }
}
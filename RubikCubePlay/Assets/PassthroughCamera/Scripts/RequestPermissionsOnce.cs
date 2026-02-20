using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class RequestPermissionsOnce : MonoBehaviour
{
    void Start()
    {
#if UNITY_ANDROID
        RequestPermissions();
#endif
    }

    void RequestPermissions()
    {
#if UNITY_ANDROID
        // Camera permission (needed for YOLO)
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.Log("Requesting Camera Permission...");
            Permission.RequestUserPermission(Permission.Camera);
        }
        else
        {
            Debug.Log("Camera Permission already granted.");
        }

        // Scene permission (Meta Quest MR)
        string scenePermission = "com.oculus.permission.USE_SCENE";

        if (!Permission.HasUserAuthorizedPermission(scenePermission))
        {
            Debug.Log("Requesting Scene Permission...");
            Permission.RequestUserPermission(scenePermission);
        }
        else
        {
            Debug.Log("Scene Permission already granted.");
        }
#endif
    }
}
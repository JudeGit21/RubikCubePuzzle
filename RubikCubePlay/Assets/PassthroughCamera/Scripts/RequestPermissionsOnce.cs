using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This check prevents the "CS0246" error from stopping the whole build
#if UNITY_ANDROID && !UNITY_EDITOR
using Oculus.VR; 
#endif

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
        string[] permissions = {
            "com.oculus.permission.USE_SCENE",
            "android.permission.CAMERA"
        };

        #if UNITY_ANDROID && !UNITY_EDITOR
        try {
            if (OVRPermissionsRequester.IsPermissionGranted(permissions[0]))
            {
                Debug.Log("Permissions already granted.");
            }
            else
            {
                OVRPermissionsRequester.RequestPermissions(permissions);
            }
        } catch (System.Exception e) {
            Debug.LogError("Oculus VR Reference missing in Build: " + e.Message);
        }
        #endif
    }
}
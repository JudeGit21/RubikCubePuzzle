using UnityEngine;
using Unity.Sentis;
// This must match the name in your Assembly Definition (yo12.png)
using Meta.XR.MRUtilityKit; 

public class YoloVisualizer : MonoBehaviour
{
    [Header("Setup")]
    public GameObject rubikCubePrefab; 
    private GameObject activeCube;

    // This is the function called by your YoloDetector
    public void UpdateVisuals(Tensor<float> output)
    {
        // 1. Get the raw data from the AI
        float[] data = output.DownloadToArray();
        
        // 2. Simple logic to find the 'Cube' (Placeholder for your detection logic)
        // Let's assume the AI found it at center screen (0.5, 0.5)
        float x = 0.5f;
        float y = 0.5f;

        // 3. Place the cube using the MRUK "Sticky" logic
        PlaceCubeInRealWorld(x, y);
    }

    private void PlaceCubeInRealWorld(float x, float y)
    {
        // Convert the 2D AI coordinates into a 3D Ray from the Quest 3 camera
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(x * Screen.width, y * Screen.height, 0));

        // Use MRUK to find the floor, wall, or table
        if (MRUK.Instance != null && MRUK.Instance.GetCurrentRoom() != null)
        {
            // This 'Raycast' checks the Quest 3's room scan data
            if (MRUK.Instance.GetCurrentRoom().Raycast(ray, 10f, out RaycastHit hit))
            {
                if (activeCube == null)
                {
                    activeCube = Instantiate(rubikCubePrefab);
                }

                // Snap the cube to the hit point on the real table
                activeCube.transform.position = hit.point;
                
                // Make the cube sit flat on the surface
                activeCube.transform.rotation = Quaternion.LookRotation(hit.normal);
            }
        }
    }
}
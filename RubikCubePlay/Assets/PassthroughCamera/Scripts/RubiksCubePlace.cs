using System.IO.MemoryMappedFiles;
using UnityEngine;

public class RubiksCubePlace : MonoBehaviour
{
    public Vector3 offset;
    public bool stick = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Transform player = Camera.main.transform;
        transform.position = player.position + (player.forward * offset.z) + (player.up * offset.y) + (player.right * offset.x);
    }

    // Update is called once per frame
    void Update()
    {
        if (stick)
        {

            Transform player = Camera.main.transform;
            transform.position = player.position + (player.forward * offset.z) + (player.up * offset.y) + (player.right * offset.x);
        }
    }
}



using UnityEngine;

public class CubeStateBridge : MonoBehaviour
{
    public char[][] faces = new char[6][];

    int currentFace = 0;

    public void SaveFace(char[] data)
    {
        faces[currentFace] = data;
        currentFace++;

        Debug.Log("Saved face " + currentFace);

        if(currentFace == 6)
        {
            SolveCube();
        }
    }

    void SolveCube()
    {
        string cubeString = "";

        for(int f=0; f<6; f++)
        {
            foreach(char c in faces[f])
            {
                cubeString += c;
            }
        }

        Debug.Log("Cube State: " + cubeString);
    }
}
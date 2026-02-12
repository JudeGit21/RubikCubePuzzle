using UnityEngine;
// No need for System.Diagnostics on Quest if we aren't using PowerShell

public class PowerShellTTS : MonoBehaviour
{
    // This is the function called by your YoloInferenceManager
    public void Speak(string text)
    {
        // 1. Log to the console so you can see it in Logcat/Unity
        Debug.Log("Quest TTS: " + text);

        // 2. Prevent crashes on Android
        try
        {
            // Note: Currently, this just logs the text.
            // To hear actual audio on Quest, you would use Meta's 'TTSService' here.
        }
        catch (System.Exception e)
        {
            Debug.LogError("TTS Error: " + e.Message);
        }
    }
}
using UnityEngine;
using System.Diagnostics;

public class PowerShellTTS : MonoBehaviour
{
    public void Speak(string text)
    {
        // Remove single quotes to prevent PowerShell command breaking
        string safeText = text.Replace("'", "");

        string command = $"Add-Type -AssemblyName System.Speech; " +
                         $"$speak = New-Object System.Speech.Synthesis.SpeechSynthesizer; " +
                         $"$speak.Speak('{safeText}')";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -WindowStyle Hidden -Command \"{command}\"", // Comma here
            CreateNoWindow = true, // Comma here
            UseShellExecute = false // No comma needed on the last one
        };

        Process.Start(psi);
        UnityEngine.Debug.Log("TTS Speaking: " + text);
    }
}
using UnityEngine;
using System.Diagnostics;

public class SpeechManager : MonoBehaviour
{
    public void Speak(string text)
    {
        // Escaping single quotes to prevent PowerShell errors
        string safeText = text.Replace("'", "");
        string command = $"Add-Type -AssemblyName System.Speech; " +
                         $"$speak = New-Object System.Speech.Synthesis.SpeechSynthesizer; " +
                         $"$speak.Speak('{safeText}')";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -WindowStyle Hidden -Command \"{command}\"",
            CreateNoWindow = true,
            UseShellExecute = false
        };
        Process.Start(psi);
    }
}
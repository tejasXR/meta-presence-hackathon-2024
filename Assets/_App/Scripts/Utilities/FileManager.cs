using System;
using System.IO;
using UnityEngine;

public static class FileManager
{
    public static bool WriteToFile(string filename, string fileContents)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, filename);
        try
        {
            File.WriteAllText(fullPath, fileContents);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to write to {fullPath} with exception {e}");
            return false;
        }
    }

    public static bool LoadFromFile(string filename, out string result)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, filename);
        try
        {
            result = File.ReadAllText(fullPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to read from {fullPath} with exception {e}");
            result = "";
            return false;
        }
    }
}

using System;
using System.IO;
using UnityEngine;

public static class FileManager
{
    public static bool TryWriteToFile(string filePath, string fileContents)
    {
        try
        {
            File.WriteAllText(filePath, fileContents);
            Debug.Log($"Successfully saved file at: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to write to {filePath} with exception {e}");
            return false;
        }
    }

    public static bool TryLoadFromFile(string filePath, out string result)
    {
        try
        {
            result = File.ReadAllText(filePath);
            Debug.Log($"Successfully loaded file at path: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to read from {filePath} with exception {e}");
            result = "";
            return false;
        }
    }

    public static bool TryDeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
            Debug.Log($"Successfully deleted file at path: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to delete file at {filePath} with exception {e}");
            return false;
        }
    }
}

﻿using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class StorageManager
{
    // public static string StorageLocation { get; private set; }
    public static string ExternalLocation = Application.persistentDataPath + "/Storage/";
    public static string InternalLocation = Application.streamingAssetsPath + "/Storage/";
    public static string ResourcesLocation = "Storage/";


    public static void Init(string storageLocation)
    {
        if (!storageLocation.EndsWith("/"))
            storageLocation += "/";
        ExternalLocation = storageLocation;
        if (!Directory.Exists(ExternalLocation))
            Directory.CreateDirectory(ExternalLocation);
    }

    public static bool IsDirectoryPresent(string path, out string fullPath)
    {
        fullPath = String.Empty;
        if (Directory.Exists(ExternalLocation + path))
        {
            fullPath = ExternalLocation + path;
            return true;
        }
        if (Directory.Exists(InternalLocation + path))
        {
            fullPath = InternalLocation + path;
            return true;
        }
        if (Directory.Exists(path))
        {
            fullPath = path;
            return true;
        }
        return false;
    }

    public static string[] GetFilesInDirectory(string directoryPath)
    {
        if (IsDirectoryPresent(directoryPath,out string fullPath))
        {
            return Directory.GetFiles(fullPath);
        }
        return Array.Empty<string>();
    }

    public static void TouchPath(string storageLocation)
    {
        storageLocation = storageLocation.Substring(0, storageLocation.LastIndexOf('/'));
        Directory.CreateDirectory(storageLocation);
    }

    public static string GetNewFileName(string prepend, string append)
    {
        string path = prepend + "_SOMETHING_" + append;

        do
        {
            int lastGeneratedIndex = PlayerPrefs.GetInt("lastGeneratedIndex", 0);
            lastGeneratedIndex++;
            PlayerPrefs.SetInt("lastGeneratedIndex", lastGeneratedIndex);
            PlayerPrefs.Save();

            path = prepend + lastGeneratedIndex + append;
        } while (IsFileExist(path));

        return path;
    }

    public static bool IsFileExist(string path)
    {
        return File.Exists(path) || File.Exists(InternalLocation + path) || File.Exists(ExternalLocation + path);
    }

    public static void Write(string file, string data)
    {
        TouchPath(ExternalLocation + file);
        StreamWriter writer = new StreamWriter(ExternalLocation + file, false);
        writer.Write(data);
        writer.Flush();
        writer.Close();
    }

    public static void Write(string file, byte[] data)
    {
        TouchPath(ExternalLocation + file);
        StreamWriter writer = new StreamWriter(ExternalLocation + file, false);
        writer.BaseStream.Write(data, 0, data.Length);
        writer.Flush();
        writer.Close();
    }

    public static string ReadNow(string file, string defaultData = null)
    {
        string path = ExternalLocation + file;
        if (!IsFileExist(path))
        {
            path = ResourcesLocation + "/" + file;

            // Remove extension if present
            int dot;
            if ((dot = path.LastIndexOf('.')) > -1)
                path = path.Substring(0, dot);

            TextAsset textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null) return defaultData;

            return textAsset.text;
        }

        StreamReader reader = new StreamReader(path);
        try
        {
            string data = reader.ReadToEnd();
            reader.Close();
            return data;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return defaultData;
        }
        finally
        {
            reader.Close();
        }
    }

    public static byte[] ReadBytesNow(string file)
    {
        string path = ExternalLocation + file;

        if (!File.Exists(path))
        {
            return null;
        }

        StreamReader reader = new StreamReader(path);
        try
        {
            using (var memstream = new MemoryStream())
            {
                reader.BaseStream.CopyTo(memstream);
                return memstream.ToArray();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
        finally
        {
            reader.Close();
        }
    }

    public static ReadOperation Read(string file, string defaultData = null)
    {
        return new ReadOperation(file, defaultData);
    }

    private static string GetFullPath(string file)
    {
        string path = ExternalLocation + file;
        if (IsFileExist(path)) return path;

        path = InternalLocation + file;
        if (IsFileExist(path)) return path;

        Debug.Log(path + " Doesn't exist");
        return null;
    }

    public static void EraseEverything()
    {
        if (Directory.Exists(ExternalLocation))
        {
            foreach (string file in Directory.GetFiles(ExternalLocation))
            {
                if (Directory.Exists(file)) continue;
                File.Delete(file);
            }
        }
    }
    
    public static void EraseEverything(string path)
    {
        if (Directory.Exists(path))
        {
            foreach (string file in Directory.GetFileSystemEntries(path))
            {
                if (Directory.Exists(file))
                {
                    EraseEverything(file);
                }
                else File.Delete(file);
            }
        }
    }

    public static bool IsStorageEmpty()
    {
        var Files = Directory.GetFiles(ExternalLocation);
        if (Files.Length > 1)
            return false;
        else
            return true;
    }

    public static bool Delete(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
        else if (File.Exists(ExternalLocation + filePath))
            File.Delete(ExternalLocation + filePath);
        else
            return false;

        return true;
    }
}

public class ReadOperation
{
    public string file;
    public string data;
    public bool isComplete { get; private set; }
    private Action<string> _action;

    public ReadOperation(string file, string defaultData)
    {
        this.file = file;
        data = defaultData;
        isComplete = false;
    }

    public ReadOperation OnComplete(Action<string> onCompleteAction)
    {
        _action = onCompleteAction;
        return this;
    }

    public ReadOperation Start(MonoBehaviour monoBehaviour)
    {
        monoBehaviour.StartCoroutine(Read());
        return this;
    }

    public IEnumerator Read()
    {
        string _data = StorageManager.ReadNow(file);
        if (_data != null)
        {
            data = _data;
            isComplete = true;
            if (_action != null)
                _action(data);
            yield break;
        }

        string path = StorageManager.ExternalLocation + file;
        if (!StorageManager.IsFileExist(path))
            path = StorageManager.InternalLocation + file;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS
        if (!StorageManager.IsFileExist(path))
        {
            isComplete = true;
            if (_action != null)
                _action(data);
            yield break;
        }
#endif

        if (path.Contains("://"))
        {
            UnityWebRequest wwwReader = UnityWebRequest.Get(path);
            while (!isComplete)
            {
                yield return wwwReader.SendWebRequest();
                isComplete = wwwReader.isDone;
            }

            if (string.IsNullOrEmpty(wwwReader.error))
                data = wwwReader.downloadHandler.text;
        }
        else
        {
            StreamReader reader = new StreamReader(path);
            data = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
            isComplete = true;
        }

        if (_action != null)
            _action(data);
    }
}
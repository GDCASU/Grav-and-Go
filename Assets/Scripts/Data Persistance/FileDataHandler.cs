using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * Cami Lee
 * 
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Pupose:
 * Handles the file writting and reading
 */// --------------------------------------------------------

/// <summary>
/// Handler class that will take care of storing the data to a file in the streaming assets folder
/// </summary>
public class FileDataHandler
{
    private string _persistantPath;

    public FileDataHandler(string dataFolderName)
    {
        // Get the path to the save file folder
        _persistantPath = ResolveDataPath() + dataFolderName;
    }

    /// <summary> Figure out where to create the serialized files </summary>
    /// <returns> Path to data </returns>
    /// <exception cref="Exception"></exception>
    private string ResolveDataPath()
    {
        switch (Application.platform)
        {
            // If on the editor, save the data within the asset folder
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.LinuxEditor:
                return Application.dataPath;
            // If on the compiled game, used persistent data path
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.WebGLPlayer:
                return Application.persistentDataPath;
        }
        // ERROR: Something happened while getting the save data file
        string msg = "PATH TO SAVED GAMED FAILED TO RESOLVE\n";
        msg += "Error thrown on FileDataHandler.cs";
        throw new Exception(msg);
    }

    /// <summary> Saves current data iteration </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="relativePath"> Specific folder to store the data in </param>
    /// <param name="data"> The data to store in JSON format </param>
    /// <param name="isEncrypted"> Whether or not we want to hide the data from users (slower) </param>
    /// <returns>Whether or not the data saved</returns>
    public bool SaveData<T>(string relativePath, T data, bool isEncrypted)
    {
        string path = _persistantPath + relativePath;

        try
        {
            // If there is already data in the path
            if (File.Exists(path))
            {
                Debug.Log("Overwriting data");
                File.Delete(path);
            }

            using FileStream stream = File.Create(path);
            stream.Close();
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save configs to file: " + path + "\n" + e);
            return false;
        }
    }

    /// <summary> Loads data by path name </summary>
    /// <typeparam name="T"> Type of data we're storing </typeparam>
    /// <param name="relativePath">Folder to take the data from under data folder.</param>
    /// <param name="isEncrypted"> Whether or not the data we're accessing is encrypted. </param>
    /// <returns>Current data</returns>
    /// <exception cref="FileNotFoundException"></exception>
    public T LoadData<T>(string relativePath, bool isEncrypted)
    {
        string path = _persistantPath + relativePath;

        if (!File.Exists(path))
        {
            Debug.Log($"File at {path} does not exist.");
            throw new FileNotFoundException($"{path} does not exist");
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }
        catch(Exception e)
        {
            Debug.LogError("Error occured when trying to load the configs file from directory: " + path + "\n" + e);
            throw e;
        }
    }
}

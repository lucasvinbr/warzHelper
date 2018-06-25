// PersistenceHandler

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// This script controls all the saving and loading procedures that require XML stuff
/// </summary>
public class PersistenceHandler
{

    public static string gamesDirectory
    {
        get
        {
            return Application.streamingAssetsPath + "/savedGames/";
        }
    }

    public static string templatesDirectory
    {
        get
        {
            return Application.streamingAssetsPath + "/savedTemplates/";
        }
    }

    public static bool IsAValidFilename(string theName)
    {
        if (string.IsNullOrEmpty(theName)) return false;

        if (theName.Contains("?") || theName.Contains(":") || theName.Contains("/") || theName.Contains("\\") ||
            theName.Contains("\"") || theName.Contains("<") || theName.Contains(">") || theName.Contains(".") ||
            theName.Contains("*") || theName.Contains("|")) return false;

        return true;
    }

    /// <summary>
    /// does a LoadFromFile on each file found in the directory;
    /// returns the successfully loaded data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="directory"></param>
    /// <returns></returns>
    public static List<T> LoadFromAllFilesInDirectory<T>(string directory)
    {
        if (Directory.Exists(directory))
        {
            string[] filesInDir = Directory.GetFiles(directory);
            List<T> returnedList = new List<T>();
            for(int i = 0; i < filesInDir.Length; i++)
            {
                T dataFromFile = LoadFromFile<T>(filesInDir[i]);
                if(dataFromFile != null)
                {
                    returnedList.Add(dataFromFile);
                }
            }

            return returnedList;
        }
        else
        {
            Debug.LogWarning("[PersistenceHandler] load from all files in directory " + directory + " aborted because the directory does not exist");
        }

        return null;
    }

    public static T LoadFromFile<T>(string fileName)
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            if (File.Exists(fileName))
            {
                using (FileStream readStream = new FileStream(fileName, FileMode.Open))
                {
                    try
                    {
                        T loadedData = (T)serializer.Deserialize(readStream);
                        readStream.Close();
                        return loadedData;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("an error occurred when trying to read file " + fileName + " as a xml! error: " + e.ToString());
                        readStream.Close();
                        return default(T);
                    }

                }
            }
            else return default(T);
        }
        catch (Exception e)
        {
            Debug.LogError("an error occurred when trying to load xml file " + fileName + "! error: " + e.ToString());
            return default(T);
        }
    }

    public static T LoadFromFile<T>(TextAsset textAssetFile)
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(textAssetFile.text))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("an error occurred when trying to load textAsset xml file " + textAssetFile.name + "! error: " + e.ToString());
            return default(T);
        }
    }

    public static void SaveToFile<T>(T dataToSave, string filePath, bool notifyMsg = true)
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, dataToSave);
                writer.Close();
                if (notifyMsg)
                {
                    Debug.Log("saved at: " + filePath);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("an error occurred while trying to save game data! error: " + e.ToString());
        }

    }

    public static void DeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
            Debug.Log("deleted file at " + filePath);
        }catch(Exception e)
        {
            Debug.LogError("an error occurred while trying to delete data! error: " + e.ToString());
        }
    }
}


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
                FileStream readStream = new FileStream(fileName, FileMode.Open);
                T loadedData = (T)serializer.Deserialize(readStream);
                readStream.Close();
                return loadedData;
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

    public static void SaveToFile<T>(T dataToSave, string fileName, bool notifyMsg = true, string subDirectory = null)
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            string filePath = Application.streamingAssetsPath + fileName + ".xml";

            if (subDirectory != null)
            {
                if (!Directory.Exists(Application.streamingAssetsPath + "/" + subDirectory + "/"))
                {
                    Directory.CreateDirectory(Application.streamingAssetsPath + "/" + subDirectory + "/");
                }

                filePath = Application.streamingAssetsPath + "/" + subDirectory + "/" + fileName + ".xml";
            }
            

            

            StreamWriter writer = new StreamWriter(filePath);
            serializer.Serialize(writer, dataToSave);
            writer.Close();
            if (notifyMsg)
            {
                Debug.LogError("saved at: " + filePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("an error occurred while trying to save game data! error: " + e.ToString());
        }

    }
}


using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

#if UNITY_2020_1_OR_NEWER
using UnityEngine;
#endif

namespace CustomScriptMakerUtility
{
    /// <summary>
    /// Contains helper methods for de/serializing objects.
    /// <para>
    /// Note that not all objects are serializable. 
    /// <br>By default this utility will only attempt to serialize using CSharps's and Unity's built in serializers.</br>
    /// <br>No intermediate conversions are done for incompatible objects.</br>
    /// </para>
    /// <para>The JSON serialization methods are exlusive to Unity engine.</para>
    /// </summary>
    public static class SerializationUtils
    {
        /// <summary>Tries to save object in binary format.</summary>
        /// <returns>True if the object was successfully saved and false if it fails.</returns>
        public static bool TrySaveToBinary<T>(string path, T dataObject) 
        {
            try
            {
                using FileStream fs = File.Open(path, FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, dataObject);
                fs.Seek(0, SeekOrigin.Begin);
                return true;
            }
            catch
            {
                return false;
            }
        }

        ///<summary>Tries load object from a binary file.</summary>
        /// <returns>True if the object was successfully loaded and false if it fails.</returns>
        public static bool TryLoadFromBinary<T>(string path, out T dataObject) 
        {
            dataObject = default;
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using FileStream fs = File.Open(path, FileMode.Open);
                fs.Seek(0, SeekOrigin.Begin);
                dataObject = (T)formatter.Deserialize(fs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Tries to save object in XML format.</summary>
        /// <returns>True if the object was successfully saved and false if it fails.</returns>
        public static bool TrySaveToXML<T>(string path, T dataObject) 
        {
            try
            {
                using StreamWriter writer = new StreamWriter(path);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, dataObject);
                return true;
            }
            catch
            {
                return false;
            }
        }

        ///<summary>Tries load object from a XML file.</summary>
        /// <returns>True if the object was successfully loaded and false if it fails.</returns>
        public static bool TryLoadFromXML<T>(string path, out T dataObject)
        {
            dataObject = default;
            try
            {
                using StreamReader reader = new StreamReader(path);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                dataObject = (T)serializer.Deserialize(reader);
                return true;
            }
            catch
            {
                return false;
            }
        }

#if UNITY_2022_1_OR_NEWER
        ///<summary>Tries to save object in json format</summary>
        /// <returns>True if the object was successfully saved and false if it fails.</returns>
        public static bool TrySaveToJSON<T>(string path, T dataObject)
        {
            try
            {
                string jsonData = JsonUtility.ToJson(dataObject);
                File.WriteAllText(path, jsonData);
                return true;
            }
            catch
            {
                return false;
            }

        }

        ///<summary>Tries load object from a json file.</summary>
        /// <returns>True if the object was successfully loaded and false if it fails.</returns>
        public static bool TryLoadFromJSON<T>(string path, out T dataObject)
        {
            dataObject = default;
            try
            {
                if (!File.Exists(path))
                    return false;

                string jsonData = File.ReadAllText(path);
                if (string.IsNullOrEmpty(jsonData))
                    return false;
                dataObject = JsonUtility.FromJson<T>(jsonData);
                return true;
            }
            catch
            {
                return false;
            }
        }
#endif

    }
}
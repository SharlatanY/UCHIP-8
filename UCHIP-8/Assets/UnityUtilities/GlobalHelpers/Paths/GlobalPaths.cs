using System.IO;
using UnityEngine;

namespace UnityUtilities.GlobalHelpers.Paths
{
  public static class GlobalPaths
  {
    /// <summary>
    /// Returns the full path of the exe.
    /// </summary>
    public static string FullApplicationPath
    {
      get { return Path.GetFullPath("."); }
    }
    /// <summary>
    /// Gets correct streaming asset path, no matter the system. Unity property Application.streaminAssetPath 
    /// returns path with slashes instead of backslashes on windows. This property fixes that.
    /// </summary>
    /// <value>
    /// The fixed streaming asset path.
    /// </value>
    public static string FixedStreamingAssetPath
    {
      get
      {
        var streamingAssetPath = Application.streamingAssetsPath;
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        //On Windows, Unity still returns the path with forwards slashes for Application.streamingAssetPath but windows works with backslashes
        //(although forward slashes should be ok in newer versions. But don't want a mix when e.g. later on combining path with Path.Combine, which uses backslashes on Win.
        streamingAssetPath = streamingAssetPath.Replace("/", "\\");
#endif
        return streamingAssetPath;
      }
    }

    /// <summary>
    /// Gets correct data path, no matter the system. Unity property Application.dataPath 
    /// returns path with slashes instead of backslashes on windows. This property fixes that.
    /// </summary>
    /// <value>
    /// The fixed data path.
    /// </value>
    public static string FixedDataPath
    {
      get
      {
        var dataPath = Application.dataPath;
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        //On Windows, Unity still returns the path with forwards slashes for Application.dataPath but windows works with backslashes
        //(although forward slashes should be ok in newer versions. But don't want a mix when e.g. later on combining path with Path.Combine, which uses backslashes on Win.
        dataPath = dataPath.Replace("/", "\\");
#endif
        return dataPath;
      }
    }

    /// <summary>
    /// Gets correct data path, no matter the system. Unity property Application.persistentDataPath 
    /// returns path with slashes instead of backslashes on windows. This property fixes that.
    /// </summary>
    /// <value>
    /// The fixed persistent data path.
    /// </value>
    public static string FixedPersistentDataPath
    {
      get
      {
        var persistentDataPath = Application.persistentDataPath;
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        //On Windows, Unity still returns the path with forwards slashes for Application.dataPath but windows works with backslashes
        //(although forward slashes should be ok in newer versions. But don't want a mix when e.g. later on combining path with Path.Combine, which uses backslashes on Win.
        persistentDataPath = persistentDataPath.Replace("/", "\\");
#endif
        return persistentDataPath;
      }
    }

    /// <summary>
    /// Gets correct data path, no matter the system. Unity property Application.temporaryCachePath 
    /// returns path with slashes instead of backslashes on windows. This property fixes that.
    /// </summary>
    /// <value>
    /// The fixed temporary cache path.
    /// </value>
    public static string FixedTemporaryCachePath
    {
      get
      {
        var temporaryCachePath = Application.temporaryCachePath;
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        //On Windows, Unity still returns the path with forwards slashes for Application.dataPath but windows works with backslashes
        //(although forward slashes should be ok in newer versions. But don't want a mix when e.g. later on combining path with Path.Combine, which uses backslashes on Win.
        temporaryCachePath = temporaryCachePath.Replace("/", "\\");
#endif
        return temporaryCachePath;
      }
    }
  }
}
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

public class BundleChecker : MonoBehaviour
{
    public string BundleFolder;
    
    public void CheckEm()
    {
        // var bundlePaths = Directory.GetFiles(Application.dataPath + "/ToCheck");
        var bundlePaths = Directory.GetFiles(BundleFolder);

        foreach (var bp in bundlePaths)
        {
            var extension = Path.GetExtension(bp);
            if (extension == ".meta" || extension == ".manifest")
            {
                continue;
            }
            
            var assBun = AssetBundle.LoadFromFile(bp);
            Debug.Log(assBun.name);
            foreach (var assName in assBun.GetAllAssetNames())
            {
                Debug.Log(assName);
            }
        }
        
    }
}

#endif
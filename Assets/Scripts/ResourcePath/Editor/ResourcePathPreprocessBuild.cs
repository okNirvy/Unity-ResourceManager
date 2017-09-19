#if UNITY_5_6_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using System;
using UnityEditor;

public class ResourcePathPreprocessBuild : IPreprocessBuild
{
    public int callbackOrder {
        get {
            throw new NotImplementedException();
        }
    }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        ResourcePath.instance.LoadXML();
    }
}
#endif

using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;
using ResourceAsset = ResourcePath.ResourceAsset;

[CustomEditor(typeof(ResourcePath))]
public class ResourcePathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ShowFields();
    }

    void ShowFields()
    {
        var resourcePath = (ResourcePath)serializedObject.targetObject;
        resourcePath.LoadXML();

        var list = new List<ResourceAsset>();
        var isUpdated = false;
        TryUpdateValues(serializedObject.targetObject, "", ref list, out isUpdated);
        if (isUpdated) {
            resourcePath.UpdateXML(list);
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.Refresh();
        }
    }

    object TryUpdateValues(object obj,
                           string hierarchy,
                           ref List<ResourceAsset> resourceList,
                           out bool isUpdate)
    {
        var fields = obj.GetType().GetFields();
        var isUpdated = false;

        foreach (var field in fields) {
            if (!field.IsStatic && field.IsPublic) {
                if (field.FieldType == typeof(ResourceAsset)) {
                    isUpdated = isUpdated |
                        ShowResourceAssetField(obj,
                                               field,
                                               CombinePath(hierarchy, field.Name),
                                               ref resourceList);
                } else if (field.FieldType.IsValueType && field.IsPublic) {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(GetVariableNameLabel(field.Name), EditorStyles.boldLabel);
                    var nextIsUpdated = false;
                    var o = TryUpdateValues(field.GetValue(obj),
                                            CombinePath(hierarchy, field.Name),
                                            ref resourceList,
                                            out nextIsUpdated);
                    field.SetValue(obj, o);
                    isUpdated = isUpdated || nextIsUpdated;
                }
            }
        }

        isUpdate = isUpdated;
        return obj;
    }

    bool ShowResourceAssetField(object obj,
                                FieldInfo field,
                                string hierarchy,
                                ref List<ResourceAsset> resourceList)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(GetVariableNameLabel(field.Name), GUILayout.Width(60));
        EditorGUI.indentLevel++;

        var resourceAsset = (ResourceAsset)field.GetValue(obj);
        var prevGUID = resourceAsset._guid;
        var guid = resourceAsset._guid;
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var asset = AssetDatabase.LoadAssetAtPath<Object>(path);

        resourceAsset._name = EditorGUILayout.TextField(hierarchy);

        asset = EditorGUILayout.ObjectField(asset, typeof(Object), false);
        path = AssetDatabase.GetAssetPath(asset);
        guid = AssetDatabase.AssetPathToGUID(path);

        if (!path.Contains("Resources/")) {
            if (asset != null) {
                Debug.LogError(string.Format("Error!:{0:G} is not resource file.", asset.name));
            }
            asset = null;
            guid = "";
        }

        resourceAsset._guid = guid;

        EditorGUI.indentLevel--;
        EditorGUILayout.EndHorizontal();

        field.SetValue(obj, resourceAsset);
        var newGUID = resourceAsset._guid;

        resourceList.Add(resourceAsset);

        return (prevGUID != newGUID);
    }

    string GetVariableNameLabel(string variableName)
    {
        if (!string.IsNullOrEmpty(variableName)) {
            if (variableName[0].Equals('_')) {
                variableName = variableName.Remove(0, 1);
            }

            if (variableName.Length > 0) {
                variableName = variableName[0].ToString().ToUpper() + variableName.Remove(0, 1);
            }
        }

        return variableName;
    }

    string CombinePath (string path1, string path2)
    {
        return Path.Combine(path1, path2).Replace ("\\", "/");
    }
}

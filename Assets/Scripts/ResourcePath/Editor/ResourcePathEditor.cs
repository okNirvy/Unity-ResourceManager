using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
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
						                       Path.Combine(hierarchy, field.Name),
						                       ref resourceList);
				} else if (field.FieldType.IsValueType && field.IsPublic) {
					EditorGUILayout.Space();
					EditorGUILayout.LabelField(field.Name);
					var nextIsUpdated = false;
					var o = TryUpdateValues(field.GetValue(obj),
					                        Path.Combine(hierarchy, field.Name),
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
		EditorGUILayout.LabelField(field.Name, GUILayout.Width(60));
		EditorGUI.indentLevel++;

		var resourceAsset = (ResourceAsset)field.GetValue(obj);
		var prevGUID = resourceAsset._guid;
		var guid = resourceAsset._guid;
		var path = AssetDatabase.GUIDToAssetPath(guid);
		var asset = AssetDatabase.LoadAssetAtPath<Object>(path);

		resourceAsset._name = EditorGUILayout.TextField (hierarchy);

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
}

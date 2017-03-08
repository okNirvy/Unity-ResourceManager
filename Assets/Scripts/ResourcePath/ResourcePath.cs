using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using UnityEngine;

public partial class ResourcePath : ScriptableObject
{
	public struct ResourceAsset
	{
		public string _name;
		public string _path;
		public string _guid;

		public override string ToString()
		{
			return _path;
		}
	}


	static public readonly string xmlPath = "ResourcePath/ResourcePathData";
	static ResourcePath s_instance;
	static public ResourcePath instance {
		get {
			if (s_instance == null) {
				s_instance = Resources.Load<ResourcePath>("ResourcePath/ResourcePath");
			}
			return s_instance;
		}
	}


	void OnEnable()
	{
		LoadXML();
	}

	public void LoadXML()
	{
		var serializer = new XmlSerializer(typeof(ResourceAsset[]));
		var textAsset = Resources.Load<TextAsset>(xmlPath);
		if (textAsset == null) {
			return;
		}
		var reader = new StringReader(textAsset.text);
		var deserialized = (ResourceAsset[])serializer.Deserialize(reader);
		reader.Close();

		var resourceList = new List<ResourceAsset>();
		SetValues(this, "", deserialized, ref resourceList);
		UpdateXML(resourceList);
	}

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public void UpdateXML(List<ResourceAsset> resourceList)
	{
#if UNITY_EDITOR
		var path = Path.Combine("Assets/Resources", xmlPath + ".xml");
		var directory = Path.GetDirectoryName(path);
		if (!Directory.Exists(directory)) {
			Directory.CreateDirectory(directory);
		}

		var list = new List<ResourceAsset>();
		for (int i = 0; i < resourceList.Count; i++) {
			var resourceAsset = resourceList[i];
			resourceAsset._path = AssetPathToResourcePath(
				UnityEditor.AssetDatabase.GUIDToAssetPath(resourceAsset._guid)
			);
			list.Add(resourceAsset);
		}

		var array = list.ToArray();
		var serialzier = new XmlSerializer(array.GetType());
		var writer = new StreamWriter(path);
		serialzier.Serialize(writer, array);

		writer.Close();
#endif
	}

	object SetValues(object obj,
					 string hierarchy,
					 ResourceAsset[] deserialized,
					 ref List<ResourceAsset> resourceList)
	{
		var fields = obj.GetType().GetFields();

		for (int i = 0; i < fields.Length; i++) {
			var field = fields[i];
			if (field.IsStatic || !field.IsPublic) {
				continue;
			}

			if (field.FieldType == typeof(ResourceAsset)) {
				if (TrySetLoadedValue (obj, field, hierarchy, deserialized)) {
					resourceList.Add ((ResourceAsset)field.GetValue (obj));
				} else {
					Debug.Log ("Error! : " + obj);
				}
			} else if (field.FieldType.IsValueType && field.IsPublic) {
				field.SetValue(obj, SetValues(field.GetValue(obj),
											  Path.Combine(hierarchy, field.Name),
											  deserialized,
											  ref resourceList));
			}
		}

		return obj;
	}

	bool TrySetLoadedValue(object obj,
						FieldInfo fieldInfo,
						string hierarchy,
						ResourceAsset[] deserialized)
	{
		for (int i = 0; i < deserialized.Length; i++) {
			var resourceAsset = deserialized[i];
			if (string.IsNullOrEmpty (resourceAsset._name)) {
				continue;
			}

			if (resourceAsset._name.Equals(Path.Combine(hierarchy, fieldInfo.Name))) {
#if UNITY_EDITOR
				resourceAsset._path = AssetPathToResourcePath(
					GUIDToAssetPath(resourceAsset._guid)
				);
#endif
				fieldInfo.SetValue(obj, resourceAsset);
				return true;
			}
		}

		return false;
	}

	string AssetPathToGUID(string path)
	{
#if UNITY_EDITOR
		return UnityEditor.AssetDatabase.AssetPathToGUID(path);
#else
		return path;
#endif
	}

	string GUIDToAssetPath(string guid)
	{
#if UNITY_EDITOR
		return UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
#else
		return guid;
#endif
	}

	string AssetPathToResourcePath(string assetPath)
	{
		var idx = assetPath.IndexOf("Resources/", System.StringComparison	.Ordinal);
		if (idx < 0) {
			return "";
		}
		var path = assetPath.Remove(0, idx + 10);
		var extension = Path.GetExtension(path);
		if (!string.IsNullOrEmpty(extension)) {
			path = path.Remove(path.Length - extension.Length, extension.Length);
		}

		return path;
	}
}

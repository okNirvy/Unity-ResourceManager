using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleResourcePath : MonoBehaviour
{
	void Start ()
	{
		var res1path = ResourcePath.instance._scene1._asset1.ToString();
		var resource = Resources.Load<GameObject>(res1path);
		if (resource) {
			Instantiate(resource);
		} else {
			Debug.Log(res1path + " is null");
		}
	}
}

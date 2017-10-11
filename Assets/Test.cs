using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {
	
	[SerializeField]
	private Vector2 _pt=new Vector2(0,0);

	public Vector2 point{
		get { return _pt; }
		set { _pt=value; }
	}


}

using UnityEngine;
using System.Collections;

public class TestGraph : MonoBehaviour 
{
	void Start () 
	{
		Graph.YMin = -2;
		Graph.YMax = +2;
 
		Graph.channel[ 0 ].isActive = true;
		Graph.channel[ 1 ].isActive = true;
	}
 
 
	void Update () {
		Graph.channel[ 0 ].Feed( Mathf.Sin( Time.time ) );
	}
 
	void FixedUpdate( ) {
		Graph.channel[ 1 ].Feed( Mathf.Sin( Time.time ) );
	}
 
}
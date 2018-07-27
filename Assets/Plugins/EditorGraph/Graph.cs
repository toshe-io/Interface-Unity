using UnityEngine;
using System.Collections;

public class Graph 
{
	public static float YMin = -1, YMax = +1;
 
	public const int MAX_HISTORY = 1024;
	public const int MAX_CHANNELS = 3;
 
	public static Channel [] channel = new Channel[ MAX_CHANNELS ];
 
	static Graph()
	{
		Graph.channel[ 0 ] = new Channel( Color.red );
		Graph.channel[ 1 ] = new Channel( Color.green );
		Graph.channel[ 2 ] = new Channel( Color.blue );
	} 
 
}
using UnityEngine;
using System.Collections;
 
public class Channel
{
	public float[] _data = new float[ Graph.MAX_HISTORY ];
	public Color _color = Color.white;
	public bool isActive = false;
 
	public Channel( Color _C ) {
		_color = _C;
	}
 
	public void Feed( float val )
	{
		for( int i = Graph.MAX_HISTORY - 1;  i >= 1;  i-- )
			_data[ i ] = _data[ i-1 ];
 
		_data[ 0 ] = val;
	}
}
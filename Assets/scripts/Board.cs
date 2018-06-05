using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Board {
	/// <summary>
	/// the board's x and y dimensions. Affects camera boundaries.
	/// doesn't affect zone placement, but may make some zones inacessible to the player!
	/// </summary>
	public Vector2 dimensions;

	/// <summary>
	/// the path to a "ground" texture used in this board. Can be a map or something depicting a landscape, for example
	/// </summary>
	public string boardTexturePath = "";

}


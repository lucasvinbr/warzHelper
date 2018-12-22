using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkLine : MonoBehaviour {

	public ZoneSpot[] zonesLinked;

	private LineRenderer line;

	/// <summary>
	/// an offset to make sure the lines don't z-fight with the ground and remain visible
	/// </summary>
	public const float LINE_Y_OFFSET = 0.1f;

	private void Start() {
		GuardSetup();
	}

	/// <summary>
	/// makes sure our data is set
	/// </summary>
	private void GuardSetup() {
		if (!line) {
			line = GetComponent<LineRenderer>();
			zonesLinked = new ZoneSpot[2];
		}
	}

	public void SetLink(ZoneSpot z1, ZoneSpot z2) {
		GuardSetup();
		zonesLinked[0] = z1;
		zonesLinked[1] = z2;
		line.SetPosition(0, z1.transform.position + Vector3.up * LINE_Y_OFFSET);
		line.SetPosition(1, z2.transform.position + Vector3.up * LINE_Y_OFFSET);
	}

	/// <summary>
	/// true if this link is connected to the target zone
	/// </summary>
	/// <param name="targetZone"></param>
	/// <returns></returns>
	public bool LinksZone(Zone targetZone) {
		foreach(ZoneSpot zs in zonesLinked) {
			if(zs.data == targetZone) {
				return true;
			}
		}

		return false;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a class wrapping around a line renderer that links between two troopContainer3ds
/// </summary>
public class LinkLine : MonoBehaviour {

	public TroopContainer3d[] linkedContainers;

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
			linkedContainers = new TroopContainer3d[2];
		}
	}

	public void SetLink(TroopContainer3d c1, TroopContainer3d c2, Color? color = null, float lineWidth = 0.1f) {
		GuardSetup();
		
		linkedContainers[0] = c1;
		linkedContainers[1] = c2;

		line.startColor = color ?? Color.white;
		line.endColor = color ?? Color.white;

		line.startWidth = lineWidth;
		line.endWidth = lineWidth;

		UpdatePositions();
	}

	public void UpdatePositions() {
		line.SetPosition(0, linkedContainers[0].transform.position + Vector3.up * LINE_Y_OFFSET);
		line.SetPosition(1, linkedContainers[1].transform.position + Vector3.up * LINE_Y_OFFSET);
	}

	/// <summary>
	/// true if this link is connected to the target container
	/// </summary>
	/// <param name="targetCont"></param>
	/// <returns></returns>
	public bool LinksContainer(TroopContainer targetCont) {
		foreach(TroopContainer3d zs in linkedContainers) {
			if(zs.data == targetCont) {
				return true;
			}
		}

		return false;
	}
}

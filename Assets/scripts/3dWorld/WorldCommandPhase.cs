using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// script that makes hovering a zone or commander show information about their troops
/// </summary>
public class WorldCommandPhase : MonoBehaviour {

    private Camera cam;
    private RaycastHit hit;

	public CommandPhaseMan cmdPhaseMan;

	public Transform cmderHighlight;

	public Commander curSelectedCmder = null;

	[HideInInspector]
	public List<Cmder3d> allowedCmders3d = new List<Cmder3d>();

	[HideInInspector]
	public List<ZoneSpot> allowedMoveSpots = new List<ZoneSpot>();

	public ZoneGrowOnHover zoneGrowScript;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
		if (GameInterface.openedPanelsOverlayLevel != 0) {
			return;
		}

		foreach(Cmder3d cmd3d in allowedCmders3d) {
			//commanders which haven't taken an action this turn will keep spinning
			cmd3d.transform.Rotate(Vector3.up * 100 * Time.deltaTime);
		}

		//we still want the spinning commanders if no overlay panel is open
		if (EventSystem.current.IsPointerOverGameObject()) {
			return;
		}

		if (curSelectedCmder != null) {
			if (Input.GetButtonDown("Select")) {
				if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100)) {
					ZoneSpot hitSpotScript = hit.transform.GetComponent<ZoneSpot>();
					if (hitSpotScript) {
						if (allowedMoveSpots.Contains(hitSpotScript)) {
							//move commander to target zone spot
							cmdPhaseMan.MoveCommander(curSelectedCmder, hitSpotScript);
						}
					}
				}
			}
		}else {
			if (Input.GetButtonDown("Select")) {
				if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 9)) {
					Cmder3d hitCmder = hit.transform.GetComponent<Cmder3d>();
					if (hitCmder) {
						if (allowedCmders3d.Contains(hitCmder)) {
							SelectCmder(hitCmder);
						}
					}
				}
			}
		}

		

	}

	/// <summary>
	/// highlights the selected cmder and tells the commandPhaseMan to proceed with the selection
	/// </summary>
	/// <param name="cmd3d"></param>
	public void SelectCmder(Cmder3d cmd3d) {
		cmderHighlight.gameObject.SetActive(true);
		cmderHighlight.transform.position = cmd3d.transform.position;
		curSelectedCmder = cmd3d.data as Commander;
		cmdPhaseMan.OnCmderSelectedInWorld(cmd3d.data as Commander);
	}

	/// <summary>
	/// sets the zones that can be selected as destination for the selected commmander.
	/// also activates the "zone grow on hover" script, with a whitelist to only make those
	/// allowed zones grow
	/// </summary>
	/// <param name="allowedZones"></param>
	public void CmderSelectedSetAllowedZones(List<ZoneSpot> allowedZones) {
		allowedMoveSpots = allowedZones;

		zoneGrowScript.enabled = true;
		zoneGrowScript.zoneWhitelist = allowedZones;
	}

	public void DeselectCmder() {
		if(cmderHighlight) cmderHighlight.gameObject.SetActive(false);
		curSelectedCmder = null;
	}

	/// <summary>
	/// deselects the currently selected cmder (if any),
	/// disables the "zone grow" script
	/// and clears "allowed" lists
	/// </summary>
	public void Cleanup() {
		DeselectCmder();
		if (zoneGrowScript) zoneGrowScript.enabled = false;
		allowedCmders3d.Clear();
		allowedMoveSpots.Clear();
	}

	private void OnDisable() {
		Cleanup();
	}

}

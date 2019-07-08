using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// script that makes hovering a zone or commander show information about their troops
/// </summary>
public class WorldGarrDescOnHover : MonoBehaviour {

    private Camera cam;
    private RaycastHit hit;

	private Transform curHoveredObj;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
		if (GameInterface.openedPanelsOverlayLevel != 0 || 
			EventSystem.current.IsPointerOverGameObject()) {
			if (LayoutToolTip.Instance.gameObject.activeSelf) {
				LayoutToolTip.Instance.HideTooltip();
			}
			return;
		}
		
		if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 
			(1 << 8) | (1 << 9))) {
			if (!curHoveredObj || curHoveredObj != hit.transform) {
				LayoutToolTip.Instance.HideTooltip();
				curHoveredObj = hit.transform;
				ZoneSpot spotScript = curHoveredObj.GetComponent<ZoneSpot>();
				if (spotScript) {
					LayoutToolTip.Instance.ShowTooltipForZone
						(spotScript.data as Zone, Input.mousePosition, 
						GameInterface.instance.curInterfaceMode == GameInterface.InterfaceMode.game);
				}else {
					Cmder3d cmderScript = curHoveredObj.GetComponent<Cmder3d>();
					if (cmderScript) {
						LayoutToolTip.Instance.ShowTooltipForCmder(cmderScript.data as Commander, Input.mousePosition);
					}
				}
			}
		}else {
			if (curHoveredObj) {
				curHoveredObj = null;
				LayoutToolTip.Instance.HideTooltip();
			}
		}

    }


	private void OnDisable() {
		curHoveredObj = null;
		LayoutToolTip.Instance?.HideTooltip();
	}

}

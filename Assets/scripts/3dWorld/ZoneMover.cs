using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ZoneMover : MonoBehaviour {

    private Camera cam;
    private RaycastHit hit;

	private List<ZoneSpot> zoneSpotsBeingMoved = new List<ZoneSpot>();

	private List<Vector3> positionsBeforeMovement = new List<Vector3>();

	private List<ZoneSpot> allZoneSpots;

	/// <summary>
	/// position where the dragging action started
	/// </summary>
	private Vector3 dragActionStartPos;

	private Vector3 reconvertedDragStartPos;

	/// <summary>
	/// dragging has a different meaning whether you're selecting or moving selected zones
	/// </summary>
	public bool draggingAction = false;

	private Quaternion pivotRotationBeforeRotEdit;

	public Transform moverPivot;

	private Canvas boxSelectorCanvas;
	/// <summary>
	/// input.mousepos, but with Z set to the distance between the camera and the ground
	/// (this makes doing ScreenToWorld on it give a nice "grounded" position to us)
	/// </summary>
	private Vector3 groundedMousePos;
	/// <summary>
	/// input.mousepos, but divided by the canvas's scaling factor,
	/// turning screen pos into "canvas pos"
	/// </summary>
	private Vector3 canvasScaledMousePos;
	
	public RectTransform boxSelectorRect;
	private Vector2 selectorRectDelta;
	private Vector2 boxSelectorPivot;
	private ZoneSpot singleSelectionSpot;

    void Start()
    {
        cam = Camera.main;
		boxSelectorCanvas = boxSelectorRect.GetComponentInParent<Canvas>();
    }

	/// <summary>
	/// enables the script with no zones selected.
	/// the "box selection mode" will then start
	/// </summary>
	public void StartSelectionProcedure() {
		World.instance.zoneEditOnClickScript.enabled = false;
		enabled = true;

		//store all the zoneSpots so that we don't have to get them every frame we're using the selector box
		allZoneSpots = GameController.ZonesToZoneSpots(GameController.instance.curData.zones);
	} 

	/// <summary>
	/// parents the zones being moved to a centralized "pivot" transform,
	/// which is then moved around with the mouse to set the zones' new position
	/// </summary>
	/// <param name="targetZones"></param>
	public void StartMovingProcedure(List<ZoneSpot> targetZones) {
		Vector3 pivotPoint = Vector3.zero;

		foreach(ZoneSpot zs in targetZones) {
			positionsBeforeMovement.Add(zs.transform.position);
			pivotPoint += zs.transform.position;
		}

		pivotPoint /= targetZones.Count;
		moverPivot.position = pivotPoint;

		foreach (ZoneSpot zs in targetZones) {

			zs.transform.SetParent(moverPivot);
			
		}

		enabled = true;
	}

	public void DoneMoving() {
		enabled = false;
		World.instance.zoneEditOnClickScript.enabled = true;
		TemplateModeUI templateUI = GameInterface.instance.templateModeUI as TemplateModeUI;
		templateUI.SetDisplayedLowerHUD(templateUI.mainLowerHUD);
	}

	private void OnDisable() {
		ClearMovingZoneSpots();
		boxSelectorRect.gameObject.SetActive(false);
	}

	/// <summary>
	/// clears the "moving zones" list,
	/// optionally consolidating the edited zone positions or reverting them
	/// </summary>
	/// <param name="consolidatePositions">sets the zones' data to the new coords if true; reverts the coords if false</param>
	public void ClearMovingZoneSpots(bool consolidatePositions = false) {
		foreach (ZoneSpot zs in zoneSpotsBeingMoved) {
			zs.transform.SetParent(World.instance.zonesContainer);
			zs.transform.rotation = Quaternion.identity;
			zs.Highlighted = false;
			if (consolidatePositions) {
				(zs.data as Zone).UpdateCoordsAccordingToSpotPosition();
				zs.RefreshDataDisplay();
				zs.RefreshPositionRelatedStuff();
			}
		}

		if (!consolidatePositions) RestoreZonePositions();

		zoneSpotsBeingMoved.Clear();
		positionsBeforeMovement.Clear();
		moverPivot.rotation = Quaternion.identity;
	}

	/// <summary>
	/// this loop should only run once there are zones in the zoneSpotsBeingMoved list
	/// </summary>
	void MovementModeLoop() {
		if (Input.GetButtonDown("Alt Select")) {
			draggingAction = true;
			dragActionStartPos = Input.mousePosition;
			pivotRotationBeforeRotEdit = moverPivot.rotation;
		}
		if (draggingAction) {
			moverPivot.rotation = pivotRotationBeforeRotEdit * 
				Quaternion.Euler(Vector3.up * (Input.mousePosition.x - dragActionStartPos.x));
		}
		if (Input.GetButtonUp("Alt Select")) {
			draggingAction = false;
		}

		if (Input.GetButtonDown("Cancel")) {
			ClearMovingZoneSpots(false);
		}

		if (EventSystem.current.IsPointerOverGameObject()) {
			return;
		}

		if (!draggingAction && Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 0)) {
			moverPivot.position = hit.point;
		}

		if (Input.GetButtonDown("Select")) {
			ClearMovingZoneSpots(true);
		}

	}

	/// <summary>
	/// returns zones to where they were before the moving started.
	/// should probably be used while finishing up the movement procedure
	/// (before clearing the zoneSpotsBeingMoved list)
	/// </summary>
	public void RestoreZonePositions() {
		for(int i = 0; i < zoneSpotsBeingMoved.Count; i++) {
			zoneSpotsBeingMoved[i].transform.position = positionsBeforeMovement[i];
		}
	}



	void Update()
    {
		if(zoneSpotsBeingMoved.Count == 0) {
			SelectionModeLoop();
		}
		else {
			MovementModeLoop();
		}
    }

	/// <summary>
	/// this loop should only run while no zones are in the zoneSpotsBeingMoved list
	/// </summary>
	void SelectionModeLoop() {
		groundedMousePos = Input.mousePosition;
		groundedMousePos.z = CameraPanner.DIST_TO_GROUND;
		canvasScaledMousePos = groundedMousePos / boxSelectorCanvas.scaleFactor;

		if (Input.GetButtonDown("Select") && !EventSystem.current.IsPointerOverGameObject()) {
			singleSelectionSpot = null;
			draggingAction = true;
			dragActionStartPos = cam.ScreenToWorldPoint(groundedMousePos);
			boxSelectorRect.pivot = Vector2.zero;
			boxSelectorRect.anchoredPosition = canvasScaledMousePos;
			boxSelectorRect.gameObject.SetActive(true);

			//also let the player pick only one zone by clicking it
			if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 8)) {
				ZoneSpot hitSpotScript = hit.transform.GetComponent<ZoneSpot>();
				singleSelectionSpot = hitSpotScript;
			}
		}

		if (draggingAction) {
			reconvertedDragStartPos = cam.WorldToScreenPoint(dragActionStartPos) / boxSelectorCanvas.scaleFactor;
			//flip pivots in x and y if we go the other way with the mouse
			boxSelectorPivot.x = reconvertedDragStartPos.x > canvasScaledMousePos.x ? 1.0f : 0.0f;
			boxSelectorPivot.y = reconvertedDragStartPos.y > canvasScaledMousePos.y ? 1.0f : 0.0f;
			boxSelectorRect.pivot = boxSelectorPivot;
			//keep the selector rect's origin from following the camera
			boxSelectorRect.anchoredPosition = reconvertedDragStartPos;
			//do a "Vector2 abs" to always get positive width and height 
			//(pivot changes make sure the box grows in the right direction)
			selectorRectDelta.x = Mathf.Abs(canvasScaledMousePos.x - boxSelectorRect.anchoredPosition.x);
			selectorRectDelta.y = Mathf.Abs(canvasScaledMousePos.y - boxSelectorRect.anchoredPosition.y);
			boxSelectorRect.sizeDelta = selectorRectDelta;

			//highlight zones that would be selected if the player stopped dragging right now
			CheckZonesToSelectWithBox(false);
		}

		if (Input.GetButtonUp("Select") && draggingAction) {
			draggingAction = false;
			CheckZonesToSelectWithBox(true);
			boxSelectorRect.sizeDelta = Vector2.zero;
			boxSelectorRect.gameObject.SetActive(false);

			//single zone click end
			if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 8)) {
				ZoneSpot hitSpotScript = hit.transform.GetComponent<ZoneSpot>();
				//if click started and ended with this zone, we suppose the player wants to move this one only
				if(singleSelectionSpot == hitSpotScript) {
					zoneSpotsBeingMoved.Add(singleSelectionSpot);
					singleSelectionSpot.Highlighted = true;
					StartMovingProcedure(zoneSpotsBeingMoved);
				}
			}

			singleSelectionSpot = null;
		}
	}

	/// <summary>
	/// checks which zones are inside the selector rect
	/// </summary>
	/// <param name="actuallySelect">should we actually add the zones to the selected list or only "preview" the selection?</param>
	public void CheckZonesToSelectWithBox(bool actuallySelect) {
		if (!boxSelectorRect.gameObject.activeSelf) return;

		foreach(ZoneSpot zs in allZoneSpots) {
			if(RectTransformUtility.RectangleContainsScreenPoint(boxSelectorRect, RectTransformUtility.WorldToScreenPoint(cam, zs.transform.position))) {
				zs.Highlighted = true;
				if (actuallySelect) zoneSpotsBeingMoved.Add(zs);
			}else {
				zs.Highlighted = false;
			}
		}

		if(actuallySelect && zoneSpotsBeingMoved.Count > 0) {
			StartMovingProcedure(zoneSpotsBeingMoved);
		}
	}

}

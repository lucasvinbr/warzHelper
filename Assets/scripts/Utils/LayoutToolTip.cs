
//ToolTip is written by Emiliano Pastorelli, H&R Tallinn (Estonia), http://www.hammerandravens.com
//Copyright (c) 2015 Emiliano Pastorelli, H&R - Hammer&Ravens, Tallinn, Estonia.
//All rights reserved.

//Redistribution and use in source and binary forms are permitted
//provided that the above copyright notice and this paragraph are
//duplicated in all such forms and that any documentation,
//advertising materials, and other materials related to such
//distribution and use acknowledge that the software was developed
//by H&R, Hammer&Ravens. The name of the
//H&R, Hammer&Ravens may not be used to endorse or promote products derived
//from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED ``AS IS'' AND WITHOUT ANY EXPRESS OR
//IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.

using System.Collections.Generic;
/// Credit drHogan 
/// Sourced from - http://forum.unity3d.com/threads/screenspace-camera-tooltip-controller-sweat-and-tears.293991/#post-1938929
/// updated ddreaper - refactored code to be more performant.
/// updated lucasvinbr - mixed with BoundTooltip, should work with Screenspace Camera (non-rotated) and Overlay
/// *Note - only works for non-rotated Screenspace Camera and Screenspace Overlay canvases at present, needs updating to include rotated Screenspace Camera and Worldspace!
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LayoutToolTip : MonoBehaviour
{
    //text of the tooltip
    private RectTransform _rectTransform, canvasRectTransform;

    [Tooltip("The canvas used by the tooltip as positioning and scaling reference. Should usually be the root Canvas of the hierarchy this component is in")]
    public Canvas canvas;

    [Tooltip("Sets if tooltip triggers will run ForceUpdateCanvases and refresh the tooltip's layout group " +
        "(if any) when hovered, in order to prevent momentaneous misplacement sometimes caused by ContentSizeFitters")]
    public bool tooltipTriggersCanForceCanvasUpdate = false;

	[Tooltip("If enabled and the tooltip pivot isn't centered, " +
		"the pivot will be flipped in the appropriate direction when crossing edges. " +
		"This helps in preventing the tooltip from getting in front of the mouse")]
	public bool pivotFlipping = true;

	[Tooltip("Extra offset applied to the tooltip's position")]
	public Vector3 offset;

	/// <summary>
	/// the tooltip's Layout Group, if any
	/// </summary>
	private LayoutGroup _layoutGroup;

    //if the tooltip is inside a UI element
    private bool _inside;

	//the tooltip's dimensions, considering its pivot
    private float rightWidth, leftWidth, topHeight, bottomHeight;

	/// <summary>
	/// the tooltip rect transform's pivot before any flipping made when touching edges of the screen
	/// </summary>
	private Vector2 originalPivot;

	/// <summary>
	/// pivot applied to the tooltip
	/// </summary>
	private Vector2 adjustedPivot;

    [HideInInspector]
    public RenderMode guiMode;

    private Camera _guiCamera;

    public Camera GuiCamera
    {
        get
        {
            if (!_guiCamera) {
                _guiCamera = Camera.main;
            }

            return _guiCamera;
        }
    }

	//screen bounds, relative to the screen space context
    private Vector3 screenLowerLeft, screenUpperRight;

    /// <summary>
    /// a screen-space point where the tooltip would be placed before applying X and Y shifts and border checks
    /// </summary>
    private Vector3 baseTooltipPos;

    private Vector3 newTTPos;
    private Vector3 adjustedNewTTPos;
    private Vector3 adjustedTTLocalPos;
    private Vector3 shifterForBorders;

    private float borderTest;

	public Transform mainLayoutContentParent;

	public GameObject baseTitleEntry, baseTroopEntry, baseInfoEntry;


    // Standard Singleton Access
    private static LayoutToolTip instance;
        
    public static LayoutToolTip Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<LayoutToolTip>();
            return instance;
        }
    }

        
    void Reset() {
        canvas = GetComponentInParent<Canvas>();
        canvas = canvas.rootCanvas;
    }

    // Use this for initialization
    public void Awake()
    {
        instance = this;
        if (!canvas) {
            canvas = GetComponentInParent<Canvas>();
            canvas = canvas.rootCanvas;
        }

        _guiCamera = canvas.worldCamera;
        guiMode = canvas.renderMode;
        _rectTransform = GetComponent<RectTransform>();
		SetNewDefaultPivot(_rectTransform.pivot);
        canvasRectTransform = canvas.GetComponent<RectTransform>();
        _layoutGroup = GetComponentInChildren<LayoutGroup>();


        _inside = false;

        this.gameObject.SetActive(false);
    }

	/// <summary>
	/// sets the default pivot for the tooltip RectTransform while it isn't trespassing any screen edges
	/// (this pivot will flip if any edges are touched).
	/// Also checks if it's worth flipping the pivot in case the edges are touched
	/// </summary>
	/// <param name="newPivot"></param>
	public void SetNewDefaultPivot(Vector2 newPivot) {
		originalPivot = newPivot;
	}

	public void ShowTooltipForZone(Zone targetZone, Vector3 basePos, bool showGarrInfo = true, bool refreshCanvasesBeforeGetSize = false) {
		baseTooltipPos = basePos;
		Faction ownerFaction = GameController.GetFactionByID(targetZone.ownerFaction);
		if(ownerFaction != null) {
			AddTooltipTitleEntry(targetZone.name, ownerFaction.color, 70);
			AddTooltipTitleEntry("(" + ownerFaction.name + ")", ownerFaction.color);
			AddTooltipTitleEntry("Garrison: " + targetZone.TotalTroopsContained + "/"
				+ targetZone.MaxTroopsInGarrison, Color.white, 55);
			foreach (TroopNumberPair tnp in targetZone.troopsContained) {
				AddTooltipTroopEntry(GameController.GetTroopTypeByID(tnp.troopTypeID).name,
					tnp.troopAmount.ToString());
			}
			List<Commander> cmdersInZone = GameController.GetCommandersOfFactionInZone(targetZone, ownerFaction);
			if(cmdersInZone.Count > 0) {
				List<TroopNumberPair> totalArmy =
					GameController.GetCombinedTroopsInZoneFromFaction(targetZone, ownerFaction);
				AddTooltipTitleEntry("Commanders: " + cmdersInZone.Count.ToString(), ownerFaction.color, 60);
				AddTooltipTitleEntry("Combined Army: " + 
					GameController.GetArmyAmountFromTroopList(totalArmy), Color.white, 50);
				foreach (TroopNumberPair tnp in	totalArmy) {
					AddTooltipTroopEntry(GameController.GetTroopTypeByID(tnp.troopTypeID).name,
						tnp.troopAmount.ToString());
				}
			}
			
		}
		else {
			AddTooltipTitleEntry(targetZone.name, Color.white, 70);
			AddTooltipTitleEntry("(" + Rules.NO_FACTION_NAME + ")", Color.white);
		}

		AddTooltipInfoEntry(targetZone.multRecruitmentPoints.ToString("0%"),
			targetZone.multTrainingPoints.ToString("0%"));
		ContextualTooltipUpdate(refreshCanvasesBeforeGetSize);
	}

	public void ShowTooltipForCmder(Commander targetCmder, Vector3 basePos, bool refreshCanvasesBeforeGetSize = false) {
		baseTooltipPos = basePos;
		Faction ownerFaction = GameController.GetFactionByID(targetCmder.ownerFaction);
		AddTooltipTitleEntry("Commander", ownerFaction.color, 50);
		AddTooltipTitleEntry("(" + ownerFaction.name + ")", ownerFaction.color);
		AddTooltipTitleEntry("Troops: " + targetCmder.TotalTroopsContained + "/" + targetCmder.MaxTroopsCommanded, Color.white);
		foreach (TroopNumberPair tnp in
			targetCmder.troopsContained) {
			AddTooltipTroopEntry(GameController.GetTroopTypeByID(tnp.troopTypeID).name,
				tnp.troopAmount.ToString());
		}

		List<Commander> cmdersInZone = 
			GameController.GetCommandersOfFactionInZone(
				GameController.GetZoneByID(targetCmder.zoneIAmIn), ownerFaction);
		if (cmdersInZone.Count > 1) {
			List<TroopNumberPair> totalCmderArmy = new List<TroopNumberPair>();
			foreach (Commander c in cmdersInZone) {
				totalCmderArmy = c.GetCombinedTroops(totalCmderArmy);
			}
			
			AddTooltipTitleEntry("Combined Cmd. Army: " +
				GameController.GetArmyAmountFromTroopList(totalCmderArmy), Color.white, 50);
			foreach (TroopNumberPair tnp in totalCmderArmy) {
				AddTooltipTroopEntry(GameController.GetTroopTypeByID(tnp.troopTypeID).name,
					tnp.troopAmount.ToString());
			}
		}

		ContextualTooltipUpdate(refreshCanvasesBeforeGetSize);
	}

	//call this function on mouse exit to deactivate the template
	public void HideTooltip()
    {
        gameObject.SetActive(false);
		DestroyAllTooltipContent();
        _inside = false;
    }

	public void AddTooltipTitleEntry(string text, Color color, float rectHeight = 60.0f) {
		GameObject newEntry = Instantiate(baseTitleEntry, mainLayoutContentParent);
		newEntry.SetActive(true);
		TMP_Text entryTxt = newEntry.GetComponent<TMP_Text>();
		entryTxt.text = text;
		entryTxt.color = color;
		entryTxt.GetComponent<LayoutElement>().preferredHeight = rectHeight;
	}

	public void AddTooltipTroopEntry(string leftText, string rightText) {
		GameObject newEntry = Instantiate(baseTroopEntry, mainLayoutContentParent);
		newEntry.SetActive(true);
		LayoutTooltipTwoTextEntry entryScript = newEntry.GetComponent<LayoutTooltipTwoTextEntry>();
		entryScript.leftTxt.text = leftText;
		entryScript.rightTxt.text = rightText;
	}

	public void AddTooltipInfoEntry(string leftText, string rightText) {
		GameObject newEntry = Instantiate(baseInfoEntry, mainLayoutContentParent);
		newEntry.SetActive(true);
		LayoutTooltipTwoTextEntry entryScript = newEntry.GetComponent<LayoutTooltipTwoTextEntry>();
		entryScript.leftTxt.text = leftText;
		entryScript.rightTxt.text = rightText;
	}

	public void DestroyAllTooltipContent() {
		for (int i = 0; i < mainLayoutContentParent.childCount; i++) {
			Destroy(mainLayoutContentParent.GetChild(i).gameObject);
		}
	}

    // Update is called once per frame
    void Update()
    {
        if (_inside)
        {
            ContextualTooltipUpdate();
        }
    }

    /// <summary>
    /// forces rebuilding of Canvases in order to update the tooltip's content size fitting.
    /// Can prevent the tooltip from being visibly misplaced for one frame when being resized.
    /// Only runs if tooltipTriggersCanForceCanvasUpdate is true
    /// </summary>
    public void RefreshTooltipSize() {
        if (tooltipTriggersCanForceCanvasUpdate) {
            Canvas.ForceUpdateCanvases();

            if (_layoutGroup) {
                _layoutGroup.enabled = false;
                _layoutGroup.enabled = true;
            }
                
        }
            
    }

    /// <summary>
    /// Runs the appropriate tooltip placement method, according to the parent canvas's render mode
    /// </summary>
    /// <param name="refreshCanvasesBeforeGettingSize"></param>
    public void ContextualTooltipUpdate(bool refreshCanvasesBeforeGettingSize = false) {
        switch (guiMode) {
            case RenderMode.ScreenSpaceCamera:
                OnScreenSpaceCamera(refreshCanvasesBeforeGettingSize);
                break;
            case RenderMode.ScreenSpaceOverlay:
                OnScreenSpaceOverlay(refreshCanvasesBeforeGettingSize);
                break;
        }
    }

    //main tooltip edge of screen guard and movement - camera
    public void OnScreenSpaceCamera(bool refreshCanvasesBeforeGettingSize = false)
    {

        baseTooltipPos.z = canvas.planeDistance;

        newTTPos = GuiCamera.ScreenToViewportPoint(baseTooltipPos - offset);
        adjustedNewTTPos = GuiCamera.ViewportToWorldPoint(newTTPos);

		adjustedPivot = originalPivot;
		_rectTransform.pivot = originalPivot;

		gameObject.SetActive(true);

        if (refreshCanvasesBeforeGettingSize) RefreshTooltipSize();

		//consider scaled dimensions when comparing against the edges
		CalcTooltipVerticalDimensions(true);
		CalcTooltipHorizontalDimensions(true);

		//get screen bounds in the canvas's rect
		RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRectTransform, Vector2.zero, GuiCamera, out screenLowerLeft);
		RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRectTransform, new Vector2(Screen.width, Screen.height), GuiCamera, out screenUpperRight);

		BorderAdjustments(true);

		_rectTransform.pivot = adjustedPivot;

		//failed attempt to circumvent issues caused when rotating the camera
		adjustedNewTTPos = transform.rotation * adjustedNewTTPos;

        transform.position = adjustedNewTTPos;
        adjustedTTLocalPos = transform.localPosition;
        adjustedTTLocalPos.z = 0;
        transform.localPosition = adjustedTTLocalPos;

        _inside = true;
    }


    //main tooltip edge of screen guard and movement - overlay
    public void OnScreenSpaceOverlay(bool refreshCanvasesBeforeGettingSize = false) {

        newTTPos = (baseTooltipPos - offset) / canvas.scaleFactor;
        adjustedNewTTPos = newTTPos;

		adjustedPivot = originalPivot;
		_rectTransform.pivot = originalPivot;

		gameObject.SetActive(true);

        if (refreshCanvasesBeforeGettingSize) RefreshTooltipSize();

            
		CalcTooltipHorizontalDimensions();
		CalcTooltipVerticalDimensions();

		//screen's 0 = overlay canvas's 0 (always?)
		screenLowerLeft = Vector3.zero;
        screenUpperRight = canvasRectTransform.sizeDelta;

		BorderAdjustments();

		_rectTransform.pivot = adjustedPivot;
        //remove scale factor for the actual positioning of the TT
        adjustedNewTTPos *= canvas.scaleFactor;
        transform.position = adjustedNewTTPos;

        _inside = true;
    }

	/// <summary>
	/// sets left and right width variables according to the adjustedPivot and, optionally, world scale of the tooltip object
	/// </summary>
	public void CalcTooltipHorizontalDimensions(bool considerLossyScale = false) {
		rightWidth = _rectTransform.sizeDelta[0] * (1 - adjustedPivot[0]);
		leftWidth = _rectTransform.sizeDelta[0] * (adjustedPivot[0]);
		if (considerLossyScale) {
			rightWidth *= transform.lossyScale.x;
			leftWidth *= transform.lossyScale.x;
		}
	}

	/// <summary>
	/// sets top and bottom height variables according to the adjustedPivot and, optionally, world scale of the tooltip object
	/// </summary>
	public void CalcTooltipVerticalDimensions(bool considerLossyScale = false) {
		topHeight = _rectTransform.sizeDelta[1] * (1 - adjustedPivot[1]);
		bottomHeight = _rectTransform.sizeDelta[1] * (adjustedPivot[1]);
		if (considerLossyScale) {
			topHeight *= transform.lossyScale.y;
			bottomHeight *= transform.lossyScale.y;
		}
	}

	/// <summary>
	/// uses the tooltip's and the screen's "dimensions" variables (topHeight, screenLowerLeft etc) to check
	/// if the tooltip is crossing any of the screen's edges, and sets the adjustedNewTTPos variable accordingly.
	/// It will always try to at least keep the top and left sides
	/// of the tooltip visible.
	/// </summary>
	/// <param name="considerLossyScale">If true, makes the "dimensions" variables of the tooltip's rectTransform consider its lossyScale</param>
	public void BorderAdjustments(bool considerLossyScale = false) {
		// check and solve problems for the tooltip that goes out of the screen on the horizontal axis
		//check for right edge of screen
		borderTest = (adjustedNewTTPos.x + rightWidth);
		if (borderTest > screenUpperRight.x) {
			//pivot check... if appropriate and allowed,
			//flip and recalculate dimensions before applying the adjustment
			if (pivotFlipping && originalPivot.x < 0.5f) {
				adjustedPivot.x = 1 - originalPivot.x;
				CalcTooltipHorizontalDimensions(considerLossyScale);
				borderTest = (adjustedNewTTPos.x + rightWidth);
				if (borderTest > screenUpperRight.x) {
					shifterForBorders.x = borderTest - screenUpperRight.x;
				}
				else {
					shifterForBorders.x = 0;
				}
			}
			else {
				shifterForBorders.x = borderTest - screenUpperRight.x;
			}

			adjustedNewTTPos.x -= shifterForBorders.x;
		}
		//check for left edge of screen
		borderTest = (adjustedNewTTPos.x - leftWidth);
		if (borderTest < screenLowerLeft.x) {
			if (pivotFlipping && originalPivot.x > 0.5f) {
				adjustedPivot.x = 1 - originalPivot.x;
				CalcTooltipHorizontalDimensions(considerLossyScale);
				borderTest = (adjustedNewTTPos.x - leftWidth);
				if (borderTest < screenLowerLeft.x) {
					shifterForBorders.x = screenLowerLeft.x - borderTest;
				}
				else {
					shifterForBorders.x = 0;
				}
			}
			else {
				shifterForBorders.x = screenLowerLeft.x - borderTest;
			}

			adjustedNewTTPos.x += shifterForBorders.x;

		}

		// check and solve problems for the tooltip that goes out of the screen on the vertical axis

		//check for lower edge of the screen
		borderTest = (adjustedNewTTPos.y - bottomHeight);
		if (borderTest < screenLowerLeft.y) {
			if (pivotFlipping && originalPivot.y > 0.5f) {
				adjustedPivot.y = 1 - originalPivot.y;
				CalcTooltipVerticalDimensions(considerLossyScale);
				borderTest = (adjustedNewTTPos.y - bottomHeight);
				if (borderTest < screenLowerLeft.y) {
					shifterForBorders.y = screenLowerLeft.y - borderTest;
				}
				else {
					shifterForBorders.y = 0;
				}
			}
			else {
				shifterForBorders.y = screenLowerLeft.y - borderTest;
			}

			adjustedNewTTPos.y += shifterForBorders.y;
		}

		//check for upper edge of the screen
		borderTest = (adjustedNewTTPos.y + topHeight);
		if (borderTest > screenUpperRight.y) {
			if (pivotFlipping && originalPivot.y < 0.5f) {
				adjustedPivot.y = 1 - originalPivot.y;
				CalcTooltipVerticalDimensions(considerLossyScale);
				borderTest = (adjustedNewTTPos.y + topHeight);
				if (borderTest > screenUpperRight.y) {
					shifterForBorders.y = borderTest - screenUpperRight.y;
				}
				else {
					shifterForBorders.y = 0;
				}
			}
			else {
				shifterForBorders.y = borderTest - screenUpperRight.y;
			}

			adjustedNewTTPos.y -= shifterForBorders.y;
		}
	}
}

using System.Collections;
///Credit Martin Nerurkar // www.martin.nerurkar.de // www.sharkbombs.com
///Sourced from - http://www.sharkbombs.com/2015/02/10/tooltips-with-the-new-unity-ui-ugui/
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
    [AddComponentMenu("UI/Extensions/Tooltip/Tooltip Trigger")]
	public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
	{
		[TextAreaAttribute]
		public string text;

		[Tooltip("Prevents the use of WorldToScreenPoint to get the desired tooltip position, which causes issues in overlay canvases")]
		public bool isInOverlayCanvas = false;

		public enum TooltipPositioningType {
			mousePosition,
			mousePositionAndFollow,
			transformPosition
		}

		[Tooltip("Defines where the tooltip will be placed and how that placement will occur. Transform position will always be used if this element wasn't selected via pointer")]
		public TooltipPositioningType tooltipPositioningType = TooltipPositioningType.mousePosition;

		private bool hovered = false;

		public Vector3 offset;


		void Reset() {
			//attempt to check if our canvas is overlay or not and check our "is overlay" accordingly
			Canvas ourCanvas = GetComponentInParent<Canvas>();
			if (ourCanvas && ourCanvas.renderMode == RenderMode.ScreenSpaceOverlay) {
				isInOverlayCanvas = true;
			}
		}


		public void OnPointerEnter(PointerEventData eventData)
		{
			switch (tooltipPositioningType) {
				case TooltipPositioningType.mousePosition:
					StartHover(Input.mousePosition + offset, true);
					break;
				case TooltipPositioningType.mousePositionAndFollow:
					StartHover(Input.mousePosition + offset, true);
					hovered = true;
					StartCoroutine(HoveredMouseFollowingLoop());
					break;
				case TooltipPositioningType.transformPosition:
					StartHover((isInOverlayCanvas ? transform.position :
						ToolTip.Instance.guiCamera.WorldToScreenPoint(transform.position)) + offset, true);
					break;
			}
		}

		IEnumerator HoveredMouseFollowingLoop() {
			while (hovered) {
				StartHover(Input.mousePosition + offset);
				yield return null;
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			StartHover((isInOverlayCanvas ? transform.position :
						ToolTip.Instance.guiCamera.WorldToScreenPoint(transform.position)) + offset, true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			StopHover();
		}

		public void OnDeselect(BaseEventData eventData)
		{
			StopHover();
		}

		void StartHover(Vector3 position, bool shouldCanvasUpdate = false)
		{
			ToolTip.Instance.SetTooltip(text, position, shouldCanvasUpdate);
		}

		void StopHover()
		{
			hovered = false;
			ToolTip.Instance.HideTooltip();
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// parent of panels that should have some sort of save functionality which can depend on whether there were changes on its content or not
/// </summary>
public class DirtableOverlayPanel : GenericOverlayPanel {

	/// <summary>
	/// this should be marked as true whenever a change is made in one of the input options,
	/// so that we can only show a "save changes?" prompt if there really was a change
	/// </summary>
	public bool isDirty;

	/// <summary>
	/// isdirty -> true
	/// </summary>
	public virtual void MarkDirty() {
		isDirty = true;
	}

	/// <summary>
	/// isdirty -> true
	/// </summary>
	public virtual void MarkDirty(string unused) {
		isDirty = true;
	}
}

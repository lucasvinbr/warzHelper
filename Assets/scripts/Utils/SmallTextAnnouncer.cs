using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// it's an announcer, but not so big hahaha... and with a singleton ref
/// </summary>
public class SmallTextAnnouncer : BigTextAnnouncer {

	public static SmallTextAnnouncer instance;

	private void Awake() {
		instance = this;
	}

}

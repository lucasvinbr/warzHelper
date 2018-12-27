using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldFXManager : MonoBehaviour {

	public ParticleSystem bolsterParticle, recruitParticle;


	private ParticleSystem.MainModule sysMain;

	public static WorldFXManager instance;

	private void Awake() {
		instance = this;
	}

	public void EmitParticle(ParticleSystem particleSys, Vector3 emissionPos, Color particleColor) {
		particleSys.transform.position = emissionPos;
		sysMain = particleSys.main;
		sysMain.startColor = particleColor;
		particleSys.Emit(25);
	}

}

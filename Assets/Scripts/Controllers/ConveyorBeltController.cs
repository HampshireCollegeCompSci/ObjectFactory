﻿/*
 * Author(s): Isaiah Mann
 * Description: This class should be attached to the parent object of a set of conveyor belts
 */

using UnityEngine;
using System.Collections;

public class ConveyorBeltController : Controller {
	ConveyorBelt[] belts;

	public bool ObjectsInMotion () {
		bool hasMovingObject = false;
		foreach (WorldObject worldObject in GetComponentsInChildren<WorldObject>()) {
			if (worldObject is WorldSocket && !(worldObject is DropZone)) {
				hasMovingObject |= ((WorldSocket) worldObject).HasObjects();
			}
		}
		return hasMovingObject;
	}

	public void SetFactoryController (FactoryController controller) {
		foreach (WorldObject worldObject in GetComponentsInChildren<WorldObject>()) {
			worldObject.SetFactoryController(controller);
		}
	}

	public void SetBeltSpeed(float beltSpeed) {
		foreach (ConveyorBelt belt in belts) {
			belt.SetBeltSpeed(beltSpeed);
		}
	}

	public void AddToBelt(FactoryObject factoryObject) {
		belts[0].AddObjectToBelt(factoryObject);
	}

	protected override void Init () {
		base.Init ();
		belts = GetComponentsInChildren<ConveyorBelt>();
	}

	public bool BeltsMoving () {
		float cumulativeBeltSpeed = 0;
		foreach (ConveyorBelt belt in belts) {
			cumulativeBeltSpeed += belt.BeltSpeed;
		}
		return cumulativeBeltSpeed > 0;
	}
}

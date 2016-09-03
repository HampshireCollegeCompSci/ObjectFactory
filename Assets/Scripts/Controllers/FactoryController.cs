﻿/*
 * Author(s): Isaiah Mann
 * Description: Controls a factory
 */

using UnityEngine;
using System.Collections;

public class FactoryController : Controller {
	const float SPAWN_OFFSET = 10f;

	public bool ShouldStartActive;
	public GameObject FactoryObjectPrefab;

	static FactoryController Instance;

	public delegate void FactoryAction();
	FactoryAction onRun;
	public ConveyorBeltController[] ConveyorBelts;
	Quota[] quotas;

	void Awake () {
		Instance = this;
	}

	void Start () {
		if (!ShouldStartActive) {
			SetConveyorBeltSpeeds(0);		
		}
	}

	void OnDestroy () {
		onRun = null;
	}

	public static void SubscribeRunFactoryAction (FactoryAction onRunAction) { 
		if (Instance) {
			Instance.onRun += onRunAction;
		}
	}

	public static void SetConveyorBeltSpeeds (float beltSpeed) {
		if (Instance) {
			foreach (ConveyorBeltController beltController in Instance.ConveyorBelts) {
				beltController.SetBeltSpeed(beltSpeed);
			}
		}
	}

	public static void InitInstancesWithQuotas (Quota[] quotas) {
		if (Instance) {
			Instance.InitWithQuotas(quotas);
		}
	}

	public void InitWithQuotas (Quota[] quotas) {
		this.quotas = quotas;
	}

	public bool HasQuotas () {
		return this.quotas != null;
	}

	public bool AreQuotasSatisfied (object[] arguments) {
		bool areQuotasSatisfied = true;
		foreach (Quota quota in quotas) {
			if (!quota.CheckSatisfied(arguments)) {
				areQuotasSatisfied = false;
			}
		}
		return areQuotasSatisfied;
	}

	public bool BeltsMoving () {
		bool anyBeltMoving = false;
		foreach (ConveyorBeltController beltController in ConveyorBelts) {
			anyBeltMoving |= beltController.BeltsMoving();
		}
		return anyBeltMoving;
	}
		
	public bool CheckQuotasForDropZone (DropZone dropZone) {
		if (quotas == null) {
			return false;
		}

		System.Collections.Generic.Dictionary<FactoryObjectDescriptorV1, int> report = dropZone.GetFactoryObjectDescriptorV1Report();
		bool areAllQuotasSatisfied = true;
		bool objectsInMotion = ObjectsInMotion();
		foreach (Quota quota in quotas) {
			bool isCurrentQuotaSatisfied = false;
			if (quota is SimpleQuota) { // Currently only supports simple quota
				foreach (FactoryObjectDescriptorV1 descriptor in report.Keys) {
					isCurrentQuotaSatisfied |= quota.CheckSatisfied(descriptor, report[descriptor]);
				}
			}
			areAllQuotasSatisfied &= isCurrentQuotaSatisfied;
			if (!isCurrentQuotaSatisfied && !objectsInMotion && quota is SimpleQuota) {
				foreach (FactoryObjectDescriptorV1 descriptor in report.Keys) {
					MessageController.SendMessageToInstance (
						MessageUtil.GetSimpleQuotaMismatchMessage(quota as SimpleQuota, new SimpleQuota(descriptor, report[descriptor])));
				}
			}
		}
		return areAllQuotasSatisfied;
	}

	public bool ObjectsInMotion () {
		bool isObjectInMotion = false;
		foreach (ConveyorBeltController beltController in ConveyorBelts) {
			isObjectInMotion |= beltController.ObjectsInMotion();
		}
		return isObjectInMotion;
	}

	public void RunFactory () {
		setFactoryController();
		ClearDropZones();
		callOnRun();
		if (!BeltsMoving()) {
			MessageController.SendMessageToInstance(MessageUtil.ZeroBeltSpeedMessage);
		}
	}

	public void ClearDropZones () {
		foreach (ConveyorBeltController controller in ConveyorBelts) {
			controller.ClearDropZones();
		}
	}
		
	public static void AddToBeltByDescriptor (int beltIndex, FactoryObjectDescriptor[] descriptors, float delayBeforeEachSpawn) {
		if (Instance) {	
			Instance.StartAddBeltByDescriptor(beltIndex, descriptors, delayBeforeEachSpawn);
		}
	}

	void StartAddBeltByDescriptor (int beltIndex, FactoryObjectDescriptor[] descriptors, float delayBeforeEachSpawn) {
		StartCoroutine(RunAddBeltByDescriptor(beltIndex, descriptors, delayBeforeEachSpawn));
	}

	IEnumerator RunAddBeltByDescriptor (int beltIndex, FactoryObjectDescriptor[] descriptors, float delayBeforeEachSpawn) {
		if (IntUtil.InRange(beltIndex, descriptors.Length)) {
			yield return new WaitForSeconds(delayBeforeEachSpawn);
			for (int i = 0; i < descriptors.Length; i++) {
				GameObject factoryObject = (GameObject) Instantiate(FactoryObjectPrefab, Vector3.up * SPAWN_OFFSET, Quaternion.identity);
				FactoryObject factoryObjectController = factoryObject.GetComponent<FactoryObject>();
				factoryObjectController.Descriptor = descriptors[i];
				ConveyorBelts[beltIndex].AddToBelt(factoryObjectController);
				yield return new WaitForSeconds(delayBeforeEachSpawn);
			}
		}
	}

	void callOnRun () {
		if (onRun != null) {
			onRun();
		}
	}

	void setFactoryController () {
		foreach (ConveyorBeltController controller in ConveyorBelts) {
			controller.SetFactoryController(this);
		}
	}
}

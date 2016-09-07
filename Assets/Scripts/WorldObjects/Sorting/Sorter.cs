﻿/*
 * Author(s): Isaiah Mann
 * Description: Sorts factory objects into multiple ouputs
 */

using UnityEngine;
using System.Collections;

public class Sorter : FactorySocket {
	const int INVALID_INDEX = -1;

	public delegate int DetermineIndexAction(WorldObject objectToSort, WorldSocket[] possibleOuputs);
	DetermineIndexAction determineIndex;
	DetermineIndexAction peekIndex;
	DetermineIndexAction tickIndex;

	public WorldSocket[] PossibleOutputs;

	public void SubscribeDetermineIndex (DetermineIndexAction determineIndexAction, DetermineIndexAction peekIndexAction, DetermineIndexAction tickIndexAction) {
		determineIndex = determineIndexAction;
		peekIndex = peekIndexAction;
		tickIndex = tickIndexAction;
	}

	public override WorldObject SendOuput () {
		callTickIndex(PeekOuput());
		return base.SendOuput ();
	}
	public WorldSocket ChooseOuput (WorldObject objectToSend) {
		int index = callDetermineIndex(objectToSend);
		if (index == INVALID_INDEX || !IntUtil.InRange(index, 0, PossibleOutputs.Length)) {
			return null;
		} else {
			return PossibleOutputs[index];
		}
	}

	public override bool OuputReceiverAvailable () {
		return PossibleOutputs.Length > 0;
	}

	public override bool OutputAvailable (WorldSocket availableFor) {
		int nextIndex = callPeekIndex(PeekOuput());
		if (IntUtil.InRange(nextIndex, PossibleOutputs.Length)) {
			return availableFor == PossibleOutputs[nextIndex] && base.OutputAvailable(availableFor);;
		} else {
			return false;
		}
	}

	protected override void sendOuputToReceiver () {
		if (OuputReceiverAvailable()) {
			WorldObject objectToSend = SendOuput();
			ChooseOuput(objectToSend).ReceiveInput(objectToSend);
		}
	}

	int callDetermineIndex (WorldObject objectToSort) {
		if (determineIndex != null) {
			return determineIndex(objectToSort, PossibleOutputs);
		} else {
			return INVALID_INDEX;
		}
	}

	int callPeekIndex (WorldObject objectToSort) {
		if (peekIndex != null) {
			return peekIndex(objectToSort, PossibleOutputs);
		} else {
			return INVALID_INDEX;
		}
	}

	int callTickIndex (WorldObject objectToSort) {
		if (tickIndex != null) {
			return tickIndex(objectToSort, PossibleOutputs);
		} else {
			return INVALID_INDEX;
		}
	}
}
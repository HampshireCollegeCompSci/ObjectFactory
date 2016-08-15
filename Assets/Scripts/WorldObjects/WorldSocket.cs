﻿/*
 * Author(s): Isaiah Mann
 * Description: 
 */

using UnityEngine;
using System.Collections;

public class WorldSocket : WorldObject {
	public WorldSocket InputSender;
	public WorldSocket OuputReceiver;

	public virtual bool InputSenderAvailable () {
		return InputSender != null && InputSender.SupportsOuput();
	}

	public virtual bool OuputReceiverAvailable () {
		return OuputReceiver != null && OuputReceiver.SupportsInput();
	}

	public virtual bool InputAvailable () {
		return false;
	}

	public virtual bool OutputAvailable () {
		return false;
	}

	public virtual bool SupportsInput () {
		return false;
	}

	public virtual bool SupportsOuput () {
		return false;
	}

	public virtual void ReceiveInput (WorldObject worldObject) {
		// Do nothing
	}

	// Fetches input that its received
	protected virtual WorldObject getInput () {
		return null;
	}

	protected virtual void processInput (WorldObject worldObject)  {
		// Do nothing
	}

	public virtual WorldObject SendOuput () {
		return null;	
	}

	protected virtual void receiveInputFromSender () {
		if (InputSenderAvailable()) {
			ReceiveInput(InputSender.SendOuput());
		}
	}

	protected virtual void sendOuputToReceiver () {
		if (OuputReceiverAvailable()) {
			OuputReceiver.ReceiveInput(SendOuput());
		}
	}
}

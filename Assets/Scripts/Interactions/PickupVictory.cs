using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "NewPickupVictory", menuName = "pickupVictory", order = 1)]
public class PickupVictory : VictoryCondition {
	public GameObject goalObject;

	public override void initConditions() {
		this.goalObject = GameObject.Find(goalObject.name);
		Assert.IsNotNull(this.goalObject, "Unable to locate the GoalObject");
	}

    public override bool isValidated() {
		return this.goalObject == null; // Goal has been picked up (destroyed)
    }
}

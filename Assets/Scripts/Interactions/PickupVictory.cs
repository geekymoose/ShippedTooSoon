using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "NewPickupVictory", menuName = "pickupVictory", order = 1)]
public class PickupVictory : VictoryCondition {
	public GameObject[] listGoals;

	public override void initConditions() {
		for(int k = 0; k < this.listGoals.Length; ++k) {
			this.listGoals[k] = GameObject.Find(this.listGoals[k].name);
			Assert.IsNotNull(this.listGoals[k], "Unable to locate the GoalObject");
		}
	}

	public override bool isValidated() {
		if(this.listGoals == null || this.listGoals.Length == 0) {
			return true;
		}
		int remaining = 0;
		foreach(GameObject o in this.listGoals) {
			if(o != null) {
				remaining++;
			}
		}
		return remaining == 0;
	}
}
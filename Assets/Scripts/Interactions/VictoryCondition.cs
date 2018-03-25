using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VictoryCondition : ScriptableObject {
	public abstract void initConditions();
	public abstract bool isValidated();
}
using UnityEngine;
using UnityEngine.InputSystem;

public class Action : ScriptableObject
{
	public InputActionReference action;
	public float duration = 0.5f;
	public int tickCost = 1;
}

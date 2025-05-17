using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputsManager : MonoBehaviour
{
	public InputActionAsset playerInputActions;

	InputAction _actionMove;
	InputAction _actionAxe;

	private void Awake()
	{
		_actionMove = playerInputActions.FindAction("ActionMove");
		_actionAxe = playerInputActions.FindAction("ActionAxe");
	}

	//private void Update()
	//{
	//	if (_actionMove.IsPressed())
	//	{
	//		Debug.Log("A");
	//	}

	//	if (_actionMove.WasPressedThisFrame())
	//	{
	//		Debug.Log("B");
	//	}

	//	if (_actionMove.WasReleasedThisFrame())
	//	{
	//		Debug.Log("C");
	//	}

	//	if (_actionMove.WasPerformedThisFrame())
	//	{
	//		Debug.Log("D");
	//	}

	//	if (_actionMove.WasCompletedThisFrame())
	//	{
	//		Debug.Log("E");
	//	}

	//}
}

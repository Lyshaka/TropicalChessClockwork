using System;
using UnityEngine;

public class PathFindingUnit : MonoBehaviour
{
	public Vector2Int gridPosition;
	public Vector2Int targetPosition;

	public Vector2Int[] path;

	protected void RequestNewPath(Action<Vector2Int[] , bool> callback)
	{
		//Debug.Log($"Requested path between : {gridPosition} and {targetPosition}");
		PathRequestManager.RequestPath(gridPosition, targetPosition, callback);
	}

	//protected virtual void OnPathFound(Vector2Int[] newPath, bool success)
	//{

	//	path = success ? newPath : null;

	//	//if (success)
	//	//{
	//	//	//Debug.Log($"Found path between : {gridPosition} and {targetPosition} !");
	//	//	path = newPath;
	//	//}
	//	//else
	//	//{
	//	//	path = null;
	//	//	//Debug.LogWarning($"Couldn't find path between : {gridPosition} and {targetPosition} :/");
	//	//}
	//}
}

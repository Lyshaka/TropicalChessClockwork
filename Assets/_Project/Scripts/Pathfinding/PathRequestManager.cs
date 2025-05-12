using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{
	static PathRequestManager instance;
	
	Queue<PathResult> _results = new Queue<PathResult>();

	PathFinding _pathfinding;
	bool _isProcessingPath;

	private void Awake()
	{
		instance = this;
		_pathfinding = GetComponent<PathFinding>();
	}

	private void Update()
	{
		if (_results.Count > 0)
		{
			int itemsInQueue = _results.Count;
			lock (_results)
			{
				for (int i = 0; i < itemsInQueue; i++)
				{
					PathResult result = _results.Dequeue();
					result.callback(result.path, result.success);
				}
			}
		}
	}

	public static void RequestPath(Vector2Int pathStart, Vector2Int pathTarget, Action<Vector2Int[], bool> callback)
	{
		RequestPath(new(pathStart, pathTarget, callback));
	}

	public static void RequestPath(PathRequest request)
	{
		ThreadStart threadStart = delegate
		{
			instance._pathfinding.FindPath(request, instance.FinishedProcessingPath);
		};

		Thread thread = new(threadStart);
		thread.Start();
		//threadStart.Invoke();
	}

	public void FinishedProcessingPath(PathResult result)
	{
		//new PathResult(path, success, originalRequest.callback);
		lock (_results)
		{
			_results.Enqueue(result);
		}
	}
}

public struct PathRequest
{
	public Vector2Int pathStart;
	public Vector2Int pathTarget;
	public Action<Vector2Int[], bool> callback;

	public PathRequest(Vector2Int pathStart, Vector2Int pathTarget, Action<Vector2Int[], bool> callback)
	{
		this.pathStart = pathStart;
		this.pathTarget = pathTarget;
		this.callback = callback;
	}
}

public struct PathResult
{
	public Vector2Int[] path;
	public bool success;
	public Action<Vector2Int[], bool> callback;

	public PathResult(Vector2Int[] path, bool success, Action<Vector2Int[], bool> callback)
	{
		this.path = path;
		this.success = success;
		this.callback = callback;
	}
}
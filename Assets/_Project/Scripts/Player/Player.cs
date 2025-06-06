using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public class Player : PathFindingUnit
{
	public static Player Instance { get; private set; }

	[Header("Actions")]
	[SerializeField] Action[] actions;
	[SerializeField] InputActionReference validateAction;

	[Header("Technical")]
	[SerializeField] GameObject pathPrefab;
	[SerializeField] GameObject cellFeedbackVisualizerPrefab;
	[SerializeField] TextMeshProUGUI coordTMP;
	[SerializeField] LayerMask mouseRaycastLayer = (1 << 3);
	[SerializeField] Transform mesh;

	// Actions
	Action _selectedAction;
	Action _performingAction;
	bool _isPerformingAction = false;
	bool _actionStart;
	float _actionElapsedTime = 0f;
	Vector2 _actionDirection = Vector2.zero;
	CardinalDirection _actionCardinal = CardinalDirection.None;

	// Cell Feedback Visualizer
	List<GameObject> _cellFeedbackVisualizersActive = new();
	List<GameObject> _cellFeedbackVisualizersInactive = new();

	LineRenderer _pathLineRenderer;

	Vector2Int[] _currentPath;
	int _currentPathIndex;
	float _currentMovementElapsedTime = 0f;
	float movementDuration = 0.2f;
	float movementCooldown = 0.1f;

	Vector2 _mousePos = Vector2.zero;
	Vector2 _playerPos = Vector2.zero;
	Vector2 _pointedDirection = Vector2.zero;
	Vector2 _clampedDirection = Vector2.zero;
	Vector2Int roundedMousePos = Vector2Int.zero;

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	void Start()
	{
		_pathLineRenderer = GetComponentInChildren<LineRenderer>();
		gridPosition = new(31, 31);
		transform.position = new Vector3(gridPosition.x, 0, gridPosition.y);
		_currentPath = null;
		InitializePositions();
	}

	void Update()
	{

		//if (Input.GetKey(KeyCode.D) && gridPosition.x < GameGrid.GRID_SIZE - 1) // Left
		//	gridPosition.x++;
		//if (Input.GetKey(KeyCode.A) && gridPosition.x > 0) // Right
		//	gridPosition.x--;
		//if (Input.GetKey(KeyCode.W) && gridPosition.y < GameGrid.GRID_SIZE - 1) // Up
		//	gridPosition.y++;
		//if (Input.GetKey(KeyCode.S) && gridPosition.y > 0) // Down
		//	gridPosition.y--;

		//if (!GameGrid.instance.gridCell[x, y].visited)
		//{
		//	GameGrid.instance.gridCell[x, y].visited = true;
		//	Instantiate(pathPrefab, new Vector3(x, 0, y), Quaternion.identity, GameGrid.instance.transform);
		//}

		// Get player position on the grid
		_playerPos = new(transform.position.x, transform.position.z);

		// Get mouse position on the grid
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, 1000f, mouseRaycastLayer))
		{
			_mousePos = new(hitInfo.point.x, hitInfo.point.z);

			roundedMousePos.x = Mathf.FloorToInt(_mousePos.x + 0.5f);
			roundedMousePos.y = Mathf.FloorToInt(_mousePos.y + 0.5f);

			coordTMP.text =
				$"x : {_mousePos.x}\ny : {_mousePos.y}\n" +
				$"x : {roundedMousePos.x}\ny : {roundedMousePos.y}";
		}

		// Get direction between player position and mouse position
		//float angle = Vector2.SignedAngle(_playerPos, _mousePos);
		_pointedDirection = _mousePos - _playerPos;
		_clampedDirection = ClampDirection(_pointedDirection);

		targetPosition = roundedMousePos;
		if (GameGrid.instance.gridCell[targetPosition.x, targetPosition.y].walkable)
			RequestNewPath(OnPathFound);

		if (path != null && path.Length > 0)
		{
			//Debug.Log("Path length: " + path.Length);

			Vector3[] positions = new Vector3[path.Length + 1];
			_pathLineRenderer.positionCount = path.Length + 1;
			positions[0] = mesh.position;

			for (int i = 1; i < path.Length + 1; i++)
			{
				positions[i] = new(path[i - 1].x, 0.2f, path[i - 1].y);
			}

			_pathLineRenderer.SetPositions(positions);
		}
		else
		{
			_pathLineRenderer.positionCount = 0;
		}

		//if (Input.GetKeyDown(KeyCode.Mouse0))
		//{
		//	_currentPathIndex = 0;
		//	_currentPath = path;
		//}

		HandleActionInputs();

		HandleAction();

		FollowPath();

		//Debug.Log("Selected Action : " + _selectedAction);
		//Debug.Log("Performing Action : " + _performingAction);
	}

	// Initialization
	void InitializePositions()
	{
		for (int i = 0; i < actions.Length; i++)
		{
			if (actions[i] is MeleeAttack meleeAttack)
				meleeAttack.directions.InitializePositions();
		}
	}

	#region PATHFINDING

	// Pathfinding
	protected void OnPathFound(Vector2Int[] newPath, bool success)
	{
		path = success ? newPath : null;
	}

	void FollowPath()
	{
		if (_currentPath == null)
			return;

		if (_currentPathIndex < _currentPath.Length)
		{
			if (_currentMovementElapsedTime < movementDuration)
			{
				mesh.position = Vector3.Lerp(
					new(gridPosition.x, 0.2f, gridPosition.y),
					new(_currentPath[_currentPathIndex].x, 0.2f, _currentPath[_currentPathIndex].y),
					_currentMovementElapsedTime / movementDuration);

				_currentMovementElapsedTime += Time.deltaTime;
			}
			else if (_currentMovementElapsedTime < (movementDuration + movementCooldown))
			{
				_currentMovementElapsedTime += Time.deltaTime;
			}
			else
			{
				gridPosition = _currentPath[_currentPathIndex];
				transform.position = new Vector3(gridPosition.x, 0, gridPosition.y);
				mesh.position = new Vector3(gridPosition.x, 0.2f, gridPosition.y);

				_currentPathIndex++;
				_currentMovementElapsedTime = 0f;
			}
		}
		else
		{
			_currentPath = null;
		}
	}

	#endregion

	#region ACTIONS
	// Actions
	CardinalDirection GetCardinalDirection(Vector2 direction)
	{
		// Exclude vector zero
		if (direction.sqrMagnitude <= 0.01f)
			return CardinalDirection.None;

		// Get angle in radians
		float angle = Mathf.Atan2(direction.y, direction.x);

		// Convert angle from radians to degrees, normalize to 0-360
		float angleDeg = Mathf.Rad2Deg * angle;
		if (angleDeg < 0)
			angleDeg += 360f;

		// Snap to nearest 45� (360� / 8 directions)
		int index = Mathf.RoundToInt(angleDeg / 45f) % 8;

		return index switch
		{
			0 => CardinalDirection.East,		// 0 -> 0�    East
			1 => CardinalDirection.NorthEast,	// 1 -> 45�   NorthEast
			2 => CardinalDirection.North,		// 2 -> 90�   North
			3 => CardinalDirection.NorthWest,	// 3 -> 135�  NorthWest
			4 => CardinalDirection.West,		// 4 -> 180�  West
			5 => CardinalDirection.SouthWest,	// 5 -> 225�  SouthWest
			6 => CardinalDirection.South,		// 6 -> 270�  South
			7 => CardinalDirection.SouthEast,	// 7 -> 315�  SouthEast
			_ => CardinalDirection.None,
		};
	}

	Vector2 ClampDirection(Vector2 direction)
	{
		// Exclude vector zero
		if (direction.sqrMagnitude <= 0.01f)
			return Vector2.zero;

		// Get angle in radians
		float angle = Mathf.Atan2(direction.y, direction.x);

		// Convert angle from radians to degrees, normalize to 0-360
		float angleDeg = Mathf.Rad2Deg * angle;
		if (angleDeg < 0)
			angleDeg += 360f;

		// Snap to nearest 45� (360� / 8 directions)
		int index = Mathf.RoundToInt(angleDeg / 45f) % 8;

		// Look-up table for the 8 directions
		Vector2[] directions8 = new Vector2[]
		{
			Vector2.right,                          // 0 -> 0�    East
			new Vector2(1, 1).normalized,           // 1 -> 45�   NorthEast
			Vector2.up,                             // 2 -> 90�   North
			new Vector2(-1, 1).normalized,          // 3 -> 135�  NorthWest
			Vector2.left,                           // 4 -> 180�  West
			new Vector2(-1, -1).normalized,         // 5 -> 225�  SouthWest
			Vector2.down,                           // 6 -> 270�  South
			new Vector2(1, -1).normalized           // 7 -> 315�  SouthEast
		};

		return directions8[index];
	}

	void HandleActionInputs()
	{
		// Exit if an action is already being performed
		if (_isPerformingAction)
			return;

		// Check every action to see if its corresponding input is pressed
		for (int i = 0; i < actions.Length; i++)
		{
			if (actions[i].action.action.triggered)
			{
				if (_selectedAction == actions[i])
					_selectedAction = null;
				else
					_selectedAction = actions[i];
			}
		}

		// If an action has been selected
		if (_selectedAction != null)
		{
			// Deactivate all the cell feedback visualizers
			foreach (GameObject obj in _cellFeedbackVisualizersActive)
			{
				_cellFeedbackVisualizersInactive.Add(obj);
				obj.SetActive(false);
			}
			_cellFeedbackVisualizersActive.Clear();

			// If an action has been selected, and the validate input(LMB) is pressed, perform the action
			if (validateAction.action.WasPressedThisFrame())
			{
				_isPerformingAction = true;
				_performingAction = _selectedAction;
				_selectedAction = null;
				_actionDirection = _clampedDirection;
				_actionCardinal = GetCardinalDirection(_actionDirection);
			}
			else // Otherwise just show the cell feedback visualizer, if applicable
			{
				if (_selectedAction is MeleeAttack attack)
				{
					Vector2Int[] gridPositions = attack.directions.GetAreaFromDirection(GetCardinalDirection(_clampedDirection)).GetPositions();

					for (int i = 0; i < gridPositions.Length; i++)
					{
						// Instantiate the visualizers if none are available
						if (_cellFeedbackVisualizersInactive.Count <= 0)
						{
							GameObject obj = Instantiate(cellFeedbackVisualizerPrefab, transform);
							_cellFeedbackVisualizersInactive.Add(obj);
						}

						Vector3 pos = new Vector3(gridPositions[i].x - 2, 0f, gridPositions[i].y - 2) + transform.position;

						_cellFeedbackVisualizersInactive[0].SetActive(true);
						_cellFeedbackVisualizersInactive[0].transform.position = pos;
						_cellFeedbackVisualizersActive.Add(_cellFeedbackVisualizersInactive[0]);
						_cellFeedbackVisualizersInactive.RemoveAt(0);
					}
				}
			}
		}
	}

	void HandleAction()
	{
		if (_isPerformingAction)
		{
			if (_actionElapsedTime > _performingAction.duration)
			{
				//foreach (GameObject obj in _cellFeedbackVisualizersActive)
				//{
				//	_cellFeedbackVisualizersInactive.Add(obj);
				//	obj.SetActive(false);
				//}
				//_cellFeedbackVisualizersActive.Clear();

				_isPerformingAction = false;
				_performingAction = null;
			}
			else
			{
				// Action Start (executed only once)
				if (_actionStart)
				{
					//if (_performingAction is MeleeAttack attack)
					//{
					//	Vector2Int[] gridPositions = attack.directions.GetAreaFromDirection(_actionCardinal).GetPositions();

					//	for (int i = 0; i < gridPositions.Length; i++)
					//	{
					//		// Instantiate the visualizers

					//		if (_cellFeedbackVisualizersInactive.Count <= 0)
					//		{
					//			GameObject obj = Instantiate(cellFeedbackVisualizerPrefab, transform);
					//			_cellFeedbackVisualizersInactive.Add(obj);
					//		}

					//		Vector3 pos = new Vector3(gridPositions[i].x - 2, 0f, gridPositions[i].y - 2) + transform.position;

					//		_cellFeedbackVisualizersInactive[0].SetActive(true);
					//		_cellFeedbackVisualizersInactive[0].transform.position = pos;
					//		_cellFeedbackVisualizersActive.Add(_cellFeedbackVisualizersInactive[0]);
					//		_cellFeedbackVisualizersInactive.RemoveAt(0);
					//	}

					//}


					_actionStart = false;
				}


				// Visualisation



				// Action here

				// If action is type of melee attack, apply its corresponding damage inside the corresponding area
				if (_performingAction is MeleeAttack meleeAttack)
				{
					//Vector2Int[] gridPositions = meleeAttack.directions.GetAreaFromDirection(GetCardinalDirection(_clampedDirection)).GetPositions();

					//for (int i = 0; i < _cellFeedbackVisualizersActive.Count; i++)
					//{
					//	Vector3 pos = new Vector3(gridPositions[i].x - 2, 0f, gridPositions[i].y - 2) + transform.position;
					//	_cellFeedbackVisualizersActive[i].transform.position = pos;
					//}
				}

				// 
				if (_performingAction is Move move)
				{

				}



				_actionElapsedTime += Time.deltaTime;
			}
		}
		else
		{
			_actionElapsedTime = 0f;
			_actionStart = true;
		}
	}

	#endregion

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(new(_playerPos.x, 0.2f, _playerPos.y), new Vector3(_playerPos.x, 0.2f, _playerPos.y) + new Vector3(_pointedDirection.x, 0f, _pointedDirection.y));
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(new(_playerPos.x, 0.2f, _playerPos.y), new Vector3(_playerPos.x, 0.2f, _playerPos.y) + new Vector3(_clampedDirection.x, 0f, _clampedDirection.y));

		if (Application.isPlaying && _performingAction != null && _performingAction is MeleeAttack meleeAttack)
		{
			Gizmos.color = Color.green;
			AttackArea attackArea = meleeAttack.directions.GetAreaFromDirection(_actionCardinal);

			if (attackArea == null)
				return;

			Vector2Int[] pos = attackArea.GetPositions();

			if (pos == null)
				return;

			for (int i = 0; i < pos.Length; i++)
			{
				Vector3 p = new(_playerPos.x + pos[i].x - 2, 0.2f, _playerPos.y + pos[i].y - 2);
				Gizmos.DrawSphere(p, 0.1f);
			}
		}
	}
}

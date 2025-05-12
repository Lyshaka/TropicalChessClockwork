using UnityEngine;
using TMPro;

public class Player : PathFindingUnit
{
	public static Player Instance { get; private set; }

	[Header("Walk")]
	[SerializeField] Action actionWalk;

	[Header("Heal")]
	[SerializeField] Action actionHeal;

	[Header("Shield")]
	[SerializeField] Action actionShield;

	[Header("Arrow")]
	[SerializeField] Action actionArrow;

	[Header("Spell")]
	[SerializeField] Action actionSpell;

	[Header("Axe")]
	[SerializeField] Action actionAxe;

	[Header("Sword")]
	[SerializeField] Action actionSword;

	[Header("Hammer")]
	[SerializeField] Action actionHammer;


	[Header("Technical")]
	[SerializeField] GameObject pathPrefab;
	[SerializeField] TextMeshProUGUI coordTMP;
	[SerializeField] LayerMask mouseRaycastLayer = (1 << 3);
	[SerializeField] Transform mesh;

	Action _selectedAction;
	Action _performingAction;


	LineRenderer _pathLineRenderer;

	Vector2Int[] _currentPath;
	int _currentPathIndex;
	float _currentMovementElapsedTime = 0f;
	float movementDuration = 0.2f;
	float movementCooldown = 0.1f;

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
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.D) && gridPosition.x < GameGrid.GRID_SIZE - 1) // Left
			gridPosition.x++;
		if (Input.GetKey(KeyCode.A) && gridPosition.x > 0) // Right
			gridPosition.x--;
		if (Input.GetKey(KeyCode.W) && gridPosition.y < GameGrid.GRID_SIZE - 1) // Up
			gridPosition.y++;
		if (Input.GetKey(KeyCode.S) && gridPosition.y > 0) // Down
			gridPosition.y--;

		//if (!GameGrid.instance.gridCell[x, y].visited)
		//{
		//	GameGrid.instance.gridCell[x, y].visited = true;
		//	Instantiate(pathPrefab, new Vector3(x, 0, y), Quaternion.identity, GameGrid.instance.transform);
		//}

		

		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, 1000f, mouseRaycastLayer))
		{
			roundedMousePos.x = Mathf.FloorToInt(hitInfo.point.x + 0.5f);
			roundedMousePos.y = Mathf.FloorToInt(hitInfo.point.z + 0.5f);

			coordTMP.text =
				$"x : {hitInfo.point.x}\ny : {hitInfo.point.z}\n" +
				$"x : {roundedMousePos.x}\ny : {roundedMousePos.y}";
			
		}

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

		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			_currentPathIndex = 0;
			_currentPath = path;
		}

		FollowPath();
	}

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

	[System.Serializable]
	public class Action
	{
		[Header("Action Properties")]
		[SerializeField] float duration = 0.5f;
		[SerializeField] int tickCost = 1;
		[SerializeField] ActionMode mode;

		//[Header("Technical")]
		//int t;
	}

	[System.Serializable]
	public enum ActionMode
	{
		None,
		Performing,
		Walk,
		Heal,
		Shield,
		Arrow,
		Spell,
		Axe,
		Sword,
		Hammer,
	}
}

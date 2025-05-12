using UnityEngine;

public class GameGrid : MonoBehaviour
{
	public const int GRID_SIZE = 64;
	public static GameGrid instance;

	[Header("Properties")]
	[SerializeField] float scale = 10f;
	[SerializeField] Vector2Int offset;

	[Header("Technical")]
	[SerializeField] GameObject cellPrefab;
	[SerializeField] GameObject voidPrefab;
	[SerializeField] Material blackMat;
	[SerializeField] Material whiteMat;

	public Cell[,] gridCell = new Cell[GRID_SIZE, GRID_SIZE];

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
			Destroy(gameObject);
	}

	void Start()
	{
		offset = new(Random.Range(-1_000_000, 1_000_000), Random.Range(-1_000_000, 1_000_000));

		for (int x = 0; x < GRID_SIZE; x++)
		{
			for (int y = 0; y < GRID_SIZE; y++)
			{
				GameObject obj;

				float noiseValue = Mathf.PerlinNoise((float)x / GRID_SIZE * scale + offset.x, (float)y / GRID_SIZE * scale + offset.y);

				if (noiseValue > 0.6f && x > 0 && x < GRID_SIZE - 1 && y > 0 && y < GRID_SIZE - 1)
				{
					obj = Instantiate(voidPrefab, new Vector3(x, 0, y), Quaternion.identity, transform);

					gridCell[x, y] = new Cell(x, y, obj, false);
				}
				else
				{
					obj = Instantiate(cellPrefab, new Vector3(x, 0, y), Quaternion.identity, transform);

					if (((x & 1) == 0 && (y & 1) == 0) || ((x & 1) == 1 && (y & 1) == 1))
						obj.GetComponentInChildren<Renderer>().material = blackMat;
					else
						obj.GetComponentInChildren<Renderer>().material = whiteMat;
					gridCell[x, y] = new Cell(x, y, obj, true);
				}


			}
		}
	}

	void Update()
	{
		
	}
}

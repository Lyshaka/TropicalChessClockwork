using UnityEngine;

[System.Serializable]
public class AttackArea
{
	public BoolGrid5x5 area;
	private Vector2Int[] _positions;

	public void InitializePositions()
	{
		int size = 0;
		int index = 0;

		for (int i = 0; i < 25; i++)
		{
			if (area.cells[i] && i != 12)
				size++;
		}

		_positions = new Vector2Int[size];
		//Debug.Log("Size " +  size);

		for (int i = 0; i < 25; i++)
		{
			if (area.cells[i] && i != 12)
			{
				_positions[index] = new Vector2Int(i % 5, 4 - (i / 5));
				//Debug.Log($"Name : {this}, Pos : {_positions[index]}");
				index++;
			}
		}
	}

	public Vector2Int[] GetPositions()
	{
		return _positions;
	}
}

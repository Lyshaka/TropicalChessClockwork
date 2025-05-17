[System.Serializable]
public class Directions
{
	public AttackArea east;
	public AttackArea southEast;
	public AttackArea south;
	public AttackArea southWest;
	public AttackArea west;
	public AttackArea northWest;
	public AttackArea north;
	public AttackArea northEast;

	public void InitializePositions()
	{
		//UnityEngine.Debug.Log("east");
		east.InitializePositions();
		//UnityEngine.Debug.Log("southEast");
		southEast.InitializePositions();
		//UnityEngine.Debug.Log("south");
		south.InitializePositions();
		//UnityEngine.Debug.Log("southWest");
		southWest.InitializePositions();
		//UnityEngine.Debug.Log("west");
		west.InitializePositions();
		//UnityEngine.Debug.Log("northWest");
		northWest.InitializePositions();
		//UnityEngine.Debug.Log("north");
		north.InitializePositions();
		//UnityEngine.Debug.Log("northEast");
		northEast.InitializePositions();
	}

	public AttackArea GetAreaFromDirection(CardinalDirection direction)
	{
		return direction switch
		{
			CardinalDirection.East => east,
			CardinalDirection.SouthEast => southEast,
			CardinalDirection.South => south,
			CardinalDirection.SouthWest => southWest,
			CardinalDirection.West => west,
			CardinalDirection.NorthWest => northWest,
			CardinalDirection.North => north,
			CardinalDirection.NorthEast => northEast,
			_ => null,
		};
	}
}

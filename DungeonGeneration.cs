using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGeneration : MonoBehaviour {

	[SerializeField]
	private Vector2Int[] possibleObstacleSizes;

	[SerializeField]
	private GameObject[] possibleEnemies;

	[SerializeField]
	private TileBase obstacleTile;

	[SerializeField]
	private GameObject goalPrefab;


	private int numberOfRooms;
	private int numberOfEnemies;
	private int numberOfObstacles;
	private Room[,] rooms;
	private Room currentRoom;
	private static DungeonGeneration instance = null;

	void Awake () {
		if (instance == null) {
			DontDestroyOnLoad (this.gameObject);
			instance = this;
			this.currentRoom = GenerateDungeon ();
			PrintGrid();
		} else {
			string roomPrefabName = instance.currentRoom.PrefabName ();
			GameObject roomObject = (GameObject) Instantiate (Resources.Load (roomPrefabName));
			Tilemap tilemap = roomObject.GetComponentInChildren<Tilemap>();
			instance.currentRoom.AddPopulationToTilemap(tilemap, instance.obstacleTile);
			Destroy (this.gameObject);
		}
	}

	void Start () {
		string roomPrefabName = this.currentRoom.PrefabName ();
		GameObject roomObject = (GameObject) Instantiate (Resources.Load (roomPrefabName));
		Tilemap tilemap = roomObject.GetComponentInChildren<Tilemap>();
		this.currentRoom.AddPopulationToTilemap(tilemap, this.obstacleTile);

	}

	private Room GenerateDungeon()
    {
        numberOfRooms = Random.Range(2, 30);

		int gridSize = 3 * numberOfRooms;

		rooms = new Room[gridSize, gridSize];

		Vector2Int initialRoomCoordinate = new Vector2Int ((gridSize / 2) - 1, (gridSize / 2) - 1);

		Queue<Room> roomsToCreate = new Queue<Room> ();
		roomsToCreate.Enqueue (new Room(initialRoomCoordinate.x, initialRoomCoordinate.y));
		List<Room> createdRooms = new List<Room> ();
		while (roomsToCreate.Count > 0 && createdRooms.Count < numberOfRooms) {
			Room currentRoom = roomsToCreate.Dequeue ();
			this.rooms [currentRoom.roomCoordinate.x, currentRoom.roomCoordinate.y] = currentRoom;
			createdRooms.Add (currentRoom);
			AddNeighbors (currentRoom, roomsToCreate);
		}

		int maximumDistanceToInitialRoom = 0;
		Room finalRoom = null;
		foreach (Room room in createdRooms) {
			List<Vector2Int> neighborCoordinates = room.NeighborCoordinates ();
			foreach (Vector2Int coordinate in neighborCoordinates) {
				Room neighbor = this.rooms [coordinate.x, coordinate.y];
				if (neighbor != null) {
					room.Connect (neighbor);
				}
			}

			numberOfObstacles = Random.Range(0, 5);
			numberOfEnemies = Random.Range(0, 4);

			room.PopulateObstacles(this.numberOfObstacles, this.possibleObstacleSizes);
			room.PopulatePrefabs(this.numberOfEnemies, this.possibleEnemies);

			int distanceToInitialRoom = Mathf.Abs(room.roomCoordinate.x - initialRoomCoordinate.x) + Mathf.Abs(room.roomCoordinate.y - initialRoomCoordinate.y);
			if (distanceToInitialRoom > maximumDistanceToInitialRoom){
				maximumDistanceToInitialRoom = distanceToInitialRoom;
				finalRoom = room;
            }
		}

		GameObject[] goalPrefabs = { this.goalPrefab };
		finalRoom.PopulatePrefabs(1, goalPrefabs);

		return this.rooms [initialRoomCoordinate.x, initialRoomCoordinate.y];
	}

	private void AddNeighbors(Room currentRoom, Queue<Room> roomsToCreate) {
		List<Vector2Int> neighborCoordinates = currentRoom.NeighborCoordinates ();
		List<Vector2Int> availableNeighbors = new List<Vector2Int> ();
		foreach (Vector2Int coordinate in neighborCoordinates) {
			if (this.rooms[coordinate.x, coordinate.y] == null) {
				availableNeighbors.Add (coordinate);
			}
		}
			
		int numberOfNeighbors = (int)Random.Range (1, availableNeighbors.Count);

		for (int neighborIndex = 0; neighborIndex < numberOfNeighbors; neighborIndex++) {
			float randomNumber = Random.value;
			float roomFrac = 1f / (float)availableNeighbors.Count;
			Vector2Int chosenNeighbor = new Vector2Int(0, 0);
			foreach (Vector2Int coordinate in availableNeighbors) {
				if (randomNumber < roomFrac) {
					chosenNeighbor = coordinate;
					break;
				} else {
					roomFrac += 1f / (float)availableNeighbors.Count;
				}
			}
			roomsToCreate.Enqueue (new Room(chosenNeighbor));
			availableNeighbors.Remove (chosenNeighbor);
		}
	}

	private void PrintGrid() {
		for (int rowIndex = 0; rowIndex < this.rooms.GetLength (1); rowIndex++) {
			string row = "";
			for (int columnIndex = 0; columnIndex < this.rooms.GetLength (0); columnIndex++) {
				if (this.rooms [columnIndex, rowIndex] == null) {
					row += "X";
				} else {
					row += "R";
				}
			}
			Debug.Log (row);
		}
	}

	public void MoveToRoom(Room room) {
		this.currentRoom = room;
	}

	public Room CurrentRoom() {
		return this.currentRoom;
	}
		
	public void ResetDungeon(){
		this.currentRoom = GenerateDungeon();
    }
}

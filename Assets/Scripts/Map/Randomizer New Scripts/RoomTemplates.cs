using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTemplates : MonoBehaviour {

	public GameObject[] bottomRooms;
	public GameObject[] topRooms;
	public GameObject[] leftRooms;
	public GameObject[] rightRooms;

	public GameObject closedRoom;

	public List<GameObject> rooms;

	public float waitTime;
	private bool spawnedBoss;
	public GameObject boss;

	void Update()
	{
		if(waitTime <= 0 && spawnedBoss == false) //if wait time less than zero and no boss has been spawned
		{
			for (int i = 0; i < rooms.Count; i++) //look at list of rooms
			{
				if(i == rooms.Count-1) //go to last room spawned, and set it to be a boss room
				{
					Instantiate(boss, rooms[i].transform.position, Quaternion.identity); 
					spawnedBoss = true;
				}
			}
		} 
		else 
		{
			if (waitTime > 0) { waitTime -= Time.deltaTime; }
			if (waitTime < 0) { waitTime = 0; }
		}
	}
}

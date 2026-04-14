using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    Vector2 worldSize = new Vector2(4, 4); // currently half the size of normal world for 8x8 grid
    RoomType[,] rooms;
    public List<Vector2> takenPositions = new List<Vector2>(); // easier to search taken positions in the world
    public int gridSizeX, gridSizeY, numberOfRooms = 20;
    public GameObject roomWhiteObj;

    // room type 0 - entry/starting room
    // room type 1 - normal room
    // room type 2 - boss room 
    // room type 3 - special room (for when we create caves? or possibly for special rooms?)

    void Start()
    {
        if (numberOfRooms >= (worldSize.x * 2) * (worldSize.y * 2)) // are there more rooms that can fit in grid?
        {
            numberOfRooms = Mathf.RoundToInt((worldSize.x * 2) * (worldSize.y * 2));
        }
        gridSizeX = Mathf.RoundToInt((worldSize.x); // the space of the world x value is set to grid size x
        gridSizeY = Mathf.RoundToInt((worldSize.y)); // the space of the world y value is set to grid size y
        CreateRooms();
        SetRoomDoors();
        DrawMap();
    }
    
    void CreateRooms()
    {
        // setup
        rooms = new RoomType[gridSizeX * 2, gridSizeY * 2]; // create room of specific type for size of grid
        rooms[gridSizeX, gridSizeY] = new RoomType(Vector2.zero, 0); //start in center of the room grid, room type 0 for starting room
        takenPositions.Insert(0, Vector2.zero); 
        Vector2 checkPos = Vector2.zero; 

        // how much clump do we want for the randomizer
        float randomCompare = 0.2f, randomCompareStart = 0.2f, randomCompareEnd = 0.01f; //farther into loop, smaller the branch out

        
        for (int i = 0; i < numberOfRooms - 1; i++) // add rooms, run once for each room we make
        {
            // math 
            float randomPerc = ((float)i) / (((float)numberOfRooms - 1)); // percent of room completion (current room number / total room number)
            randomCompare = Mathf.Lerp(randomCompareStart, randomCompareEnd, randomPerc); // calculate how clumpy based on start, end, and the percentage of completion

            checkPos = NewPosition(); // grab new position

            if (NumberOfNeighbors(checkPos, takenPositions) > 1 && Random.value > randomCompare) // test new position
            {
                int iterations = 0;
                do { checkPos = SelectiveNewPosition(); iterations++; } while(NumberOfNeighbors(checkPos, takenPositions) > 1 && iterations < 100); // this checks position until finds safe spot
                if (iterations >= 50) { Debug.Log("error: could not create with fewer neighbors than: " + NumberOfNeighbors(checkPos, takenPositions)); } // if it takes really long to search
            }

            // finalize position
            rooms[(int)checkPos.x + gridSizeX, (int)checkPos.y + gridSizeY] = new RoomType(checkPos, 1); // calculate offset for array while creating it in new position, room type 1 for normal room
            takenPositions.Insert(0, checkPos); // mark position as taken
        }
    }

    Vector2 NewPosition()
    {
        // initialize blank slate
        int x = 0, y = 0;
        Vector2 checkingPos = Vector2.zero;

        do // do (this) while (that happens)
        {
            int index = Mathf.RoundToInt(Random.value * (takenPositions.Count - 1)); // get index
            x = (int)takenPositions[index].x; // get x value
            y = (int)takenPositions[index].y; // get y value
            bool UpDown = (Random.value < 0.5f); // randomly decide if moving vertical/horizontal
            bool positive = (Random.value < 0.5f); // randomly decide if positive/negative

            if (UpDown) // if moving vertical
            {
                if (positive) { y += 1; }
                else { y -= 1; }
            }
            else // if moving horizontalright 
            {
                if(positive) { x += 1; }
                else { x -= 1; }
            }

        } while (takenPositions.Contains(checkingPos) || x >= gridSizeX || x < -gridSizeX || y >= gridSizeY || y < -gridSizeY);
        return checkingPos;
    }
}

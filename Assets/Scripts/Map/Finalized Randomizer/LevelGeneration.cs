using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LightTransport.PostProcessing;

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
        gridSizeX = Mathf.RoundToInt((worldSize.x)); // the space of the world x value is set to grid size x
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
            bool UpDown = (Random.value < 0.5f); // randomly decide if moving vertical/horizontal (up/down)
            bool positive = (Random.value < 0.5f); // randomly decide if positive/negative (left/right)

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

    Vector2 SelectiveNewPosition() 
    {
        // reset data
        int index = 0, inc = 0;
        int x = 0, y = 0;
        Vector2 checkingPos = Vector2.zero;

        do // modified from new room to find rooms with only one neighbor position
        {
            inc = 0;
            do { index = Mathf.RoundToInt(Random.value * (takenPositions.Count - 1)); inc++; } while (NumberOfNeighbors(takenPositions[index], takenPositions) > 1 && inc < 100); // get index of room when neighbors greater than 1
            x = (int) takenPositions[index].x; // get x value
            y = (int) takenPositions[index].y; // get y value
            bool UpDown = (Random.value < 0.5f); // randomly decide if moving vertical/horizontal (up/down)
            bool positive = (Random.value < 0.5f); // randomly decide if positive/negative (left/right)

            if (UpDown) // if moving vertical
            {
                if (positive) { y += 1; }
                else { y -= 1; }
            }
            else // if moving horizontalright 
            {
                if (positive) { x += 1; }
                else { x -= 1; }
            }

            checkingPos = new Vector2(x, y);
        } while (takenPositions.Contains(checkingPos) || x >= gridSizeX || x < -gridSizeX || y >= gridSizeY || y < -gridSizeY);

        if(inc >= 100) { Debug.Log("error: could not find position with only one neighbor!"); }

        return checkingPos;
    }
    int NumberOfNeighbors(Vector2 checkingPos, List<Vector2> usedPositions)
    {
        int ret = 0; // reset value every time you check

        //increment it for each taken position that is fulfilled - ie if one spot if filled then it is 1, if all fouer sps filled then it is 4
        if (usedPositions.Contains(checkingPos + Vector2.right)) { ret++; }
        if (usedPositions.Contains(checkingPos + Vector2.left)) { ret++; }
        if (usedPositions.Contains(checkingPos + Vector2.up)) { ret++; }
        if (usedPositions.Contains(checkingPos + Vector2.down)) { ret++; }

        return ret; // return the value so you know how many neighbors are around the position
    }

    void SetRoomDoors() // find where rooms around are located from current position
    {
        for (int x = 0; x < ((gridSizeX * 2)); x++) // checks every x position in array
        {
            for(int y = 0; y < ((gridSizeY * 2)); y++) // checks every y position in array (as a coordinate of (x,y) now)
            {
                if (rooms[x,y] == null) { continue; } // if there is nothing there, go to next position of the array

                Vector2 gridPosition = new Vector2(x, y); // check to see if there is a room in each cardinal direction
                
                if (y - 1 < 0) { rooms[x, y].doorBottom = false; } // check above
                else { rooms[x,y].doorBottom = (rooms[x, y - 1] != null); }

                if (y + 1 >= gridSizeY * 2) { rooms[x,y].doorTop = false; } // check bellow
                else { rooms[x, y].doorTop = (rooms[x, y + 1] != null); }

                if (x - 1 < 0) { rooms[x,y].doorLeft  = false; } // check left
                else { rooms[x, y].doorLeft = (rooms[x - 1, y] != null); }

                if (x + 1 >= gridSizeX * 2) { rooms[x,y].doorRight = false; } // check right
                else { rooms[x, y].doorRight = (rooms[x + 1, y] != null); }
            }
        }
    }

    void DrawMap()
    {
        foreach (RoomType room in rooms) // loop for every coordinate
        {
            if (room == null) { continue; } //if there isnt a room in that position on the map, skip to next slot

            Vector2 drawPos = room.gridPos; // gather coordinate

            // multiply to size of map sprite px
            drawPos.x *= 16;
            drawPos.y *= 16;

            // draw based on info in Map Sprite Selector script
            MapSpriteSelector mapper = Object.Instantiate(roomWhiteObj, drawPos, Quaternion.identity).GetComponent<MapSpriteSelector>();
            mapper.type = room.type;
            mapper.up = room.doorTop;
            mapper.down = room.doorBottom;
            mapper.right = room.doorRight;
            mapper.left = room.doorLeft;
        }
    }
}

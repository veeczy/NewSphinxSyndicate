using UnityEngine;

public class RoomType
{
    public Vector2 gridPos;
    public int type;
    public bool doorTop, doorBottom, doorLeft, doorRight;

    public RoomType(Vector2 _gridPos, int _type) //instance of room, set the type information
    {
        gridPos = _gridPos;
        type = _type;
    }
}

using UnityEngine;

public class Cube : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public bool isMatched;
    private Vector2 currentPos;
    private Vector2 targetPos;
    public bool isMoving;
    public CubeType cubeType;

    public void SetIndices(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public enum CubeType
    {
        Blue,
        Red,
        Green,
        Yellow
    }
}
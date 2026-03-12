using UnityEngine;

public class Box : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    public void SetIndices(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void TakeDamage()
    {
        CubeBoard.Instance.ClearBox(this);
    }
}
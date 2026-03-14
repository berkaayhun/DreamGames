using UnityEngine;

public class Stone : MonoBehaviour
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
        // 1. Tahtadaki (hafızadaki) yerini tamamen boşaltıyoruz
        if (CubeBoard.Instance.cubeBoard[xIndex, yIndex] != null)
        {
            CubeBoard.Instance.cubeBoard[xIndex, yIndex].cube = null;
        }
        
        // 2. Fiziksel objeyi yok et (İleride taş kırılma partiküllerini buraya ekleyeceğiz)
        Destroy(gameObject);
    }
}
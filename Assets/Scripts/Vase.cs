using UnityEngine;
using System.Collections;

public class Vase : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    
    [Header("Can ve Görseller")]
    public int hp = 2; // Vazonun 2 canı var
    public Sprite fullVaseSprite;    // Sağlam vazo resmi
    public Sprite damagedVaseSprite; // Çatlak vazo resmi
    
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Oyun başlarken sağlam resmi yükle
        if (fullVaseSprite != null) spriteRenderer.sprite = fullVaseSprite;
    }

    public void SetIndices(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    // Hasar Alma Metodu
    public void TakeDamage()
    {
        hp--; // Canı 1 azalt

        if (hp == 1)
        {
            // 1 canı kaldıysa görseli çatlak vazo ile değiştir
            if (damagedVaseSprite != null) spriteRenderer.sprite = damagedVaseSprite;
        }
        else if (hp <= 0)
        {
            // Canı 0 olduysa tahtadan sil (Partikülleri buraya ekleyeceğiz)
            CubeBoard.Instance.cubeBoard[xIndex, yIndex].cube = null;
            Destroy(gameObject);
        }
    }

    // PDF'e göre Vazo aşağı düşebildiği için düşme animasyonu
    public void MoveToTarget(Vector2 targetPos)
    {
        StartCoroutine(MoveCoroutine(targetPos));
    }

    IEnumerator MoveCoroutine(Vector2 targetPos)
    {
        float duration = 0.15f;
        float elapsed = 0f;
        Vector2 startPos = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector2.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
    }
}
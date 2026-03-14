using UnityEngine;
using System.Collections;

public class Cube : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public bool isMoving;
    public CubeType cubeType;
    public GameObject destroyIcon;
    public GameObject rocketHintIcon;
    
    public void SetIndices(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void ShowDestroyIcon()
    {   
        if (destroyIcon == null)
        {
            Debug.LogWarning("destroyIcon boş!");
            return;
        }

        Instantiate(destroyIcon, transform.position, Quaternion.identity);
    }

    public void SetRocketHint(bool isEligible)
    {
        if (rocketHintIcon != null)
        {
            rocketHintIcon.SetActive(isEligible);

            SpriteRenderer cubeSprite = GetComponent<SpriteRenderer>();
            if (cubeSprite != null)
            {
               
                cubeSprite.enabled = !isEligible; 
            }
        }
    }
    public void MoveToTarget(Vector2 targetPos)
    {
        StartCoroutine(MoveCoroutine(targetPos));
    }

    private IEnumerator MoveCoroutine(Vector2 targetPos)
    {
        isMoving = true;
        float duration = 0.2f;
        Vector2 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }

    public enum CubeType
    {
        Blue, Red, Green, Yellow
    }
}


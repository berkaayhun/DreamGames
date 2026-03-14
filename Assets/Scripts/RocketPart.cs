using UnityEngine;

public class RocketPart : MonoBehaviour
{
    public Vector2 direction; // Parçanın uçacağı yön (sağ, sol, yukarı, aşağı)
    public float speed = 15f; // Uçuş hızı

    void Update()
    {
        // Her karede belirlediğimiz yöne doğru hareket et
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void Start()
    {
        // Ekranda sonsuza kadar gidip hafızayı doldurmasın diye 2 saniye sonra kendini yok et
        Destroy(gameObject, 2f); 
    }
}
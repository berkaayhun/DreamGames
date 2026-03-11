using UnityEngine;

public class CubeBoard : MonoBehaviour
{
    public int width = 6;
    public int height = 8;

    private float spacingX;
    private float spacingY;

    public GameObject[] cubePrefabs;
    public Node[,] cubeBoard;
    public GameObject cubeBoardGO; // Değişken adını buradakiyle aynı yap (Inspector'da sürüklemeyi unutma)
    public ArrayLayout arrayLayout;
    public static CubeBoard Instance;

    public void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeBoard();
    }

    void InitializeBoard()
    {
        // 1. Diziyi oluştur
        cubeBoard = new Node[width, height];

        // 2. Spacing hesaplamasını düzelt (float bölmesi için '2f' kullanıyoruz)
        spacingX = (float)(width - 1) / 2f;
        spacingY = (float)(height - 1) / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 3. Güvenlik Kontrolü: Prefab atanmamışsa hata verme, döngüden çık
                if (cubePrefabs == null || cubePrefabs.Length == 0)
                {
                    Debug.LogError("Lütfen Inspector panelinden Cube Prefabs listesini doldur!");
                    return;
                }

                // 4. Pozisyonu hesapla
                Vector2 position = new Vector2(x - spacingX, y - spacingY);
                if(arrayLayout.rows[y].row[x]){
                    cubeBoard[x,y] = new Node(false,null);

                }else{
                    int randomIndex = Random.Range(0, cubePrefabs.Length);
                    GameObject cube = Instantiate(cubePrefabs[randomIndex], position, Quaternion.identity);

                    // 6. Hiyerarşiyi temiz tutmak için küpü bir objenin altına koy
                    if (cubeBoardGO != null)
                    {
                        cube.transform.parent = cubeBoardGO.transform;
                    }

                    // 7. Script kontrolü ve Node ataması
                    Cube cubeScript = cube.GetComponent<Cube>();
                    if (cubeScript != null)
                    {
                        cubeScript.SetIndices(x, y);
                        cubeBoard[x, y] = new Node(true, cube);
                    }
                    else
                    {
                        Debug.LogWarning(cube.name + " üzerinde Cube scripti bulunamadı!");
                    }
                }
                
            }
        }
    }
}
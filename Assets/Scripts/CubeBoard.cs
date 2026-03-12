using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeBoard : MonoBehaviour
{
    public static CubeBoard Instance;
    
    public int width = 6;
    public int height = 8;

    public GameObject[] cubePrefabs;
    public GameObject cubeBoardGO;
    public GameObject boxPrefab;

    public Node[,] cubeBoard;

    private float spacingX;
    private float spacingY;

    [SerializeField] private bool isProcessingMove;

    void Awake() => Instance = this;
    void Start() => InitializeBoard();

    void Update()
    {
        if (isProcessingMove) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                Cube cube = hit.collider.gameObject.GetComponent<Cube>();
                if (cube != null)
                {
                    Debug.Log("Tıklanan küp: " + cube.cubeType + " [" + cube.xIndex + "," + cube.yIndex + "]");
                    SelectCube(cube);
                }
            }
        }
    }

    void InitializeBoard()
    {
        if (cubeBoardGO != null)
            foreach (Transform child in cubeBoardGO.transform)
                Destroy(child.gameObject);

        cubeBoard = new Node[width, height];
        spacingX = (float)(width - 1) / 2f;
        spacingY = (float)(height - 1) / 2f;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                SpawnCube(x, y);

        Debug.Log("Board hazır, oyun başlıyor!");
    }

    void SpawnCube(int x, int y)
    {
        Vector2 pos = new Vector2(x - spacingX, y - spacingY);
        int randomIndex = Random.Range(0, cubePrefabs.Length);
        GameObject cubeGO = Instantiate(
            cubePrefabs[randomIndex], pos, Quaternion.identity, cubeBoardGO.transform);

        Cube cube = cubeGO.GetComponent<Cube>();
        cube.SetIndices(x, y);
        cubeBoard[x, y] = new Node(true, cubeGO);
    }

    void SelectCube(Cube cube)
    {
        List<Cube> group = GetBlastGroup(cube);
        Debug.Log("Grup büyüklüğü: " + group.Count);

        if (group.Count < 2)
        {
            Debug.Log("Yeterli komşu yok, blast olmadı.");
            return;
        }

        isProcessingMove = true;
        StartCoroutine(BlastAndRefill(group));
    }

    void SpawnBox(int x, int y)
    {
    Vector2 pos = new Vector2(x - spacingX, y - spacingY);
    GameObject boxGO = Instantiate(boxPrefab, pos, Quaternion.identity, cubeBoardGO.transform);

    BoxObstacle box = boxGO.GetComponent<BoxObstacle>();
    box.SetIndices(x, y);

    cubeBoard[x, y] = new Node(true, boxGO);
    } 
      
    List<Cube> GetBlastGroup(Cube startCube)
    {
        List<Cube> result = new List<Cube>();
        Queue<Cube> queue = new Queue<Cube>();
        HashSet<Cube> visited = new HashSet<Cube>();

        queue.Enqueue(startCube);
        visited.Add(startCube);

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (queue.Count > 0)
        {
            Cube current = queue.Dequeue();
            result.Add(current);

            for (int i = 0; i < 4; i++)
            {
                int nx = current.xIndex + dx[i];
                int ny = current.yIndex + dy[i];

                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                if (cubeBoard[nx, ny]?.cube == null) continue;

                Cube neighbor = cubeBoard[nx, ny].cube.GetComponent<Cube>();
                if (neighbor == null || visited.Contains(neighbor)) continue;

                if (neighbor.cubeType == startCube.cubeType)
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return result;
    }

    IEnumerator BlastAndRefill(List<Cube> group)
    {
        foreach (Cube cube in group)
        {
            cube.ShowDestroyIcon();
        }

        yield return new WaitForSeconds(0.12f);

        foreach (Cube cube in group)
        {
            cubeBoard[cube.xIndex, cube.yIndex].cube = null;
            Destroy(cube.gameObject);
        }

    yield return new WaitForSeconds(0.15f);

        yield return new WaitForSeconds(0.15f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cubeBoard[x, y].cube == null)
                {
                    for (int yAbove = y + 1; yAbove < height; yAbove++)
                    {
                        if (cubeBoard[x, yAbove].cube != null)
                        {
                            GameObject fallingGO = cubeBoard[x, yAbove].cube;
                            Cube fallingCube = fallingGO.GetComponent<Cube>();

                            cubeBoard[x, y].cube = fallingGO;
                            cubeBoard[x, yAbove].cube = null;

                            fallingCube.SetIndices(x, y);

                            Vector2 targetPos = new Vector2(x - spacingX, y - spacingY);
                            fallingCube.MoveToTarget(targetPos);

                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.25f);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (cubeBoard[x, y].cube == null)
                    SpawnCube(x, y);

        yield return new WaitForSeconds(0.1f);

        isProcessingMove = false;
        Debug.Log("Blast tamamlandı, yeni tıklama bekleniyor.");
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeBoard : MonoBehaviour
{
    public static CubeBoard Instance;
    
   

    public GameObject[] cubePrefabs;
    public GameObject cubeBoardGO;
    public GameObject boxPrefab;
    public GameObject stonePrefab;
    public GameObject vasePrefab;

    [HideInInspector] public int width;
    [HideInInspector] public int height;
    public GameObject horizontalRocketPrefab;
    public GameObject verticalRocketPrefab;
    
  
    public GameObject rocketPartLeftPrefab;
    public GameObject rocketPartRightPrefab;
    public GameObject rocketPartTopPrefab;
    public GameObject rocketPartBottomPrefab;
    public GameObject rocketExplosionParticle;

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
                Rocket rocket = hit.collider.gameObject.GetComponent<Rocket>();

                if (cube != null)
                {
                    SelectCube(cube);
                }
                else if (rocket != null)
                {
                    // ROKETE TIKLANDIĞINDA BURASI ÇALIŞACAK
                    SelectRocket(rocket);
                }
            }
        }
    }

    void InitializeBoard()
    {
        if (cubeBoardGO != null)
            foreach (Transform child in cubeBoardGO.transform)
                Destroy(child.gameObject);

        // 1. PATRONDAN (JSON) BOYUTLARI VE HARİTAYI ÇEK!
        width = LevelManager.Instance.currentLevelData.grid_width;
        height = LevelManager.Instance.currentLevelData.grid_height;
        string[] levelGrid = LevelManager.Instance.currentLevelData.grid;

        cubeBoard = new Node[width, height];
        spacingX = (float)(width - 1) / 2f;
        spacingY = (float)(height - 1) / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                
                int jsonIndex = y * width + x;
                string itemType = levelGrid[jsonIndex];

                if (itemType == "bo"){
                    SpawnObstacle(x, y, boxPrefab);
                } 
                else if (itemType == "s"){
                    SpawnObstacle(x, y, stonePrefab);
                } 
                else if (itemType == "v"){
                    SpawnObstacle(x, y, vasePrefab);
                } 
                else 
                {
                    
                    SpawnSpecificCube(x, y, itemType); 
                }
            }
        }
        UpdateRocketHints(); 
    }

    void SpawnCube(int x, int y)
    {
        Vector2 pos = new Vector2(x - spacingX, y - spacingY);
        int randomIndex = Random.Range(0, cubePrefabs.Length);
        GameObject cubeGO = Instantiate(cubePrefabs[randomIndex], pos, Quaternion.identity, cubeBoardGO.transform);

        Cube cube = cubeGO.GetComponent<Cube>();
        cube.SetIndices(x, y);
        cubeBoard[x, y] = new Node(true, cubeGO);
    }
    void SpawnSpecificCube(int x, int y, string type)
    {
        Vector2 pos = new Vector2(x - spacingX, y - spacingY);
        int cubeIndex = 0; // Varsayılan

        // HARF KONTROLÜ
        if (type == "r") cubeIndex = 0;       // Red
        else if (type == "g") cubeIndex = 1;  // Green
        else if (type == "b") cubeIndex = 2;  // Blue
        else if (type == "y") cubeIndex = 3;  // Yellow
        else if (type == "rand") cubeIndex = Random.Range(0, cubePrefabs.Length); // Rastgele
        
        if (type == "vro" || type == "hro")
        {
            GameObject rocketToSpawn = (type == "vro") ? verticalRocketPrefab : horizontalRocketPrefab;
            GameObject rocketGO = Instantiate(rocketToSpawn, pos, Quaternion.identity, cubeBoardGO.transform);
            Rocket rocket = rocketGO.GetComponent<Rocket>();
            if (rocket != null) rocket.SetIndices(x, y);
            cubeBoard[x, y] = new Node(true, rocketGO);
            return; // Roket ürettik, aşağıdaki küp üretme koduna geçme
        }
        // Seçilen küpü yarat
        GameObject cubeGO = Instantiate(cubePrefabs[cubeIndex], pos, Quaternion.identity, cubeBoardGO.transform);
        Cube cube = cubeGO.GetComponent<Cube>();
        cube.SetIndices(x, y);
        cubeBoard[x, y] = new Node(true, cubeGO);
    }

    void SelectCube(Cube cube)
    {
        List<Cube> group = GetBlastGroup(cube);
        if (group.Count < 2) return;
        if (LevelManager.Instance.isGameOver) return; 
        // Geçerli bir hamle yapıldı, sayacı 1 düşür!
        LevelManager.Instance.DecreaseMove();
    
        isProcessingMove = true;
        StartCoroutine(BlastAndRefill(group, cube));
    }

    // YENİ: ROKETİ SEÇME METODU
    void SelectRocket(Rocket rocket)
    {
        if (LevelManager.Instance.isGameOver) return; 
        // Roket patlatmak da geçerli bir hamledir, sayacı 1 düşür!
        LevelManager.Instance.DecreaseMove();
        isProcessingMove = true;
        StartCoroutine(RocketBlastCoroutine(rocket));
    }

    void DamageAdjacentObstacles(List<Cube> group)
    {   
        HashSet<Box> boxesToDamage = new HashSet<Box>();
        HashSet<Vase> vasesToDamage = new HashSet<Vase>();
        // DİKKAT: Buraya bilerek Stone eklemiyoruz! PDF kuralı: Taş yanındaki patlamadan hasar almaz.
        
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        foreach (Cube cube in group)
        {
            for (int i = 0; i < 4; i++)
            {
                int nx = cube.xIndex + dx[i];
                int ny = cube.yIndex + dy[i];

                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                if (cubeBoard[nx, ny] == null || cubeBoard[nx, ny].cube == null) continue;

                GameObject neighborGO = cubeBoard[nx, ny].cube;

                Box box = neighborGO.GetComponent<Box>();
                if (box != null) boxesToDamage.Add(box);

                Vase vase = neighborGO.GetComponent<Vase>();
                if (vase != null) vasesToDamage.Add(vase); // Vazo patlamadan hasar alır (PDF Kuralı)
            }
        }
        
        // Toplanan engellere hasar ver
        foreach (Box box in boxesToDamage) box.TakeDamage();
        foreach (Vase vase in vasesToDamage) vase.TakeDamage();
    }

    public void ClearBox(Box box)
    {
        cubeBoard[box.xIndex, box.yIndex].cube = null;
        Destroy(box.gameObject);
    }

    void SpawnObstacle(int x, int y, GameObject prefabToSpawn)
    {
        Vector2 pos = new Vector2(x - spacingX, y - spacingY);
        GameObject obstacleGO = Instantiate(prefabToSpawn, pos, Quaternion.identity, cubeBoardGO.transform);
        
        Box box = obstacleGO.GetComponent<Box>();
        if (box != null) box.SetIndices(x, y);

        Vase vase = obstacleGO.GetComponent<Vase>();
        if (vase != null) vase.SetIndices(x, y);
        
        // İŞTE BENİM UNUTTUĞUM, OYUNU BOZAN EKSİK SATIRLAR:
        Stone stone = obstacleGO.GetComponent<Stone>();
        if (stone != null) stone.SetIndices(x, y);

        cubeBoard[x, y] = new Node(true, obstacleGO);
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

    void UpdateRocketHints()
    {
        HashSet<Cube> visited = new HashSet<Cube>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cubeBoard[x, y] != null && cubeBoard[x, y].cube != null)
                {
                    Cube startCube = cubeBoard[x, y].cube.GetComponent<Cube>();
                    if (startCube != null && !visited.Contains(startCube))
                    {
                        List<Cube> group = GetBlastGroup(startCube);
                        bool isEligibleForRocket = group.Count >= 4;
                        foreach (Cube c in group)
                        {
                            visited.Add(c);
                            c.SetRocketHint(isEligibleForRocket);
                        }
                    }
                }
            }
        }
    }

    IEnumerator BlastAndRefill(List<Cube> group, Cube clickedCube)
    {
        DamageAdjacentObstacles(group);
        bool createRocket = group.Count >= 4;
        
        if (createRocket)
        {
            Vector2 targetPos = clickedCube.transform.position;
            int targetX = clickedCube.xIndex;
            int targetY = clickedCube.yIndex;
            
            foreach (Cube cube in group)
                if (cube != clickedCube) cube.MoveToTarget(targetPos);
            
            yield return new WaitForSeconds(0.2f); 
            
            foreach (Cube cube in group)
            {
                cubeBoard[cube.xIndex, cube.yIndex].cube = null;
                Destroy(cube.gameObject);
            }

            GameObject rocketPrefab = Random.value > 0.5f ? horizontalRocketPrefab : verticalRocketPrefab;
            GameObject rocketGO = Instantiate(rocketPrefab, targetPos, Quaternion.identity, cubeBoardGO.transform);
            
            Rocket rocket = rocketGO.GetComponent<Rocket>();
            if (rocket != null) rocket.SetIndices(targetX, targetY);
            
            cubeBoard[targetX, targetY] = new Node(true, rocketGO);
        }
        else 
        {
            foreach (Cube cube in group) cube.ShowDestroyIcon();
            yield return new WaitForSeconds(0.12f);
            foreach (Cube cube in group)
            {
                cubeBoard[cube.xIndex, cube.yIndex].cube = null;
                Destroy(cube.gameObject);
            }
        }
        
        yield return new WaitForSeconds(0.15f);

        // Boşlukları doldurma işlemini metoda yönlendirdik
        yield return StartCoroutine(RefillBoardCoroutine());
    }

    // YENİ: ROKET PATLATMA MANTIĞI
    IEnumerator RocketBlastCoroutine(Rocket clickedRocket)
    {
        int targetX = clickedRocket.xIndex;
        int targetY = clickedRocket.yIndex;
        bool isHorz = clickedRocket.isHorizontal;
        Vector2 pos = clickedRocket.transform.position;

        // 1. Tıklanan roketi tahtadan temizle
        cubeBoard[targetX, targetY].cube = null;
        Destroy(clickedRocket.gameObject);
        
        if (rocketExplosionParticle != null)
        {
            GameObject effect = Instantiate(rocketExplosionParticle, pos, Quaternion.identity);
        
        // 2. KÜP TARİFESİ: Unity'ye "Bunu 2 saniye sonra yok et" emrini ver
            Destroy(effect, 1f);
        }
       
        if (isHorz)
        {
            Instantiate(rocketPartLeftPrefab, pos, Quaternion.identity);
            Instantiate(rocketPartRightPrefab, pos, Quaternion.identity);
        }
        else
        {
            Instantiate(rocketPartTopPrefab, pos, Quaternion.identity);
            Instantiate(rocketPartBottomPrefab, pos, Quaternion.identity);
        }

        // Parçaların uçması hissiyatı için minik bir bekleme
        yield return new WaitForSeconds(0.1f);

        List<Cube> cubesToDestroy = new List<Cube>();
        HashSet<Box> boxesToDamage = new HashSet<Box>();
        HashSet<Vase> vasesToDamage = new HashSet<Vase>();
        HashSet<Stone> stonesToDamage = new HashSet<Stone>();
        // 3. Satırı veya Sütunu Tara
        if (isHorz)
        {
            for (int x = 0; x < width; x++)
            {
                if (cubeBoard[x, targetY] != null && cubeBoard[x, targetY].cube != null)
                {
                    GameObject obj = cubeBoard[x, targetY].cube;
                    Cube c = obj.GetComponent<Cube>();
                    Box b = obj.GetComponent<Box>();
                    Rocket r = obj.GetComponent<Rocket>(); 
                    Vase v = obj.GetComponent<Vase>();
                    Stone s = obj.GetComponent<Stone>();

                    if (c != null){
                        cubesToDestroy.Add(c);
                    }
                    if (b != null){
                        boxesToDamage.Add(b);
                    } 
                    if (r != null)
                    {
                        cubeBoard[x, targetY].cube = null;
                        Destroy(r.gameObject);
                    }
                    if (v != null){
                        vasesToDamage.Add(v);
                    } 
                    if (s != null){
                        stonesToDamage.Add(s);
                    }
                }
            }
        }
        else
        {
            for (int y = 0; y < height; y++)
            {
                if (cubeBoard[targetX, y] != null && cubeBoard[targetX, y].cube != null)
                {
                    GameObject obj = cubeBoard[targetX, y].cube;
                    Cube c = obj.GetComponent<Cube>();
                    Box b = obj.GetComponent<Box>();
                    Rocket r = obj.GetComponent<Rocket>();

                    if (c != null) cubesToDestroy.Add(c);
                    if (b != null) boxesToDamage.Add(b);
                    if (r != null)
                    {
                        cubeBoard[targetX, y].cube = null;
                        Destroy(r.gameObject);
                    }
                }
            }
        }

        // 4. Kutulara hasar ver ve Küpleri patlat
        foreach (Box box in boxesToDamage) box.TakeDamage();
        foreach (Vase vase in vasesToDamage) vase.TakeDamage();
        foreach (Stone stone in stonesToDamage) stone.TakeDamage();
        // GÜVENLİK ÖNLEMİ: Beklemeden ÖNCE küplerin tahtadaki yerini boşaltıyoruz!
        foreach (Cube cube in cubesToDestroy) 
        {
            // Küp henüz hayattayken ve koordinatları okunabiliyorken yerini null yap
            cubeBoard[cube.xIndex, cube.yIndex].cube = null;
            cube.ShowDestroyIcon();
        }
        
        
        // Animasyon için bekle
        yield return new WaitForSeconds(0.12f);

        // Bekleme bittikten sonra küpleri fiziksel olarak yok et
        foreach (Cube cube in cubesToDestroy)
        {
            // Eğer küp bu 0.12 saniye içinde zaten yok edildiyse, hata vermemesi için kontrol ediyoruz
            if (cube != null) 
            {
                Destroy(cube.gameObject);
            }
        }

        yield return new WaitForSeconds(0.15f);

        // Boşlukları doldur
        yield return StartCoroutine(RefillBoardCoroutine());
    }

    // TAHTAYI DOLDURMA MANTIĞI (Aynı kod, sadece düzenli dursun diye ayırdık)
    IEnumerator RefillBoardCoroutine()
    {
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
                            GameObject upperGO = cubeBoard[x, yAbove].cube;
                            Cube fallingCube = upperGO.GetComponent<Cube>();
                            Rocket fallingRocket = upperGO.GetComponent<Rocket>();
                            
                            if (fallingCube != null) 
                            {
                                cubeBoard[x, y].cube = upperGO;
                                cubeBoard[x, yAbove].cube = null;
                                fallingCube.SetIndices(x, y);
                                fallingCube.MoveToTarget(new Vector2(x - spacingX, y - spacingY));
                                break; 
                            }
                            else if(fallingRocket != null)
                            {
                                cubeBoard[x, y].cube = upperGO;
                                cubeBoard[x, yAbove].cube = null;
                                fallingRocket.SetIndices(x, y);
                                fallingRocket.MoveToTarget(new Vector2(x - spacingX, y - spacingY));
                                break;
                            }
                            else 
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        yield return new WaitForSeconds(0.25f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cubeBoard[x, y].cube == null)
                {
                    bool isBlocked = false;
                    for (int yAbove = y + 1; yAbove < height; yAbove++)
                    {
                        if (cubeBoard[x, yAbove].cube != null)
                        {
                            Cube upperCube = cubeBoard[x, yAbove].cube.GetComponent<Cube>();
                            Rocket upperRocket = cubeBoard[x, yAbove].cube.GetComponent<Rocket>();
                            if (upperCube == null && upperRocket == null) 
                            {
                                isBlocked = true;
                                break;
                            }
                        }
                    }
                    if (!isBlocked) SpawnCube(x, y);
                }
            }
        }
        
        yield return new WaitForSeconds(0.1f);
        UpdateRocketHints(); 
        isProcessingMove = false;
    }
}
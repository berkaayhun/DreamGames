using UnityEngine;
using System.Collections.Generic;

public class CubeBoard : MonoBehaviour
{
    public int width = 6;
    public int height = 8;

    private float spacingX;
    private float spacingY;

    public GameObject[] cubePrefabs;
    public Node[,] cubeBoard;
    public GameObject cubeBoardGO; 
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
        // 1. ÖNCE ESKİLERİ YOK ET (Cleanup)
        if (cubeBoardGO != null)
        {
            foreach (Transform child in cubeBoardGO.transform)
            {
                // Unity'de objeleri sahneden tamamen siler
                Destroy(child.gameObject);
            }
        }
        cubeBoard = new Node[width, height];

        spacingX = (float)(width - 1) / 2f;
        spacingY = (float)(height - 1) / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (cubePrefabs == null || cubePrefabs.Length == 0)
                {
                    Debug.LogError("Lütfen Inspector panelinden Cube Prefabs listesini doldur!");
                    return;
                }

                Vector2 position = new Vector2(x - spacingX, y - spacingY);
                if(arrayLayout.rows[y].row[x]){
                    cubeBoard[x,y] = new Node(false,null);
                }else{
                    int randomIndex = Random.Range(0, cubePrefabs.Length);
                    GameObject cube = Instantiate(cubePrefabs[randomIndex], position, Quaternion.identity);

                    if (cubeBoardGO != null)
                    {
                        cube.transform.parent = cubeBoardGO.transform;
                    }

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
        
        if(CheckBoard()){
            Debug.Log("We have matches let's re-create the board:");
            InitializeBoard();
        }else{
            Debug.Log("There are no matches, it's time to start the game! ");
        }

    }

    public bool CheckBoard(){
            Debug.Log("Checking Board");
            bool hasMatched = false;

            List<Cube> cubesToRemove = new List<Cube>();

            for(int x = 0; x < width; x++){
                for(int y = 0; y < height; y++ ){
                    if(cubeBoard[x,y].isUsable && cubeBoard[x,y].cube != null){
                        Cube cube = cubeBoard[x,y].cube.GetComponent<Cube>();
                        if(!cube.isMatched){
                            MatchResult matchedCubes = IsConnected(cube);

                            if(matchedCubes.connectedCubes.Count >= 3){
                                cubesToRemove.AddRange(matchedCubes.connectedCubes);
                                foreach(Cube cub in matchedCubes.connectedCubes)
                                    cub.isMatched = true;
                                hasMatched = true;
                            }
                        }
                    }
                }
            }
            return hasMatched;
    }       

    MatchResult IsConnected(Cube cube){
        List<Cube> connectedCubes = new List<Cube>();
        // DÜZELTME: Cube.CubeType olarak erişildi
        Cube.CubeType cubeType = cube.cubeType; 
        connectedCubes.Add(cube);

        CheckDirection(cube, new Vector2Int(1,0), connectedCubes); 
        CheckDirection(cube, new Vector2Int(-1,0), connectedCubes); 

        if(connectedCubes.Count == 3){
            Debug.Log("I have a normal horizontal match, the colof my match is: " + connectedCubes[0].cubeType);
            return new MatchResult{
                
                connectedCubes = new List<Cube>(connectedCubes),
                direction = MatchDirection.Horizontal
            };
        }
        if(connectedCubes.Count > 3){
            Debug.Log("I have a Long horizontal match, the colof my match is: " + connectedCubes[0].cubeType);
            return new MatchResult{
                connectedCubes = new List<Cube>(connectedCubes),
                direction = MatchDirection.LongHorizontal
            };
        }

        connectedCubes.Clear();
        connectedCubes.Add(cube);

        CheckDirection(cube, new Vector2Int(0,1), connectedCubes); 
        CheckDirection(cube, new Vector2Int(0,-1), connectedCubes); 

        if(connectedCubes.Count == 3){
            Debug.Log("I have a normal vertical match, the colof my match is: " + connectedCubes[0].cubeType);
            return new MatchResult{
                connectedCubes = new List<Cube>(connectedCubes),
                direction = MatchDirection.Vertical
            };
        }else if(connectedCubes.Count > 3){
            Debug.Log("I have a Long horizontal match, the colof my match is: " + connectedCubes[0].cubeType);
            return new MatchResult{
                connectedCubes = new List<Cube>(connectedCubes),
                direction = MatchDirection.LongVertical
            };
        }else{
            return new MatchResult{
                connectedCubes = connectedCubes,
                direction = MatchDirection.None
            };
        }
    }

    void CheckDirection(Cube cub, Vector2Int direction, List<Cube> connectedCubes){
        // DÜZELTME: Cube.CubeType olarak erişildi
        Cube.CubeType cubeType = cub.cubeType; 
        int x = cub.xIndex + direction.x;
        int y = cub.yIndex + direction.y;

        while(x >= 0 && x < width && y >= 0 && y < height){
            if(cubeBoard[x,y].isUsable && cubeBoard[x,y].cube != null){
                Cube neighbourCube = cubeBoard[x,y].cube.GetComponent<Cube>();
                if(!neighbourCube.isMatched && neighbourCube.cubeType == cubeType){
                    connectedCubes.Add(neighbourCube);
                    x += direction.x;
                    y += direction.y;
                }else{
                    break;
                }
            }else{
                break;
            }
        }
    }
}

public class MatchResult{
    public List<Cube> connectedCubes;
    public MatchDirection direction;
}

public enum MatchDirection{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}
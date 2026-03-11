using UnityEngine;

public class Node
{
    public bool isUsable;
    public GameObject cube;

    public Node(bool isUsable, GameObject cube)
    {
        this.isUsable = isUsable;
        this.cube = cube;
    }
}
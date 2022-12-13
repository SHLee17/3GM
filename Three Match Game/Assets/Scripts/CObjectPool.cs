using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CObjectPool : CSingleton<CObjectPool>
{
    float timer;
    public float leftSync;
    public float rightSync;
    public List<CBoard> destructionList;

    public CTile cTile;
    public Queue<CTile> cTileQueue;
    public CSpriteManager SOspriteManger;

    void Start()
    {
        destructionList = new List<CBoard>();
        cTileQueue = new Queue<CTile>();
        for (int i = 0; i < CMapManager.Instance.cBoardList.Count; i++)
        {
            CTile temp = Instantiate(cTile.transform.GetComponent<CTile>(),
                transform.position, Quaternion.identity, transform);
            cTileQueue.Enqueue(temp);
            temp.name = "Tile";
            temp.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        timer += Time.deltaTime * 3f;

        leftSync = 0.1f * Mathf.Cos(timer) + -0.9f;
        rightSync = 0.1f * -Mathf.Cos(timer) + 0.9f;
    }
    public void InsertTile(CTile tile)
    {
        tile.speed = 20f;
        tile.transform.SetParent(transform);
        cTileQueue.Enqueue(tile);
        tile.eTileType = ETileType.NOMAL;
        tile.eDirection = (EDirection)Random.Range(0, (int)EDirection.END);
        tile.gameObject.SetActive(false);
    }
    public CTile GetTile()
    {
        CTile tile = cTileQueue.Dequeue();
        tile.gameObject.SetActive(true);
        return tile;
    }
    public bool DuplicateList(CBoard board)
    {
        foreach (var item in destructionList)
        {
            if(item == board)
            return true;
        }
        return false;
    }
}

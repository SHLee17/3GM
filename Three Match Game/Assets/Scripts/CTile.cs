using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public enum ETileType
{
    NOMAL,
    STRAIGHT,
}

[System.Serializable]

public class CTile : MonoBehaviour
{
    public SpriteRenderer sprRenderer;

    public EDirection eDirection;
    public ETileType eTileType;
    public ETileColor eTileColor;
    public bool isDestroyTile;
    public bool isNew;
    public GameObject objArrow;
    public SortingGroup arrowSortingGroup;
    public CObstruction cObstruction;
    public float speed;
    public EObstructionType eObstructionType
    {
        get { return cObstruction.eObstructionType; }
        set { cObstruction.eObstructionType = value; }
    }
private void Start()
    {
        arrowSortingGroup = objArrow.GetComponent<SortingGroup>();
        speed = 20f;
        isDestroyTile = false;
        isNew = false;
        if (eTileType == ETileType.STRAIGHT)
            eDirection = (EDirection)Random.Range(0, (int)EDirection.END);

        TileInit();


    }
    private void Update()
    {
        if (isDestroyTile)
            StartCoroutine(MoveTile());
    }

    public IEnumerator TileColorChange()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        switch (eTileColor)
        {
            case ETileColor.BLUE:
                sprRenderer.sprite = CObjectPool.Instance.SOspriteManger.sprNormalTileList[0];
                break;
            case ETileColor.GREEN:
                sprRenderer.sprite = CObjectPool.Instance.SOspriteManger.sprNormalTileList[1];
                break;
            case ETileColor.PURPLE:
                sprRenderer.sprite = CObjectPool.Instance.SOspriteManger.sprNormalTileList[2];
                break;
            case ETileColor.YELLOW:
                sprRenderer.sprite = CObjectPool.Instance.SOspriteManger.sprNormalTileList[3];
                break;
            case ETileColor.RED:
                sprRenderer.sprite = CObjectPool.Instance.SOspriteManger.sprNormalTileList[4];
                break;
            case ETileColor.ORANGE:
                sprRenderer.sprite = CObjectPool.Instance.SOspriteManger.sprNormalTileList[5];
                break;

        }
        gameObject.SetActive(true);

    }

    public void TileInit()
    {
        if (eTileType == ETileType.NOMAL)
        {
            objArrow.SetActive(false);
            sprRenderer.sprite = CObjectPool.Instance.SOspriteManger.sprNormalTileList[(int)eTileColor];
        }
        else if (eTileType == ETileType.STRAIGHT)
        {
            TileChangeType(eTileType);
        }

    }
    public void TileChangeType(ETileType type , EDirection direction = EDirection.END)
    {
        if (direction == EDirection.END)
            eDirection = (EDirection)Random.Range(0, (int)EDirection.END);
        else
            eDirection = direction;

        if (type == ETileType.STRAIGHT)
        {
            if(eTileType != ETileType.STRAIGHT)
            eTileType = ETileType.STRAIGHT;

            objArrow.SetActive(true);

            if (eDirection == EDirection.UP_R || eDirection == EDirection.DOWN_L)
                objArrow.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 30));

            else if (eDirection == EDirection.UP_L || eDirection == EDirection.DOWN_R)
                objArrow.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -30));

            else if (eDirection == EDirection.DOWN || eDirection == EDirection.UP)
                objArrow.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));

            sprRenderer.sprite = CObjectPool.Instance.SOspriteManger.sprSpecialTileList[(int)eTileColor];
        }
    }

    IEnumerator MoveTile()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        if (Vector3.Distance(transform.localPosition, Vector3.zero) != 0)
        {
            float speed = Time.deltaTime * 80f;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, speed);
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.1f);
            //foreach (var item in CObjectiveUI.Instance.cObjectiveList)
            //{
            //    if (item.eTileColor == eTileColor) 
            //    {
            //       if (item.count > 0)
            //            item.count -= 1;
            //    }
            //}
            isDestroyTile = false;
        }
    }

    public bool MovePossible()
    {
        if (cObstruction.gameObject.activeSelf)
        {
            switch (cObstruction.eObstructionType)
            {
                case EObstructionType.ICE_BOX:
                    return cObstruction.isMatchPossible;
            }
        }
        return true;
    }
    public bool MatchPossible()
    {
        if (cObstruction.gameObject.activeSelf)
        {
            switch (cObstruction.eObstructionType)
            {
                case EObstructionType.ICE_BOX:
                    return cObstruction.isMatchPossible;
            }
        }
        return true;
    }

}

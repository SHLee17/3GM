using UnityEngine;

public class CArrowMove : MonoBehaviour
{
    public GameObject objLeft;
    public GameObject objRight;
    Vector3 leftPos;
    Vector3 rightPos;


    void Update()
    {
        if (gameObject.activeSelf)
        {
            objLeft.transform.localPosition = new Vector3(CObjectPool.Instance.leftSync, 0, 0);
            objRight.transform.localPosition = new Vector3(CObjectPool.Instance.rightSync, 0, 0);
        }
    }

}

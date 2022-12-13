using UnityEngine;

public class CSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instacne = default(T);
    public static T Instance
    {
        get
        {
            //싱글톤이 없다. 
            if (_instacne == null)
            {
                //그러면 싱글톤을 찾아온다.
                _instacne = FindObjectOfType(typeof(T)) as T;
            }

            //찾았지만 싱글톤이 없다.
            if (_instacne == null)
            {
                //새로운 오브젝트를 만들어서 거기다 싱글톤을 넣어서 만든다.
                var gameObject = new GameObject(typeof(T).ToString());
                _instacne = gameObject.AddComponent<T>();
                //DontDestroyOnLoad(gameObject);
            }
            //그리고 반환한다.
            return _instacne;
        }
    }
    //여기도 중요!
    private void Awake()
    {
        _instacne = Create();
        //DontDestroyOnLoad(gameObject);
    }

    public static T Create()
    {
        return _instacne;
    }

}
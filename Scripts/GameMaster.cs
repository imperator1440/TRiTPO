using UnityEngine;

public class GameMaster : MonoBehaviour
{
    private static GameMaster instance; // Wrong naming ('m_'), private field must start with "_"
    public Vector2 m_lastCheckPoint; // Wrong naming ('m_'), public field must start with uppercase letter and without prefix.

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManage : MonoBehaviour
{
    public static SceneManage i;
    
    public void Awake()
    {
        i = this;
    }
    
    public void LoadRound1()
    {
        SceneManager.LoadScene("Round1_ForestScene");
    }

    public void LoadRound2()
    {
        SceneManager.LoadScene("Round2_Winter");
    }

    public void LoadRound3()
    {
        SceneManager.LoadScene("Round3_Desert");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

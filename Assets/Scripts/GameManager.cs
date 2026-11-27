using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _i;
    [Header("GameObject - Panel")]
    public GameObject PauseGamePanel;
    public GameObject NotificationPanel;
    public GameObject PlayerSkillsPanel;
    public GameObject TreasurePanel;
    public GameObject InventoryPanel;
    public GameObject CalendarPanel;
    public GameObject[] CheckinImage;
    [Header("Check_bool_variables")]
    public bool isPause;
    public bool isHome;
    public bool isPlay;
    public bool isPlayerSkillsActive;
    public bool isTreasureActive;
    public bool isInventoryActive;
    public bool isRound1;
    public bool isRound2;
    public bool isRound3;
    public bool isCalendarActive;
    public bool isCheckin;

    private int selectedIndex = -1;


    public void Awake()
    {
        _i = this;
    }

    public void PauseGame()
    {
        isPause = true;
    }

    public void ResumeGame()
    {
        isPause  = false;
    }

    public void BackHome()
    {
        isHome = true;
    }

    public  void Play()
    {
        isPlay = true;
    }
   
    public void PlayerSkillsActive()
    {
        isPlayerSkillsActive = true;
    }

    public void PlayerSkillsOff()
    {
        isPlayerSkillsActive = false;
    }

    public void TreasurePanelActive()
    {
        isTreasureActive = true;
    }

    public void TreasurePanelOff()
    {
        isTreasureActive  = false;
    }

    public void InventoryPanelActive()
    {
        isInventoryActive = true;
    }

    public void InventoryPanelOff()
    {
        isInventoryActive = false;
    }

    public void CalendarPanelActive()
    {
        isCalendarActive = true;
    }

    public void CalendarPanelOff()
    {
        isCalendarActive  = false;
    }

    public void CheckinActive(int dayIndex)
    {
        if (dayIndex < 0 || dayIndex >= CheckinImage.Length)
        return;
        isCheckin = true;
        selectedIndex  = dayIndex;
    }

    public void SceneChange()
    {
        if(isRound1)
        {
            SceneManage.i.LoadRound1();
            isRound1 = false;
        }

        if(isRound2)
        {
            SceneManage.i.LoadRound2();
            isRound2 = false;
        }

        if(isRound3)
        {
            SceneManage.i.LoadRound3();
            isRound3 = false;
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isPause) PauseGamePanel.SetActive(true);
        else PauseGamePanel.SetActive(false);

        if(isHome)
        {
            NotificationPanel.SetActive(true);
            isHome  = false;
            isPause = false;
        }

        if(isPlay)
        {
            NotificationPanel.SetActive(false);
            isPlay = false;
        }

        if(isPlayerSkillsActive) PlayerSkillsPanel.SetActive(true);
        else PlayerSkillsPanel.SetActive(false);

        if(isTreasureActive) TreasurePanel.SetActive(true);
        else TreasurePanel.SetActive(false);
        
        if(isInventoryActive)  InventoryPanel.SetActive(true);
        else InventoryPanel.SetActive(false);

        if(isCalendarActive) CalendarPanel.SetActive(true);
        else CalendarPanel.SetActive(false);
        
        if(isCheckin)
        {
            CheckinImage[selectedIndex].SetActive(true);
            isCheckin = false;
        }
       
    }
}

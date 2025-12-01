using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScreenUI : MonoBehaviour
{
    [SerializeField] UIManager UIMgr; //Initialized in inspector
    [SerializeField] private bool start = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(UIMgr.IsPanelOpen("StartPanel")) {
            if(SceneManager.GetActiveScene().name != "StartScene") {
                UIMgr.ClosePanel("StartPanel");
            }
            else if(!start) {
                UIMgr.ClosePanel("StartPanel");
            }
        }
        else if(SceneManager.GetActiveScene().name == "StartScene" && start) {
            UIMgr.OpenPanel("StartPanel");
        }
        if(UIMgr.IsPanelOpen("EndPanel")) {
            if(SceneManager.GetActiveScene().name != "StartScene") {
                UIMgr.ClosePanel("EndPanel");
            }
            else if(start) {
                UIMgr.ClosePanel("EndPanel");
            }
        }
        else if(SceneManager.GetActiveScene().name == "StartScene" && !start) {
            UIMgr.OpenPanel("EndPanel");
        }
    }

    public void OnStartButtonClick() 
    {
        Debug.Log("Clicks start button");
        UIMgr.ClosePanel("StartPanel");
        this.start = false;
        SceneManager.LoadScene("Level1");
    }

    public void OnEndButtonClick() 
    {
        Debug.Log("Clicks end button");
        UIMgr.ClosePanel("EndPanel");
        this.start = true;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScreenUI : MonoBehaviour
{
    [SerializeField] UIManager UIMgr; //Initialized in inspector
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(SceneManager.GetActiveScene().name == "StartScene" && !UIMgr.IsPanelOpen("StartPanel"))
        {
            UIMgr.OpenPanel("StartPanel");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnStartButtonClick() 
    {
        UIMgr.ClosePanel("StartPanel");
        SceneManager.LoadScene("Level1");
    }
}

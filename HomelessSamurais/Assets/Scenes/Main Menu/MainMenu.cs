using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainPanel;
    public GameObject hostJoinPanel;
    public GameObject settingsPanel;
    
    [Header("Host/Join Elements")]
    public TMP_InputField ipAddressInput;
    public TMP_Text statusText;
    
    [Header("Settings")]
    public string gameSceneName = "Arena";
    public string defaultIP = "127.0.0.1";
    
    private string ipToJoin = "127.0.0.1";
    
    void Start()
    {
        ShowMainPanel();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (ipAddressInput != null)
        {
            ipToJoin = PlayerPrefs.GetString("LastUsedIP", defaultIP);
            ipAddressInput.text = ipToJoin;
        }
        
        // DEBUG: Print panel positions
        DebugPanelInfo();
    }
    
    void DebugPanelInfo()
    {
        if (mainPanel != null)
        {
            RectTransform rect = mainPanel.GetComponent<RectTransform>();
            Debug.Log($"MainPanel - Active: {mainPanel.activeSelf}, Position: {rect.anchoredPosition}, Size: {rect.sizeDelta}");
        }
        
        if (hostJoinPanel != null)
        {
            RectTransform rect = hostJoinPanel.GetComponent<RectTransform>();
            Debug.Log($"HostJoinPanel - Active: {hostJoinPanel.activeSelf}, Position: {rect.anchoredPosition}, Size: {rect.sizeDelta}");
        }
    }
    
    void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (hostJoinPanel != null) hostJoinPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        Debug.Log("=== MAIN PANEL SHOWN ===");
        DebugPanelInfo();
    }
    
    void ShowHostJoinPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (hostJoinPanel != null) hostJoinPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        Debug.Log("=== HOST/JOIN PANEL SHOWN ===");
        DebugPanelInfo();
    }
    
    // Button callbacks
    public void OnPlayButton()
    {
        Debug.Log("Play button clicked!");
        ShowHostJoinPanel();
    }
    
    public void OnHostButton()
    {
        PlayerPrefs.SetString("NetworkMode", "Host");
        PlayerPrefs.Save();
        
        LoadGameScene();
    }
    
    public void OnJoinButton()
    {
        if (ipAddressInput != null)
        {
            ipToJoin = ipAddressInput.text;
        }
        
        PlayerPrefs.SetString("LastUsedIP", ipToJoin);
        PlayerPrefs.SetString("NetworkMode", "Client");
        PlayerPrefs.SetString("ServerIP", ipToJoin);
        PlayerPrefs.Save();
        
        LoadGameScene();
    }
    
    public void OnBackButton()
    {
        Debug.Log("Back button clicked!");
        ShowMainPanel();
    }
    
    public void OnSettingsButton()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
    
    public void OnQuitButton()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    void LoadGameScene()
    {
        Debug.Log($"=== LOADING SCENE: {gameSceneName} ===");
        SceneManager.LoadScene(gameSceneName);
    }
}
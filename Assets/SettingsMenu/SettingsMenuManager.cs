using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("Настройки кнопок")]
    [SerializeField] private Button soundButton;
    [SerializeField] private Button exitButton;
    
    [Header("Настройки сцены")]
    [SerializeField] private string sceneToLoad = "GameScene";
    [Tooltip("Задержка перед загрузкой сцены в секундах")]
    [SerializeField] private float loadDelay = 0.3f;

    private void Start()
    {
        // Проверяем, что кнопки назначены
        if (soundButton != null)
        {
            soundButton.onClick.AddListener(LoadGameScene);
        }
        else
        {
            Debug.LogError("Кнопка Play не назначена!");
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
        else
        {
            Debug.LogError("Кнопка Exit не назначена!");
        }
    }

    /// <summary>
    /// Загружает игровую сцену
    /// </summary>
    private void LoadGameScene()
    {
        // Можно добавить звук нажатия или анимацию
        Debug.Log($"Загрузка сцены: {sceneToLoad}");
        
        // Небольшая задержка для плавности (опционально)
        if (loadDelay > 0)
        {
            Invoke(nameof(LoadSceneWithDelay), loadDelay);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void LoadSceneWithDelay()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    /// <summary>
    /// Выход из игры
    /// </summary>
    private void ExitGame()
    {
        Debug.Log("Выход из игры...");
        
        // Можно добавить подтверждение выхода
        if (loadDelay > 0)
        {
            Invoke(nameof(ExitWithDelay), loadDelay);
        }
        else
        {
            QuitApplication();
        }
    }

    private void ExitWithDelay()
    {
        QuitApplication();
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        // В редакторе Unity останавливаем воспроизведение
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // В собранной версии закрываем приложение
        Application.Quit();
#endif
    }

    /// <summary>
    /// Методы для вызова из других скриптов (опционально)
    /// </summary>
    public void LoadScene(string sceneName)
    {
        sceneToLoad = sceneName;
        LoadGameScene();
    }
    
    public void Quit()
    {
        ExitGame();
    }
}
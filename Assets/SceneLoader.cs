using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [System.Serializable]
    public class SceneButton
    {
        public Button button;
        public string sceneName;
        public LoadSceneMode loadMode = LoadSceneMode.Single;
    }

    [Header("Настройки сцен")]
    [SerializeField] private SceneButton[] sceneButtons;

    [Header("Настройки загрузки")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingProgressBar;
    [SerializeField] private Text progressText;

    private void Start()
    {
        // Назначаем обработчики для всех кнопок
        foreach (var sceneButton in sceneButtons)
        {
            if (sceneButton.button != null && !string.IsNullOrEmpty(sceneButton.sceneName))
            {
                // Сохраняем параметры в локальные переменные для замыкания
                string sceneName = sceneButton.sceneName;
                LoadSceneMode loadMode = sceneButton.loadMode;
                
                sceneButton.button.onClick.AddListener(() => LoadScene(sceneName, loadMode));
            }
            else
            {
                Debug.LogWarning("Кнопка или имя сцены не назначены!");
            }
        }
    }

    /// <summary>
    /// Загружает сцену по имени
    /// </summary>
    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Имя сцены не указано!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Сцена '{sceneName}' не найдена в Build Settings!");
            return;
        }

        if (loadingScreen != null)
        {
            StartCoroutine(LoadSceneAsync(sceneName, mode));
        }
        else
        {
            SceneManager.LoadScene(sceneName, mode);
        }
    }

    /// <summary>
    /// Загружает следующую сцену по индексу
    /// </summary>
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            LoadScene(SceneManager.GetSceneByBuildIndex(nextSceneIndex).name);
        }
        else
        {
            Debug.LogWarning("Следующая сцена не найдена!");
        }
    }

    /// <summary>
    /// Загружает предыдущую сцену
    /// </summary>
    public void LoadPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = currentSceneIndex - 1;

        if (previousSceneIndex >= 0)
        {
            LoadScene(SceneManager.GetSceneByBuildIndex(previousSceneIndex).name);
        }
        else
        {
            Debug.LogWarning("Предыдущая сцена не найдена!");
        }
    }

    /// <summary>
    /// Перезагружает текущую сцену
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        LoadScene(currentSceneName);
    }

    /// <summary>
    /// Выход из игры
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Асинхронная загрузка сцены с экраном загрузки
    /// </summary>
    private System.Collections.IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode mode)
    {
        // Показываем экран загрузки
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // Начинаем асинхронную загрузку
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // Прогресс загрузки от 0 до 0.9
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // Обновляем UI
            if (loadingProgressBar != null)
                loadingProgressBar.value = progress;

            if (progressText != null)
                progressText.text = $"{(progress * 100):0}%";

            // Когда загрузка почти завершена, разрешаем активацию сцены
            if (operation.progress >= 0.9f)
            {
                // Можно добавить задержку или ожидание ввода пользователя
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Добавляет сцену аддитивно
    /// </summary>
    public void LoadSceneAdditive(string sceneName)
    {
        LoadScene(sceneName, LoadSceneMode.Additive);
    }

    /// <summary>
    /// Выгружает сцену по имени
    /// </summary>
    public void UnloadScene(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}
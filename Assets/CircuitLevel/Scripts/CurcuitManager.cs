using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircuitManager : MonoBehaviour
{
    [Header("Целевые параметры уровня")]
    public float targetVoltage = 12f;
    public float targetCurrent = 2.5f;
    
    [Header("Текущие значения")]
    [SerializeField] private float currentVoltage;
    [SerializeField] private float currentCurrent;
    [SerializeField] private float currentResistance;

    [Header("UI элементы управления")]
    public Slider voltageSlider;
    public Slider currentSlider;
    public Slider resistanceSlider;
    public TMP_Text voltageText;
    public TMP_Text currentText;
    public TMP_Text resistanceText;
    public TMP_Text taskText;
    public TMP_Text resultText;
    public Button startButton;
    public Button resetButton;
    public Button hintButton;

    [Header("UI изображения элементов цепи")]
    public Image lampImage;
    public Image switchImage;
    public Image batteryImage;
    public Image fanBaseImage;
    public Image fanBladesImage;
    public Image[] wireImages;

    [Header("Спрайты состояний")]
    public Sprite lampOffSprite;
    public Sprite lampOnSprite;
    public Sprite lampHalfSprite;
    public Sprite switchOffSprite;
    public Sprite switchOnSprite;
    public Sprite batteryNormalSprite;
    public Sprite batteryLowSprite;
    public Sprite fanBladesSprite;

    [Header("Визуальные настройки")]
    public Color wireActiveColor = new Color(1f, 0.8f, 0f, 1f);
    public Color wireInactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    public float fanBaseRotationSpeed = 300f;
    [Range(0.1f, 2f)] public float fanSpeedMultiplier = 1f;

    [Header("Настройки уровня")]
    [Range(0.05f, 0.5f)] public float successTolerance = 0.15f;
    public bool enableHints = true;

    [Header("Анимационные настройки")]
    public float elementPulseSpeed = 2f;
    public float elementPulseAmount = 0.1f;

    // Внутренние переменные
    private bool isCircuitActive = false;
    private bool isSuccess = false;
    private float fanRotationSpeed = 0f;
    private float currentIntensity = 0f;
    private Vector3[] originalScales;
    private Coroutine successCoroutine;

    void Start()
    {
        InitializeUI();
        InitializeCircuitImages();
        SetupEventListeners();
    }

    void InitializeUI()
    {
        taskText.text = $"<b>Цель цепи:</b>\nНапряжение = {targetVoltage} В\nСила тока = {targetCurrent} А";
        resultText.text = "";
        
        // Настройка ползунков с разумными диапазонами
        voltageSlider.minValue = 1f;
        voltageSlider.maxValue = 24f;
        voltageSlider.value = 12f;
        
        currentSlider.minValue = 0.1f;
        currentSlider.maxValue = 5f;
        currentSlider.value = 2f;
        
        resistanceSlider.minValue = 0.5f;
        resistanceSlider.maxValue = 50f;
        resistanceSlider.value = 6f;
        
        UpdateUIText();
        UpdateCircuitValues();
        
        // Настройка кнопок
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetCircuit);
        
        if (hintButton != null)
            hintButton.onClick.AddListener(ShowFormulaHint);
    }

    void InitializeCircuitImages()
    {
        // Устанавливаем начальные спрайты
        if (lampImage != null && lampOffSprite != null)
            lampImage.sprite = lampOffSprite;
        
        if (switchImage != null && switchOffSprite != null)
            switchImage.sprite = switchOffSprite;
        
        if (batteryImage != null && batteryNormalSprite != null)
            batteryImage.sprite = batteryNormalSprite;
        
        if (fanBladesImage != null && fanBladesSprite != null)
            fanBladesImage.sprite = fanBladesSprite;
        
        // Инициализируем провода
        UpdateWiresColor(wireInactiveColor);
        
        // Выключаем вращение
        fanRotationSpeed = 0f;
    }

    void SetupEventListeners()
    {
        voltageSlider.onValueChanged.AddListener(OnVoltageChanged);
        currentSlider.onValueChanged.AddListener(OnCurrentChanged);
        resistanceSlider.onValueChanged.AddListener(OnResistanceChanged);
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    void OnVoltageChanged(float value)
    {
        currentVoltage = value;
        if (!isCircuitActive)
        {
            RecalculateCurrent();
            currentSlider.SetValueWithoutNotify(currentCurrent);
        }
        UpdateUIText();
        
        // Визуальная обратная связь при изменении
        if (batteryImage != null)
        {
            float voltageRatio = currentVoltage / targetVoltage;
            batteryImage.color = Color.Lerp(Color.red, Color.white, voltageRatio);
        }
    }

    void OnCurrentChanged(float value)
    {
        currentCurrent = value;
        if (!isCircuitActive)
        {
            RecalculateResistance();
            resistanceSlider.SetValueWithoutNotify(currentResistance);
        }
        UpdateUIText();
    }

    void OnResistanceChanged(float value)
    {
        currentResistance = value;
        if (!isCircuitActive)
        {
            RecalculateCurrent();
            currentSlider.SetValueWithoutNotify(currentCurrent);
        }
        UpdateUIText();
    }

    void RecalculateCurrent()
    {
        if (currentResistance > 0.01f) // Защита от деления на 0
            currentCurrent = currentVoltage / currentResistance;
        else
            currentCurrent = currentSlider.maxValue;
        
        currentCurrent = Mathf.Clamp(currentCurrent, currentSlider.minValue, currentSlider.maxValue);
    }

    void RecalculateResistance()
    {
        if (currentCurrent > 0.01f)
            currentResistance = currentVoltage / currentCurrent;
        else
            currentResistance = resistanceSlider.maxValue;
        
        currentResistance = Mathf.Clamp(currentResistance, resistanceSlider.minValue, resistanceSlider.maxValue);
    }

    void UpdateUIText()
    {
        voltageText.text = $"<b>Напряжение:</b> {currentVoltage:F2} В";
        currentText.text = $"<b>Сила тока:</b> {currentCurrent:F2} А";
        resistanceText.text = $"<b>Сопротивление:</b> {currentResistance:F2} Ом";
    }

    void UpdateCircuitValues()
    {
        currentVoltage = voltageSlider.value;
        currentCurrent = currentSlider.value;
        currentResistance = resistanceSlider.value;
    }

    void OnStartButtonClicked()
    {
        if (isCircuitActive)
        {
            ResetCircuit();
        }
        else
        {
            StartCircuitSimulation();
        }
    }

    void StartCircuitSimulation()
    {
        isCircuitActive = true;
        startButton.GetComponentInChildren<TMP_Text>().text = "ОСТАНОВИТЬ";
        
        // Обновляем значения
        UpdateCircuitValues();
        
        // Активируем выключатель
        if (switchImage != null && switchOnSprite != null)
        {
            switchImage.sprite = switchOnSprite;
            StartElementPulse(switchImage);
        }
        
        // Включаем провода
        UpdateWiresColor(wireActiveColor);
        
        // Рассчитываем реальный ток
        float realCurrent = currentVoltage / currentResistance;
        
        // Настраиваем визуальные эффекты
        currentIntensity = Mathf.Clamp(realCurrent / targetCurrent, 0.1f, 2f);
        UpdateCircuitVisuals(realCurrent);
        
        // Настраиваем вентилятор
        fanRotationSpeed = realCurrent * fanBaseRotationSpeed * fanSpeedMultiplier;
        
        // Проверяем успех
        CheckSuccess(realCurrent);
    }

    void UpdateCircuitVisuals(float realCurrent)
    {
        // Лампочка
        if (lampImage != null)
        {
            if (realCurrent <= 0.1f)
            {
                lampImage.sprite = lampOffSprite;
            }
            else if (realCurrent < targetCurrent * 0.7f && lampHalfSprite != null)
            {
                lampImage.sprite = lampHalfSprite;
            }
            else
            {
                lampImage.sprite = lampOnSprite;
            }
            
            // Прозрачность и пульсация
            float alpha = Mathf.Clamp(currentIntensity * 0.8f, 0.3f, 1f);
            Color lampColor = lampImage.color;
            lampColor.a = alpha;
            lampImage.color = lampColor;
            
            StartElementPulse(lampImage);
        }
        
        // Батарея (меняем спрайт при низком напряжении)
        if (batteryImage != null)
        {
            if (currentVoltage < targetVoltage * 0.5f && batteryLowSprite != null)
            {
                batteryImage.sprite = batteryLowSprite;
            }
            
            // Эффект разряда
            float batteryEffect = Mathf.Sin(Time.time * 2f) * 0.1f + 0.9f;
            batteryImage.color = new Color(batteryEffect, batteryEffect, batteryEffect, 1f);
        }
    }

    void StartElementPulse(Image element)
    {
        // Запускаем корутину пульсации
        if (element != null)
        {
            StartCoroutine(PulseElement(element));
        }
    }

    System.Collections.IEnumerator PulseElement(Image element)
    {
        Vector3 originalScale = element.rectTransform.localScale;
        float pulseTime = 0f;
        
        while (isCircuitActive && element != null)
        {
            pulseTime += Time.deltaTime * elementPulseSpeed;
            float pulse = Mathf.Sin(pulseTime) * elementPulseAmount;
            element.rectTransform.localScale = originalScale * (1f + pulse);
            yield return null;
        }
        
        // Возвращаем оригинальный масштаб
        if (element != null)
            element.rectTransform.localScale = originalScale;
    }

    void CheckSuccess(float realCurrent)
    {
        float difference = Mathf.Abs(realCurrent - targetCurrent);
        isSuccess = difference <= successTolerance;
        
        if (isSuccess)
        {
            resultText.text = "<color=#4CAF50><b>✓ УСПЕХ!</b> Цепь настроена правильно!</color>";
            
            // Эффект успеха
            fanRotationSpeed *= 1.3f; // Ускоряем вентилятор
            
            // Запускаем анимацию успеха
            if (successCoroutine != null)
                StopCoroutine(successCoroutine);
            successCoroutine = StartCoroutine(SuccessAnimation());
        }
        else
        {
            string direction = realCurrent < targetCurrent ? "увеличьте" : "уменьшите";
            resultText.text = $"<color=#F44336><b>✗ НЕВЕРНО</b>\nТок: {realCurrent:F2} А ({direction} силу тока)</color>";
        }
    }

    System.Collections.IEnumerator SuccessAnimation()
    {
        float duration = 2f;
        float elapsed = 0f;
        Image[] allCircuitImages = { lampImage, switchImage, batteryImage, fanBaseImage };
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Мигание элементов
            float blink = Mathf.Sin(t * Mathf.PI * 4f) * 0.5f + 0.5f;
            
            foreach (Image img in allCircuitImages)
            {
                if (img != null)
                {
                    Color c = img.color;
                    c.g = Mathf.Lerp(c.g, 1f, blink);
                    c.b = Mathf.Lerp(c.b, 1f, blink);
                    img.color = c;
                }
            }
            
            yield return null;
        }
        
        // Возвращаем цвета
        foreach (Image img in allCircuitImages)
        {
            if (img != null)
                img.color = Color.white;
        }
    }

    void ResetCircuit()
    {
        isCircuitActive = false;
        isSuccess = false;
        
        startButton.GetComponentInChildren<TMP_Text>().text = "ЗАПУСТИТЬ ЦЕПЬ";
        resultText.text = "";
        
        // Возвращаем выключатель
        if (switchImage != null && switchOffSprite != null)
        {
            switchImage.sprite = switchOffSprite;
            switchImage.color = Color.white;
        }
        
        // Выключаем лампочку
        if (lampImage != null && lampOffSprite != null)
        {
            lampImage.sprite = lampOffSprite;
            lampImage.color = Color.white;
        }
        
        // Возвращаем батарею
        if (batteryImage != null && batteryNormalSprite != null)
        {
            batteryImage.sprite = batteryNormalSprite;
            batteryImage.color = Color.white;
        }
        
        // Выключаем провода
        UpdateWiresColor(wireInactiveColor);
        
        // Останавливаем вентилятор
        fanRotationSpeed = 0f;
        currentIntensity = 0f;
        
        // Останавливаем все корутины
        if (successCoroutine != null)
        {
            StopCoroutine(successCoroutine);
            successCoroutine = null;
        }
        
        // Возвращаем масштабы
        Image[] allImages = GetComponentsInChildren<Image>();
        for (int i = 0; i < Mathf.Min(allImages.Length, originalScales.Length); i++)
        {
            allImages[i].rectTransform.localScale = originalScales[i];
        }
    }

    void UpdateWiresColor(Color color)
    {
        if (wireImages == null) return;
        
        foreach (Image wire in wireImages)
        {
            if (wire != null)
            {
                wire.color = Color.Lerp(wire.color, color, Time.deltaTime * 5f);
            }
        }
    }

    void ShowFormulaHint()
    {
        if (!enableHints) return;
        
        string hint = "<color=#FFC107><b>ПОДСКАЗКА</b>\nЗакон Ома: I = U / R\n";
        hint += $"Где:\n• I - сила тока (А)\n• U - напряжение (В)\n• R - сопротивление (Ом)\n\n";
        hint += $"Для цели: R = {targetVoltage} / {targetCurrent} = {targetVoltage/targetCurrent:F1} Ом</color>";
        
        resultText.text = hint;
        
        // Автоочистка через 5 секунд
        if (isCircuitActive)
            Invoke("ClearHint", 5f);
    }

    void ClearHint()
    {
        if (!isCircuitActive)
            resultText.text = "";
    }

    void Update()
    {
        // Вращение лопастей вентилятора
        if (fanBladesImage != null && fanRotationSpeed > 0)
        {
            fanBladesImage.rectTransform.Rotate(0, 0, fanRotationSpeed * Time.deltaTime);
        }
        
        // Динамическое обновление цвета проводов
        if (isCircuitActive && wireImages != null)
        {
            float pulse = Mathf.Sin(Time.time * 2f) * 0.1f + 0.9f;
            Color activeColor = wireActiveColor * pulse;
            activeColor.a = 1f;
            
            foreach (Image wire in wireImages)
            {
                if (wire != null)
                    wire.color = Color.Lerp(wire.color, activeColor, Time.deltaTime * 3f);
            }
        }
        
        // Обновление прозрачности лампы
        if (isCircuitActive && lampImage != null)
        {
            float targetAlpha = Mathf.Clamp(currentIntensity * 0.8f, 0.3f, 1f);
            Color lampColor = lampImage.color;
            lampColor.a = Mathf.Lerp(lampColor.a, targetAlpha, Time.deltaTime * 2f);
            lampImage.color = lampColor;
        }
    }

    // Публичные методы для дополнительного UI
    public void CalculateOptimalResistance()
    {
        // Рассчитать оптимальное сопротивление для текущего напряжения
        float optimalResistance = targetVoltage / targetCurrent;
        resistanceSlider.value = optimalResistance;
        OnResistanceChanged(optimalResistance);
        
        resultText.text = $"<color=#2196F3>Оптимальное сопротивление: {optimalResistance:F2} Ом</color>";
        Invoke("ClearHint", 3f);
    }

    public void ToggleRealTimeUpdate(bool enabled)
    {
        // Включает/выключает пересчет в реальном времени
        if (enabled)
        {
            RecalculateCurrent();
            currentSlider.value = currentCurrent;
            UpdateUIText();
        }
    }
}
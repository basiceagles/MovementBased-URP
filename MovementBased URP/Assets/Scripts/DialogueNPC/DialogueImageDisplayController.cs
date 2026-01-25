using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Reflection;

public class DialogueImageDisplayController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject imagePanel;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI clickToContinueText;

    [Header("Image Settings")]
    [SerializeField] private float maxZoom = 3f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomStep = 0.1f;
    [SerializeField] private float minZoom = 0.5f;

    [Header("Drag Settings")]
    [SerializeField] private float dragSensitivity = 1f;
    [SerializeField] private bool enableDrag = true;
    [SerializeField] private float dragBoundary = 0.2f; // В процентах от размера экрана (0-1)
    [SerializeField] private bool smoothDrag = true;
    [SerializeField] private float dragSmoothness = 10f;

    [Header("Dialogue System")]
    [SerializeField] private DialoguesSystem dialogueSystem;
    [SerializeField] private bool showClickToContinueText = true;

    [System.Serializable]
    public class DialogueImageData
    {
        public string npcId;
        public ImagePerLine[] imagesPerLine;
    }

    [System.Serializable]
    public class ImagePerLine
    {
        public int lineIndex;
        public Texture2D russianImage;
        public Texture2D englishImage;
    }

    [SerializeField] private DialogueImageData[] dialogueImages;

    private RectTransform imageRectTransform;
    private Vector2 originalImageSize;
    private Vector2 originalImagePosition;
    private float currentZoom = 1f;
    private bool isImageModeActive = false;
    private bool waitingForImageClick = false;
    private bool imageClickProcessedThisFrame = false; // Флаг для обработки клика в текущем кадре

    // Перетаскивание
    private bool isDragging = false;
    private Vector2 dragStartMousePosition;
    private Vector2 dragStartImagePosition;
    private Vector2 targetImagePosition;
    private Vector2 currentVelocity; // Для плавного движения

    // Кэшированные поля рефлексии
    private FieldInfo currentDialogueIndexField;
    private FieldInfo currentLineIndexField;
    private FieldInfo dialoguesField;
    private FieldInfo isTypingField;
    private PropertyInfo dialoguesProperty;

    void Start()
    {
        InitializeUI();
        CacheReflectionFields();
    }

    void Update()
    {
        imageClickProcessedThisFrame = false; // Сбрасываем флаг в начале кадра

        if (isImageModeActive)
        {
            HandleImageMode();

            // Плавное движение к целевой позиции
            if (smoothDrag && !isDragging)
            {
                imageRectTransform.anchoredPosition = Vector2.SmoothDamp(
                    imageRectTransform.anchoredPosition,
                    targetImagePosition,
                    ref currentVelocity,
                    1f / dragSmoothness
                );
            }
        }
        else if (waitingForImageClick && Input.GetMouseButtonDown(0))
        {
            ShowImageForCurrentLine();
            imageClickProcessedThisFrame = true; // Отмечаем, что клик обработан для изображения
        }
    }

    void LateUpdate()
    {
        // В LateUpdate проверяем, нужно ли заблокировать диалог
        // Это вызывается после Update диалоговой системы
        if (waitingForImageClick && !imageClickProcessedThisFrame && Input.GetMouseButtonDown(0))
        {
            // Если в Update диалоговая система уже обработала клик, мы все равно можем показать изображение
            ShowImageForCurrentLine();
        }
    }

    private void InitializeUI()
    {
        if (imagePanel != null)
        {
            imagePanel.SetActive(false);
            imageRectTransform = displayImage.GetComponent<RectTransform>();

            if (imageRectTransform != null)
            {
                originalImagePosition = imageRectTransform.anchoredPosition;
                targetImagePosition = originalImagePosition;
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideImage);
        }

        if (clickToContinueText != null)
        {
            clickToContinueText.gameObject.SetActive(false);
        }
    }

    private void CacheReflectionFields()
    {
        var systemType = typeof(DialoguesSystem);
        currentDialogueIndexField = systemType.GetField("currentDialogueIndex",
            BindingFlags.NonPublic | BindingFlags.Instance);
        currentLineIndexField = systemType.GetField("currentLineIndex",
            BindingFlags.NonPublic | BindingFlags.Instance);
        dialoguesField = systemType.GetField("dialogues",
            BindingFlags.NonPublic | BindingFlags.Instance);
        isTypingField = systemType.GetField("isTyping",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Пробуем получить свойство, если поле недоступно
        dialoguesProperty = systemType.GetProperty("dialogues",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    // Вызывается из DialoguesSystem когда строка полностью отображена
    public void CheckForImage()
    {
        if (dialogueSystem == null || isImageModeActive) return;

        if (HasImageForCurrentLine())
        {
            waitingForImageClick = true;
            ShowClickToContinueText();
        }
        else
        {
            waitingForImageClick = false;
            HideClickToContinueText();
        }
    }

    private bool HasImageForCurrentLine()
    {
        int currentDialogueIndex = (int)currentDialogueIndexField.GetValue(dialogueSystem);
        int currentLineIndex = (int)currentLineIndexField.GetValue(dialogueSystem);

        DialogueDataNPC[] dialogues;

        // Пробуем получить dialogues разными способами
        if (dialoguesField != null)
        {
            dialogues = (DialogueDataNPC[])dialoguesField.GetValue(dialogueSystem);
        }
        else if (dialoguesProperty != null)
        {
            dialogues = (DialogueDataNPC[])dialoguesProperty.GetValue(dialogueSystem);
        }
        else
        {
            // Если не получилось, пробуем через публичное свойство
            var publicDialoguesField = dialogueSystem.GetType().GetField("dialogues",
                BindingFlags.Public | BindingFlags.Instance);
            if (publicDialoguesField != null)
            {
                dialogues = (DialogueDataNPC[])publicDialoguesField.GetValue(dialogueSystem);
            }
            else
            {
                return false;
            }
        }

        if (currentDialogueIndex < 0 || dialogues == null ||
            currentDialogueIndex >= dialogues.Length)
            return false;

        var dialogueData = dialogues[currentDialogueIndex];
        if (dialogueData == null) return false;

        string currentNpcId = dialogueData.npcId;

        foreach (var dialogueImageData in dialogueImages)
        {
            if (dialogueImageData.npcId == currentNpcId)
            {
                foreach (var imagePerLine in dialogueImageData.imagesPerLine)
                {
                    if (imagePerLine.lineIndex == currentLineIndex)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void ShowImageForCurrentLine()
    {
        if (!waitingForImageClick || isImageModeActive) return;

        int currentDialogueIndex = (int)currentDialogueIndexField.GetValue(dialogueSystem);
        int currentLineIndex = (int)currentLineIndexField.GetValue(dialogueSystem);

        DialogueDataNPC[] dialogues;

        if (dialoguesField != null)
        {
            dialogues = (DialogueDataNPC[])dialoguesField.GetValue(dialogueSystem);
        }
        else if (dialoguesProperty != null)
        {
            dialogues = (DialogueDataNPC[])dialoguesProperty.GetValue(dialogueSystem);
        }
        else
        {
            var publicDialoguesField = dialogueSystem.GetType().GetField("dialogues",
                BindingFlags.Public | BindingFlags.Instance);
            if (publicDialoguesField != null)
            {
                dialogues = (DialogueDataNPC[])publicDialoguesField.GetValue(dialogueSystem);
            }
            else
            {
                return;
            }
        }

        if (currentDialogueIndex < 0 || dialogues == null ||
            currentDialogueIndex >= dialogues.Length)
            return;

        var dialogueData = dialogues[currentDialogueIndex];
        if (dialogueData == null) return;

        string currentNpcId = dialogueData.npcId;
        Texture2D imageToShow = GetImageForLine(currentNpcId, currentLineIndex);

        if (imageToShow != null)
        {
            SetupImageDisplay(imageToShow);
            imagePanel.SetActive(true);
            isImageModeActive = true;
            waitingForImageClick = false;
            HideClickToContinueText();
        }
        else
        {
            // Если изображения нет, продолжаем диалог
            ContinueDialogue();
            waitingForImageClick = false;
            HideClickToContinueText();
        }
    }

    private Texture2D GetImageForLine(string npcId, int lineIndex)
    {
        if (string.IsNullOrEmpty(npcId)) return null;

        foreach (var dialogueImageData in dialogueImages)
        {
            if (dialogueImageData.npcId == npcId)
            {
                foreach (var imagePerLine in dialogueImageData.imagesPerLine)
                {
                    if (imagePerLine.lineIndex == lineIndex)
                    {
                        string currentLanguage = LocalizationSettings.SelectedLocale.Identifier.Code;

                        if (currentLanguage == "ru" && imagePerLine.russianImage != null)
                            return imagePerLine.russianImage;
                        else if (currentLanguage == "en" && imagePerLine.englishImage != null)
                            return imagePerLine.englishImage;
                        else if (imagePerLine.russianImage != null)
                            return imagePerLine.russianImage;
                    }
                }
            }
        }

        return null;
    }

    private void SetupImageDisplay(Texture2D image)
    {
        displayImage.texture = image;
        displayImage.raycastTarget = false;

        // Рассчитываем оптимальный размер
        float maxWidth = Screen.width * 0.8f;
        float maxHeight = Screen.height * 0.8f;

        float imageRatio = (float)image.width / image.height;
        float maxRatio = maxWidth / maxHeight;

        Vector2 displaySize;

        if (imageRatio > maxRatio)
        {
            displaySize = new Vector2(maxWidth, maxWidth / imageRatio);
        }
        else
        {
            displaySize = new Vector2(maxHeight * imageRatio, maxHeight);
        }

        imageRectTransform.sizeDelta = displaySize;
        originalImageSize = displaySize;
        currentZoom = 1f;

        // Сбрасываем позицию
        imageRectTransform.anchoredPosition = originalImagePosition;
        targetImagePosition = originalImagePosition;
        currentVelocity = Vector2.zero;
    }

    private void HandleImageMode()
    {
        // Обработка перетаскивания
        HandleDrag();

        // Обработка зума
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float oldZoom = currentZoom;
            currentZoom += scroll * zoomStep * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            // Пересчитываем позицию при зуме чтобы изображение оставалось под курсором
            if (currentZoom != oldZoom)
            {
                Vector2 mousePosition = Input.mousePosition;
                Vector2 imageCenter = imageRectTransform.position;
                Vector2 offset = (mousePosition - imageCenter) * ((currentZoom / oldZoom) - 1);
                targetImagePosition += offset / currentZoom;

                // Применяем изменения сразу
                imageRectTransform.anchoredPosition = targetImagePosition;
            }

            imageRectTransform.sizeDelta = originalImageSize * currentZoom;
        }

        // Выход по Escape или ПКМ
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            HideImage();
        }
    }

    private void HandleDrag()
    {
        if (!enableDrag) return;

        // Проверяем, не над кнопкой закрытия
        if (IsPointerOverCloseButton())
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = false;
                return;
            }
        }

        // Начало перетаскивания
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartMousePosition = Input.mousePosition;
            dragStartImagePosition = imageRectTransform.anchoredPosition;
            currentVelocity = Vector2.zero; // Сбрасываем плавность при начале перетаскивания
        }
        // Завершение перетаскивания
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            // При резком отпускании можно добавить инерцию здесь
            // currentVelocity = (targetImagePosition - imageRectTransform.anchoredPosition) * inertiaFactor;
        }

        // Перетаскивание
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 delta = (currentMousePosition - dragStartMousePosition) * dragSensitivity;

            // Вычисляем новую позицию
            targetImagePosition = dragStartImagePosition + delta;

            // Ограничиваем позицию
            targetImagePosition = ClampPositionToBounds(targetImagePosition);

            // Если плавность отключена, применяем позицию сразу
            if (!smoothDrag)
            {
                imageRectTransform.anchoredPosition = targetImagePosition;
            }
        }
    }

    private bool IsPointerOverCloseButton()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == closeButton.gameObject)
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 ClampPositionToBounds(Vector2 position)
    {
        // Получаем текущий размер изображения с учетом зума
        float scaledWidth = originalImageSize.x * currentZoom;
        float scaledHeight = originalImageSize.y * currentZoom;

        // Границы в пикселях (процент от размеров экрана)
        float boundaryWidth = Screen.width * dragBoundary;
        float boundaryHeight = Screen.height * dragBoundary;

        // Вычисляем допустимые границы для центра изображения
        float minX = -scaledWidth / 2 + boundaryWidth;
        float maxX = scaledWidth / 2 - boundaryWidth;
        float minY = -scaledHeight / 2 + boundaryHeight;
        float maxY = scaledHeight / 2 - boundaryHeight;

        // Если изображение меньше экрана, ограничиваем его центром
        if (scaledWidth < Screen.width)
        {
            minX = maxX = 0;
        }
        else
        {
            // Корректируем границы чтобы изображение не уходило за экран
            float screenHalfWidth = Screen.width / 2;
            if (maxX > screenHalfWidth) maxX = screenHalfWidth - boundaryWidth;
            if (minX < -screenHalfWidth) minX = -screenHalfWidth + boundaryWidth;
        }

        if (scaledHeight < Screen.height)
        {
            minY = maxY = 0;
        }
        else
        {
            float screenHalfHeight = Screen.height / 2;
            if (maxY > screenHalfHeight) maxY = screenHalfHeight - boundaryHeight;
            if (minY < -screenHalfHeight) minY = -screenHalfHeight + boundaryHeight;
        }

        // Применяем ограничения
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    private void HideImage()
    {
        if (!isImageModeActive) return;

        imagePanel.SetActive(false);
        displayImage.texture = null;
        isImageModeActive = false;
        currentZoom = 1f;
        isDragging = false;

        displayImage.raycastTarget = true;

        ContinueDialogue();
    }

    private void ContinueDialogue()
    {
        MethodInfo showNextLineMethod = typeof(DialoguesSystem).GetMethod("ShowNextLine",
            BindingFlags.NonPublic | BindingFlags.Instance);
        showNextLineMethod?.Invoke(dialogueSystem, null);
    }

    private void ShowClickToContinueText()
    {
        if (showClickToContinueText && clickToContinueText != null)
        {
            clickToContinueText.gameObject.SetActive(true);

            string currentLanguage = LocalizationSettings.SelectedLocale.Identifier.Code;
            clickToContinueText.text = currentLanguage == "ru"
                ? "Нажмите для просмотра изображения"
                : "Click to view image";
        }
    }

    private void HideClickToContinueText()
    {
        if (clickToContinueText != null)
        {
            clickToContinueText.gameObject.SetActive(false);
        }
    }

    public bool IsImageModeActive()
    {
        return isImageModeActive;
    }

    // Новый метод для проверки, ожидаем ли мы клик для изображения
    public bool IsWaitingForImageClick()
    {
        return waitingForImageClick;
    }

    // Публичные методы для управления
    public void SetDragEnabled(bool enabled)
    {
        enableDrag = enabled;
    }

    public void SetDragSensitivity(float sensitivity)
    {
        dragSensitivity = Mathf.Max(0.1f, sensitivity);
    }

    public void ResetImagePosition()
    {
        targetImagePosition = originalImagePosition;
        currentVelocity = Vector2.zero;

        if (!smoothDrag)
        {
            imageRectTransform.anchoredPosition = targetImagePosition;
        }
    }

    // Метод для отладки границ
    public void DrawDebugBounds()
    {
        Vector2 clampedPos = ClampPositionToBounds(targetImagePosition);
        Debug.Log($"Current Position: {targetImagePosition}, Clamped Position: {clampedPos}");
        Debug.Log($"Image Size: {originalImageSize * currentZoom}, Zoom: {currentZoom}");
    }
}
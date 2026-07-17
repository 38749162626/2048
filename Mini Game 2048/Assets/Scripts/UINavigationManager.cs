using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class UINavigationManager : MonoBehaviour
{
    [Header("默认首选按钮")]
    [SerializeField] private GameObject defaultFirstSelected;
    [SerializeField] private bool selectOnStart = true;

    [Header("行为")]
    [SerializeField] private bool clearOnMouseClick = false;
    [SerializeField] private bool autoSelectOnAnyGamepadKey = true;
    [SerializeField] private float keyPollInterval = 0.2f; // 轮询间隔，避免每帧触发

    private bool isGamepadConnected;
    private float lastKeyPressTime;
    private bool hasPendingAutoSelect; // 标记是否需要自动选中

    private static UINavigationManager instance;
    public static UINavigationManager Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void Start()
    {
        CheckCurrentDevice();
        if (selectOnStart && defaultFirstSelected != null && Gamepad.current != null)
        {
            SelectFirstButton();
        }
        lastKeyPressTime = -keyPollInterval; // 确保第一次检测能触发
    }

    private void Update()
    {
        // 如果功能未启用，直接返回
        if (!autoSelectOnAnyGamepadKey) return;

        // 条件1：手柄已连接
        if (Gamepad.current == null) return;

        // 条件2：当前没有选中的 UI 元素（失去焦点）
        if (EventSystem.current.currentSelectedGameObject != null) return;

        // 条件3：默认按钮存在
        if (defaultFirstSelected == null) return;

        // 条件4：检测手柄任意按键是否被按下（轮询）
        if (IsAnyGamepadButtonPressed())
        {
            // 限制触发频率，避免连续触发
            if (Time.unscaledTime - lastKeyPressTime >= keyPollInterval)
            {
                lastKeyPressTime = Time.unscaledTime;
                SelectFirstButton();
            }
        }
    }

    /// <summary>
    /// 检测手柄上是否有任意按键被按下（包括方向键、扳机、肩键等）
    /// </summary>
    private bool IsAnyGamepadButtonPressed()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) return false;

        // 检查所有常见的按钮和轴（阈值过滤）
        // 注意：部分轴（如扳机）需要判断是否大于阈值，避免摇杆漂移误触
        float threshold = 0.5f;

        // 检查所有按钮（按下状态）
        foreach (var button in gamepad.allControls)
        {
            // 如果是按钮类型（ButtonControl）
            if (button is ButtonControl btn && btn.wasPressedThisFrame)
                return true;

            // 如果是轴（如摇杆、扳机），检查其值是否超过阈值（模拟“按下”）
            if (button is AxisControl axis)
            {
                float val = axis.ReadValue();
                if (Mathf.Abs(val) > threshold)
                    return true;
            }
        }

        return false;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed)
        {
            CheckCurrentDevice();
        }
    }

    private void CheckCurrentDevice()
    {
        bool wasConnected = isGamepadConnected;
        isGamepadConnected = Gamepad.current != null;

        if (isGamepadConnected && !wasConnected)
        {
            SelectFirstButton();
        }

        if (!isGamepadConnected && wasConnected)
        {
            if (clearOnMouseClick)
                ClearSelected();
        }
    }

    public void SelectFirstButton()
    {
        if (defaultFirstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultFirstSelected);
        }
        else
        {
            Button first = FindFirstObjectByType<Button>();
            if (first != null)
                EventSystem.current.SetSelectedGameObject(first.gameObject);
            else
                Debug.LogWarning("[UINavigationManager] 没有找到默认按钮，请设置 defaultFirstSelected");
        }
    }

    public void ClearSelected()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SetDefaultButton(GameObject newDefault)
    {
        defaultFirstSelected = newDefault;
        if (isGamepadConnected)
            SelectFirstButton();
    }

    public void SelectButton(GameObject target)
    {
        EventSystem.current.SetSelectedGameObject(target);
    }
}
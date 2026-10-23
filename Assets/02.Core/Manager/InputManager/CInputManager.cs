using System;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// 뉴 인풋 매니저의 입력만 받아와서 이벤트로 전달
/// </summary>
public class CInputManager : MonoBehaviour
{

    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _dashAction;
    [SerializeField] private InputActionReference _attackAction;
    [SerializeField] private InputActionReference _specialAttackAction;
    [SerializeField] private InputActionReference _rapidCasting1Action;
    [SerializeField] private InputActionReference _rapidCasting2Action;
    [SerializeField] private InputActionReference _interactionAction;
    [SerializeField] private InputActionReference _usePotionAction;
    [SerializeField] private InputActionReference _openMapAction;
    [SerializeField] private InputActionReference _inventoryAction;
    [SerializeField] private InputActionReference _playerStatsAction;
    [SerializeField] private InputActionReference _talentAction;
    [SerializeField] private InputActionReference _presetSettingAction;
    [SerializeField] private InputActionReference _chattingAction;
    [SerializeField] private InputActionReference _reloadingAction;
    [SerializeField] private InputActionReference _pingAction;
    [SerializeField] private InputActionReference _choosePotionAction;
    [SerializeField] private InputActionReference _beforeAction;
    [SerializeField] private InputActionReference _nextAction;
    [SerializeField] private InputActionReference _ESCAction;

    public event Action<Vector2> OnMove;
    public event Action<float> OnDash;
    public event Action<float> OnAttack;
    public event Action <float> OnSpecialAttack;
    public event Action OnRapidCasting1;
    public event Action OnRapidCasting2;
    public event Action OnInteraction;
    public event Action OnUsePotion;
    public event Action OnOpenMap;
    public event Action OnInventory;
    public event Action OnPlayerStats;
    public event Action OnTalent;
    public event Action OnPresetSetting;
    public event Action OnChatting;
    public event Action OnReloading;
    public event Action OnPing;
    public event Action OnChoosePotion;
    public event Action OnBefore;
    public event Action OnNext;
    public event Action OnESC;


    private void OnEnable()
    {
        _moveAction.action.performed += moveAction;
        _moveAction.action.canceled += moveActionCanceled;

        _dashAction.action.performed += dashAction;
        _dashAction.action.canceled += dashActionCanceled;

        _attackAction.action.performed += attackAction;
        _attackAction.action.canceled += attackActionCanceled;

        _specialAttackAction.action.performed += specialAttackAction;
        _specialAttackAction.action.canceled += specialAttackActionCanceled;

        _rapidCasting1Action.action.performed += rapidCasting1Action;
        _rapidCasting2Action.action.performed += rapidCasting2Action;
        _interactionAction.action.performed += interactionAction;
        _usePotionAction.action.performed += usePotionAction;
        _openMapAction.action.performed += openMapAction;
        _inventoryAction.action.performed += inventoryAction;
        _playerStatsAction.action.performed += playerStatsAction;
        _talentAction.action.performed += talentAction;
        _presetSettingAction.action.performed += presetSettingAction;
        _chattingAction.action.performed += chattingAction;
        _reloadingAction.action.performed += reloadingAction;
        _pingAction.action.performed += pingAction;
        _choosePotionAction.action.performed += choosePotionAction;
        _beforeAction.action.performed += beforeAction;
        _nextAction.action.performed += nextAction;
        _ESCAction.action.performed += ESCAction;

        _moveAction.action.Enable();
        _dashAction.action.Enable();
        _attackAction.action.Enable();
        _specialAttackAction.action.Enable();
        _rapidCasting1Action.action.Enable();
        _rapidCasting2Action.action.Enable();
        _interactionAction.action.Enable();
        _usePotionAction.action.Enable();
        _openMapAction.action.Enable();
        _inventoryAction.action.Enable();
        _playerStatsAction.action.Enable();
        _talentAction.action.Enable();
        _presetSettingAction.action.Enable();
        _chattingAction.action.Enable();
        _reloadingAction.action.Enable();
        _pingAction.action.Enable();
        _choosePotionAction.action.Enable();
        _beforeAction.action.Enable();
        _nextAction.action.Enable();
        _ESCAction.action.Enable();
    }

    private void OnDisable()
    {
        _moveAction.action.performed -= moveAction;
        _moveAction.action.canceled -= moveActionCanceled;

        _dashAction.action.performed -= dashAction;
        _dashAction.action.canceled -= dashActionCanceled;

        _attackAction.action.performed -= attackAction;
        _attackAction.action.canceled -= attackActionCanceled;

        _specialAttackAction.action.performed -= specialAttackAction;
        _specialAttackAction.action.canceled -= specialAttackActionCanceled;

        _rapidCasting1Action.action.performed -= rapidCasting1Action;
        _rapidCasting2Action.action.performed -= rapidCasting2Action;
        _interactionAction.action.performed -= interactionAction;
        _usePotionAction.action.performed -= usePotionAction;
        _openMapAction.action.performed -= openMapAction;
        _inventoryAction.action.performed -= inventoryAction;
        _playerStatsAction.action.performed -= playerStatsAction;
        _talentAction.action.performed -= talentAction;
        _presetSettingAction.action.performed -= presetSettingAction;
        _chattingAction.action.performed -= chattingAction;
        _reloadingAction.action.performed -= reloadingAction;
        _pingAction.action.performed -= pingAction;
        _choosePotionAction.action.performed -= choosePotionAction;
        _beforeAction.action.performed -= beforeAction;
        _nextAction.action.performed -= nextAction;
        _ESCAction.action.performed -= ESCAction;

        _moveAction.action.Disable();
        _dashAction.action.Disable();
        _attackAction.action.Disable();
        _specialAttackAction.action.Disable();
        _rapidCasting1Action.action.Disable();
        _rapidCasting2Action.action.Disable();
        _interactionAction.action.Disable();
        _usePotionAction.action.Disable();
        _openMapAction.action.Disable();
        _inventoryAction.action.Disable();
        _playerStatsAction.action.Disable();
        _talentAction.action.Disable();
        _presetSettingAction.action.Disable();
        _chattingAction.action.Disable();
        _reloadingAction.action.Disable();
        _pingAction.action.Disable();
        _choosePotionAction.action.Disable();
        _beforeAction.action.Disable();
        _nextAction.action.Disable();
        _ESCAction.action.Disable();
    }

    private void moveAction(InputAction.CallbackContext ctx)
    {
        OnMove?.Invoke(ctx.ReadValue<Vector2>());
    }

    private void moveActionCanceled(InputAction.CallbackContext ctx)
    {
        OnMove?.Invoke(Vector2.zero);
    }

    private void dashAction(InputAction.CallbackContext ctx)
    {
        OnDash?.Invoke(1f);
    }

    private void dashActionCanceled(InputAction.CallbackContext ctx)
    {
        OnDash?.Invoke(0f);
    }

    // 좌클릭 누름
    private void attackAction(InputAction.CallbackContext ctx)
    {
        OnAttack?.Invoke(1f);
    }

    // 좌클릭 땜
    private void attackActionCanceled(InputAction.CallbackContext ctx)
    {
        OnAttack?.Invoke(0f);
    }

    // 우클릭 누름
    private void specialAttackAction(InputAction.CallbackContext ctx)
    {
        OnSpecialAttack?.Invoke(1f);
    }

    // 우클릭 땜
    private void specialAttackActionCanceled(InputAction.CallbackContext ctx)
    {
        OnSpecialAttack?.Invoke(0f);
    }
    private void rapidCasting1Action(InputAction.CallbackContext ctx)
    {
        OnRapidCasting1?.Invoke();
    }

    private void rapidCasting2Action(InputAction.CallbackContext ctx)
    {
        OnRapidCasting2?.Invoke();
    }

    private void interactionAction(InputAction.CallbackContext ctx)
    {
        OnInteraction?.Invoke();
    }

    private void usePotionAction(InputAction.CallbackContext ctx)
    {
        OnUsePotion?.Invoke();
    }

    private void openMapAction(InputAction.CallbackContext ctx)
    {
        OnOpenMap?.Invoke();
    }

    private void inventoryAction(InputAction.CallbackContext ctx)
    {
        OnInventory?.Invoke();
    }

    private void playerStatsAction(InputAction.CallbackContext ctx)
    {
        OnPlayerStats?.Invoke();
    }

    private void talentAction(InputAction.CallbackContext ctx)
    {
        OnTalent?.Invoke();
    }

    private void presetSettingAction(InputAction.CallbackContext ctx)
    {
        OnPresetSetting?.Invoke();
    }

    private void chattingAction(InputAction.CallbackContext ctx)
    {
        OnChatting?.Invoke();
    }

    private void reloadingAction(InputAction.CallbackContext ctx)
    {
        OnReloading?.Invoke();
    }

    private void pingAction(InputAction.CallbackContext ctx)
    {
        OnPing?.Invoke();
    }

    private void choosePotionAction(InputAction.CallbackContext ctx)
    {
        OnChoosePotion?.Invoke();
    }

    private void beforeAction(InputAction.CallbackContext ctx)
    {
        OnBefore?.Invoke();
    }

    private void nextAction(InputAction.CallbackContext ctx)
    {
        OnNext?.Invoke();
    }

    private void ESCAction(InputAction.CallbackContext ctx)
    {
        OnESC?.Invoke();
    }
}
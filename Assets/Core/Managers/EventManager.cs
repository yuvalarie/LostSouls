using System;
using Game_Flow.Camera;
using Game_Flow.DotVisual.Scripts.States;
using UnityEngine;

namespace Core.Managers
{
    public class EventManager : MonoBehaviour
    {
        
        public static EventManager Instance {get; private set;}
        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public static event Action<IObjeckLockingState> OnLockStateChanged;

        public static event Action OnDollPlaced; 

        public static void LockStateChanged(IObjeckLockingState state)
        {
            OnLockStateChanged?.Invoke(state);
        }
        
        public static event Action<bool> OnPlayerZoneChanged;

        public static void TriggerPlayerZoneChanged(bool canSwitchView)
        {
            OnPlayerZoneChanged?.Invoke(canSwitchView);
        }
        
        public static event Action<ViewMode> OnViewModeChanged;

        public static void ViewModeChanged(ViewMode newMode)
        {
            OnViewModeChanged?.Invoke(newMode);
        }
        
        public static event Action <float, float, float> OnStartRumble;

        public static void StartRumble(float duration, float lowFrequency, float highFrequency)
        {
            OnStartRumble?.Invoke(duration, lowFrequency, highFrequency);
        }
        
        public static void DollPlaced()
        {
            OnDollPlaced?.Invoke();
        }
        
        public static event Action OnShowPainting;

        public static void ShowPainting()
        {
            OnShowPainting?.Invoke();
        }
        
        public static event Action OnExitPainting; 
        public static void ExitPainting()
        {
            OnExitPainting?.Invoke();
        }

        public static event Action FinishGame;

        public static void OnFinishGame()
        {
            FinishGame?.Invoke();
        }
        
        public static event Action EnterRoom;
        
        public static void OnEnterRoom()
        {
            EnterRoom?.Invoke();
        }
    }
}
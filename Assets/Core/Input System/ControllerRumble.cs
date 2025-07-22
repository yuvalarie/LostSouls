using System.Collections;
using Core.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Input_System
{
    public class ControllerRumble: MonoBehaviour
    {
        private Gamepad _gamepad;
        
        private void OnEnable()
        {
            EventManager.OnStartRumble += RumblePulse;
        }
        private void OnDisable()
        {
            EventManager.OnStartRumble -= RumblePulse;
        }
        
        private void RumblePulse(float duration, float lowFrequency, float highFrequency)
        {
            _gamepad = Gamepad.current;
            if (_gamepad == null)
            {
                Debug.Log("No gamepad");
                return;
            }

            _gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
            StartCoroutine(Rumble(duration));
        }

        private IEnumerator Rumble(float duration)
        {
            float timePassed = 0f;
            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                yield return null;
            }
    
            _gamepad.SetMotorSpeeds(0, 0);
            Debug.Log("Rumble Ended");
        }
    }
}
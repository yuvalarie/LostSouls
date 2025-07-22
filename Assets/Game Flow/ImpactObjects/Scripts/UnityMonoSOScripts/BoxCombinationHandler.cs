
using System;
using System.Collections.Generic;
using Core.Audio;
using Core.Managers;
using DG.Tweening;
using Game_Flow.ImpactObjects.Scripts.Audio;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sequence = DG.Tweening.Sequence;

namespace Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts
{
    public class BoxCombinationHandler : MonoBehaviour
    {
        [SerializeField] private MonoImpactObject thisPart;
        [SerializeField] private MonoImpactObject otherPart;
        [SerializeField] private Grid grid;
        [SerializeField] private List<UnityEngine.ParticleSystem> activate;
        [SerializeField] private List<UnityEngine.ParticleSystem> deActivate;
        [SerializeField] private float combineDuration = 0.5f;
        [SerializeField] private Ease combineEase = Ease.OutQuad;
        
        [SerializeField] private float rightBoxPoint;
        [SerializeField] private float leftBoxPoint;
        
        
        public bool GameFinished { get; private set; }

        private bool _combining;
        private Tween _doTween;

        

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                FinishGame();
            }
            if (_combining || GameFinished) return;

            // Example trigger: thisPart adjacent to otherPart
            var cells = thisPart.UsedCells;
            var neighbor = grid.GetOccupant(cells[0].x + 1, cells[0].y);
            
            var otherBoxCells = otherPart.UsedCells;
            var otherBoxNeighbor = grid.GetOccupant(otherBoxCells[0].x - 1, otherBoxCells[0].y);
            
            if (neighbor == otherPart || otherBoxNeighbor == thisPart)
            {
                FinishGame();
            }
            
            
        }

        private void FinishGame()
        {
            if (GameFinished) return;
            if(!thisPart.IsMoving && !otherPart.IsMoving)
            {
                EventManager.StartRumble(8f, .05f, .1f);
                EndSceneMusic.Instance.PlayMusic();
                GameFinished = true;

                // Play any activation particles
                foreach (var ps in deActivate)
                {
                    ps.Stop();
                }
            
                foreach (var ps in activate)
                {
                    ps.gameObject.SetActive(true);
                    ps.Play();
                }

                // Start combine animation
                
                AnimateCombine();
            }
            
        }

        private void AnimateCombine()
        {
            _combining = true;

            // Compute midpoint between the two parts
            Vector3 startA = thisPart.transform.position;
            Vector3 startB = otherPart.transform.position;

            // A moves to the 25% point (one quarter) from A→B
            Vector3 targetA = Vector3.Lerp(startA, startB, rightBoxPoint );

            // B moves to the 75% point (three quarters) from A→B
            Vector3 targetB = Vector3.Lerp(startB, startA, leftBoxPoint);


            Sequence seq = DOTween.Sequence();
            seq.Append(thisPart.transform.DOMove(targetA, combineDuration).SetEase(combineEase));
            seq.Join(otherPart.transform.DOMove(targetB, combineDuration).SetEase(combineEase));
            seq.OnComplete(() =>
            {
                SceneFader.Instance.FadeToScene("Ending Scene");
            });
        }
    }
}

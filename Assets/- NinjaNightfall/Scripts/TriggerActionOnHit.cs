using UnityEngine;
using System;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    [AddComponentMenu("Corgi Engine/Character/Tools/Trigger Action On Hit")]
    [RequireComponent(typeof(Collider2D))]
    public class TriggerActionOnHit : MonoBehaviour
    {
        public LayerMask TargetLayerMask;
        public bool TriggerOnEnter = true;
        public bool TriggerOnStay = false;

        public event Action OnValidHit;

        protected virtual void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (TriggerOnEnter)
            {
                EvaluateHit(collider);
            }
        }

        protected virtual void OnTriggerStay2D(Collider2D collider)
        {
            if (TriggerOnStay)
            {
                EvaluateHit(collider);
            }
        }

        protected virtual void EvaluateHit(Collider2D collider)
        {
            if (!MMLayers.LayerInLayerMask(collider.gameObject.layer, TargetLayerMask))
            {
                return;
            }

            Debug.Log("Tiro no jogador");
            OnValidHit?.Invoke();
        }

        protected virtual void OnDisable()
        {
            // Prevent duplicate events if reused from pool
            if (OnValidHit != null)
            {
                foreach (Delegate d in OnValidHit.GetInvocationList())
                {
                    OnValidHit -= (Action)d;
                }
            }
        }
    }
}
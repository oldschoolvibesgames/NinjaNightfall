using System;
using UnityEngine;
using UnityEngine.Events;

public class ObjectPerTime : MonoBehaviour
{
    public FrameObject[] frames;
    public UnityEvent onFrameEnd;
    private int _frameIndex = 0;
    private bool _frameEnd = false;

    private void OnEnable()
    {
        foreach (var f in frames)
        {
            f.frame.SetActive(false);
        }

        _frameIndex = 0;
        frames[_frameIndex].frame.SetActive(true);
    }

    private void Update()
    {
        if (!_frameEnd)
        {
            if (frames[_frameIndex].timeDuration > 0)
            {
                frames[_frameIndex].timeDuration -= Time.deltaTime;
            }
            else
            {
                if (_frameIndex + 1 < frames.Length)
                {
                    frames[_frameIndex].frame.SetActive(false);
                    _frameIndex++;
                    frames[_frameIndex].frame.SetActive(true);
                }
                else
                {
                    _frameEnd = true;
                    onFrameEnd?.Invoke();
                }
            }
        }
    }

    [Serializable]
    public class FrameObject
    {
        public GameObject frame;
        public float timeDuration = 0;
    }
}

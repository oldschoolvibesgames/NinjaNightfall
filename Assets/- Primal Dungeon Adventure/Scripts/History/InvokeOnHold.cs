using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InvokeOnHold : MonoBehaviour
{
    public GameObject skip;
    public Image fill;
    public UnityEvent onSkip;
    public GameObject view;
    private PlayerInputs _inpult;

    private void Awake()
    {
        _inpult = new PlayerInputs();
    }

    private void OnEnable()
    {
        _inpult.Enable();
    }

    private void OnDisable()
    {
        _inpult.Disable();
    }

    private void Update()
    {
        if (_inpult.Menu.Select.IsPressed())
        {
            skip.SetActive(true);
            fill.fillAmount += Time.deltaTime / 3;

            if (fill.fillAmount == 1)
            {
                view.SetActive(false);
                onSkip?.Invoke();
                fill.fillAmount = 0;
                skip.SetActive(false);
            }
        }
        else
        {
            skip.SetActive(false);
            fill.fillAmount = 0;
        }
    }
}

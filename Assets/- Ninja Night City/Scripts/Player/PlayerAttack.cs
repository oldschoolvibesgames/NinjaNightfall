using MoreMountains.CorgiEngine;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Animator playerAnimator;
    public GameObject[] attacks;

    private CorgiController _corgiController;
    private PlayerInputs _inputAction;
    private Health _health;
    private bool _canAttack = true;


    void Awake()
    {
        _corgiController = GetComponent<CorgiController>();
        _inputAction = new PlayerInputs();
        _inputAction.Enable();
        
        _health = GetComponent<Health>();
        _health.OnDeath += DisableAttack;
        _health.OnRevive += EnableAttack;
    }

    private void OnEnable()
    {
        CancelAttack();
    }

    private void OnDestroy()
    {
        _inputAction.Disable();
        _inputAction.Gameplay.Disable();
    }

    private void Update()
    {
        if(!_canAttack || _health.CurrentHealth <= 0) return;
        
        if (_inputAction.Gameplay.Attack0.triggered && _corgiController.State.IsGrounded)
        {
            Sword2Attack(true);
        }
        else if (_inputAction.Gameplay.Attack0.triggered && !_corgiController.State.IsGrounded)
        {
            Sword1Attack(true);
        }
        else if (_inputAction.Gameplay.Special.triggered)
        {
            Sword3Attack(true);
        }
    }

    private void CancelAttack()
    {
        Sword1Attack(false);
        Sword2Attack(false);
        Sword3Attack(false);

        foreach (var atk in attacks)
        {
            atk.SetActive(false);
        }
        

    }

    public void DisableAttack()
    {
        _canAttack = false;
        CancelAttack();
    }

    public void EnableAttack()
    {
        CancelAttack();
        _canAttack = true;
        
        playerAnimator.ResetTrigger("Sword1");
        playerAnimator.ResetTrigger("Sword2");
        playerAnimator.ResetTrigger("Sword3");
    }
    
    public void Sword1Attack(bool status)
    {
        playerAnimator.ResetTrigger("Sword1");
        playerAnimator.ResetTrigger("Sword2");
        playerAnimator.ResetTrigger("Sword3");
        
        playerAnimator.SetBool("Sword1", status);
        _canAttack = !status;
    }
    
    public void Sword2Attack(bool status)
    {   
        playerAnimator.ResetTrigger("Sword1");
        playerAnimator.ResetTrigger("Sword2");
        playerAnimator.ResetTrigger("Sword3");
        
        playerAnimator.SetBool("Sword2", status);
        _canAttack = !status;
    }
    
    public void Sword3Attack(bool status)
    {
        playerAnimator.ResetTrigger("Sword1");
        playerAnimator.ResetTrigger("Sword2");
        playerAnimator.ResetTrigger("Sword3");
        
        playerAnimator.SetBool("Sword3", status);
        _canAttack = !status;
    }
}

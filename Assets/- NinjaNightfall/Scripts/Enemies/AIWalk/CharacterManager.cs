using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public Animator animator;
    public bool canBlockDistanceAttack;
    public bool canBlockBodyAttack;

    public void BlockAttack()
    {
        animator.SetTrigger("Block");
    }
}

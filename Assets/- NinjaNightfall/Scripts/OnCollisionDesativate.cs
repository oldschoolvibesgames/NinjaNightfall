using UnityEngine;

public class OnCollisionDesativate : MonoBehaviour
{
    [Header("Objeto a ser desativado")]
    public GameObject targetToDisable;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && targetToDisable != null)
        {
            targetToDisable.SetActive(false);
        }
    }


}

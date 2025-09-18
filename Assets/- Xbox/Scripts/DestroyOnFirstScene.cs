using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyOnFirstScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            Destroy(this.gameObject);
    }
}

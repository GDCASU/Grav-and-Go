using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryManager : MonoBehaviour
{
    public static RetryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void OnDeath(GameObject deadVictim)
    {
        gameObject.BroadcastMessage("OnDeath", gameObject);

        if (deadVictim.TryGetComponent(out PlayerMovementController player))
        {
            OnRetry();
            RetryUI();
        }
    }

    public void OnRetry()
    {
        //Reloads the whole level. Temporary solution; replace with LoadSave() (or the equivalent) when the saving system is done.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RetryUI()
    {
        
    }
}

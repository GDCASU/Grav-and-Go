using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryManager : MonoBehaviour
{
    public void OnDeath(GameObject deadVictim)
    {
        gameObject.BroadcastMessage("OnDeath", gameObject);

        if (deadVictim.TryGetComponent(out PlayerMovementController player)) RetryUI();

        Retry();
    }

    public void Retry()
    {
        //Reloads the whole level. Temporary solution; replace with LoadSave() (or the equivalent) when the saving system is done.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RetryUI()
    {
        
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class PortaAberta : MonoBehaviour, IInteractable
{
    public void Interagir()
    {
        SceneManager.LoadScene("Oficina_Selecao");
    }
}
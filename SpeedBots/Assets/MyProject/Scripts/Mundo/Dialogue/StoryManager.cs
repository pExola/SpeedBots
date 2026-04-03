using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [Header("Configurações de Cena")]
    public string nomeCenaCorrida = "Cena_Corrida_01";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Remova o método Start() inteiro!

    public void IrParaCorrida()
    {
        Debug.Log($"<color=green>[SISTEMA]</color> Carregando a corrida: {nomeCenaCorrida}");
        SceneManager.LoadScene(nomeCenaCorrida);
    }
}
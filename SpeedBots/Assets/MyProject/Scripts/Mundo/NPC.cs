using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("Configuração do Twine")]
    public string arquivoDoDialogo;
    public string noInicial = "Inicio";

    [Header("Transição de Cena")]
    public string cenaAoEncerrar = "";

    public void Interagir()
    {
        // Simples e direto. Roda o que estiver na gaveta.
        LeitorTwine.Instance.CarregarTwee(arquivoDoDialogo);
        DialogueManager.Instance.IniciarDialogo(noInicial, cenaAoEncerrar);
    }
}
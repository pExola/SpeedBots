using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("Configuração do Twine")]
    [Tooltip("Nome do arquivo de texto na pasta Resources (sem o .txt)")]
    public string arquivoDoDialogo; // Ex: Dialogo_Tom, Dialogo_Piastri, etc.

    [Tooltip("O nó onde a conversa deve começar")]
    public string noInicial = "Inicio";

    public void Interagir()
    {
        // 1. O NPC manda o Leitor engolir o arquivo DELE antes de falar
        LeitorTwine.Instance.CarregarTwee(arquivoDoDialogo);

        // 2. Agora sim, ele manda a interface abrir no nó certo
        DialogueManager.Instance.ExibirNo(noInicial);
    }
}
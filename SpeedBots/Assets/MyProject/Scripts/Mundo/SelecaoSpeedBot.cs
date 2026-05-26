using UnityEngine;

public class SelecaoSpeedBot : MonoBehaviour, IInteractable
{
    [Header("Identificação do Robô")]
    public string nomeDesteRobo;

    [Header("Textos (Twine)")]
    // Coloque aqui o nome do arquivo .twee que tem os textos desta cena
    public string arquivoDoDialogo;

    // O nome do "Nó" no Twine que tem a frase: "Acho que devo falar com Oscar antes..."
    public string noBloqueado = "BloqueioPiastri";

    // O nome do "Nó" no Twine que abre a escolha do robô
    public string noLiberado = "EscolhaRobo";

    public static bool falouComOscar = false;

    void Awake()
    {
        // RESET DE SEGURANÇA:
        // Toda vez que você dá o Play e a cena carrega, forçamos o bloqueio a voltar para 'false'.
        // Isso evita que a Unity grave o resultado do teste anterior.
        falouComOscar = false;
    }

    public void Interagir()
    {
        // Carrega o arquivo de texto igualzinho ao script do NPC
        LeitorTwine.Instance.CarregarTwee(arquivoDoDialogo);

        // 1. CHECAGEM DO BLOQUEIO
        if (!falouComOscar)
        {
            // Em vez de só imprimir no console, agora ele ABRE o diálogo de bloqueio na tela!
            DialogueManager.Instance.IniciarDialogo(noBloqueado, "");
            return;
        }

        // 2. SELEÇÃO LIBERADA
        DialogueManager.Instance.IniciarDialogo(noLiberado, "");
    }
}
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SelecaoSpeedBot : MonoBehaviour, IInteractable
{
    [Header("Identificação do Robô")]
    public string nomeDesteRobo;

    [Header("Textos (Twine)")]
    public string arquivoDoDialogo;
    public string noBloqueado = "BloqueioPiastri";
    public string noLiberado = "EscolhaRobo";
    public string roboEscolhido = "roboEscolhido";

    [Header("Animação e Efeitos")]
    public string parametroLigado = "Ligado";

    [Tooltip("Arraste o GameObject Acesa_ correspondente a este robô para cá")]
    public GameObject luzDoRobo; // O objeto que será ativado/desativado

    private Animator animador;
    public static bool falouComOscar = false;
    public static bool escolheuRobo = false;

    void Awake()
    {
        falouComOscar = false;
        escolheuRobo = false;
        animador = GetComponent<Animator>();

        // Garante que a luz sempre comece desligada quando a cena carregar
        if (luzDoRobo != null)
        {
            luzDoRobo.SetActive(false);
        }
    }

    public void Interagir()
    {
        LeitorTwine.Instance.CarregarTwee(arquivoDoDialogo);

        // 1. Trava do Piastri
        if (!falouComOscar)
        {
            DialogueManager.Instance.IniciarDialogo(noBloqueado, "");
            return;
        }

        if (escolheuRobo)
        {
            // Toca o nó "roboEscolhido" para sempre e para o código aqui.
            DialogueManager.Instance.IniciarDialogo(roboEscolhido, "");
            return;
        }

        // 2. Animação de ligar
        if (animador != null)
        {
            animador.SetBool(parametroLigado, true);
        }

        // 3. LIGA O GAMEOBJECT ESPECÍFICO (Crawler, Slider ou Aerial)
        if (luzDoRobo != null)
        {
            luzDoRobo.SetActive(true);
        }

        DialogueManager.Instance.IniciarDialogo(noLiberado, "");
    }

    // A MÁGICA DE DESLIGAR
    void OnCollisionExit2D(Collision2D colisao)
    {
        if (colisao.gameObject.CompareTag("Player"))
        {
            // Apaga o olho do robô
            if (animador != null)
            {
                animador.SetBool(parametroLigado, false);
            }

            // DESLIGA O GAMEOBJECT quando o Sam desencosta
            if (luzDoRobo != null)
            {
                luzDoRobo.SetActive(false);
            }
        }
    }
}
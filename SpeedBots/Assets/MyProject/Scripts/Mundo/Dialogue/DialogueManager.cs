using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class PerfilPersonagem
{
    public string nome;
    public Sprite foto;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI do Diálogo")]
    public GameObject painelDialogo;
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoPrincipal;
    public Image fotoPersonagem;

    [Header("Botões")]
    public Transform areaDosBotoes;
    public GameObject prefabBotao;

    [Header("Banco de Personagens")]
    public List<PerfilPersonagem> perfis;

    // --- VARIÁVEIS DE CONTROLE DO RITMO ---
    private NoTwine noAtualAtivo;
    private int indiceDaFala = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        painelDialogo.SetActive(false);
    }

    public void ExibirNo(string tituloDoNo)
    {
        painelDialogo.SetActive(true);
        if (!LeitorTwine.Instance.historia.ContainsKey(tituloDoNo)) return;

        noAtualAtivo = LeitorTwine.Instance.historia[tituloDoNo];
        indiceDaFala = 0; // Sempre começa a ler da primeira linha do bloco

        MostrarFalaNaTela();
    }

    private void MostrarFalaNaTela()
    {
        if (noAtualAtivo == null || noAtualAtivo.falas.Count == 0) return;

        LinhaDeFala fala = noAtualAtivo.falas[indiceDaFala];
        textoPrincipal.text = fala.texto;

        // --- SISTEMA UNIVERSAL DE PERSONAGENS vs NARRAÇÃO ---
        if (fala.nome == "Narrador")
        {
            // MODO NARRAÇÃO: Limpa o nome e esconde a foto
            if (textoNome != null) textoNome.text = "";
            if (fotoPersonagem != null) fotoPersonagem.gameObject.SetActive(false);
        }
        else
        {
            // MODO PERSONAGEM: Exibe o nome e caça a foto
            if (textoNome != null) textoNome.text = fala.nome;

            if (fotoPersonagem != null)
            {
                Sprite fotoEncontrada = null;
                foreach (var perfil in perfis)
                {
                    // Checa se o nome digitado no Twine bate com o banco de dados
                    if (fala.nome.Contains(perfil.nome))
                    {
                        fotoEncontrada = perfil.foto;
                        break;
                    }
                }

                if (fotoEncontrada != null)
                {
                    fotoPersonagem.sprite = fotoEncontrada;
                    fotoPersonagem.gameObject.SetActive(true);
                }
                else
                {
                    fotoPersonagem.gameObject.SetActive(false);
                }
            }
        }

        // Limpa a tela de botões
        foreach (Transform filho in areaDosBotoes) Destroy(filho.gameObject);

        // A LÓGICA DE AVANÇO FICA IGUAL
        if (indiceDaFala >= noAtualAtivo.falas.Count - 1)
        {
            foreach (var resposta in noAtualAtivo.respostas)
            {
                GameObject novoBotao = Instantiate(prefabBotao, areaDosBotoes);
                novoBotao.GetComponentInChildren<TextMeshProUGUI>().text = resposta.Key;
                string destino = resposta.Value;
                novoBotao.GetComponent<Button>().onClick.AddListener(() => { ExibirNo(destino); });
            }

            if (noAtualAtivo.respostas.Count == 0)
            {
                GameObject botaoSair = Instantiate(prefabBotao, areaDosBotoes);
                botaoSair.GetComponentInChildren<TextMeshProUGUI>().text = "Encerrar";
                botaoSair.GetComponent<Button>().onClick.AddListener(() => {
                    painelDialogo.SetActive(false);
                });
            }
        }
        else
        {
            GameObject btnContinuar = Instantiate(prefabBotao, areaDosBotoes);
            btnContinuar.GetComponentInChildren<TextMeshProUGUI>().text = "Continuar ▼";
            btnContinuar.GetComponent<Button>().onClick.AddListener(() => { AvancarDialogo(); });
        }
    }

    // -----------------------------------------------------------------
    // ADAPTAÇÃO PARA OS SCRIPTS ANTIGOS (PlayerOverworld e Interactor)
    // -----------------------------------------------------------------
    public bool isTalking
    {
        get { return painelDialogo.activeInHierarchy; }
    }

    // Acionado pelo botão Continuar na tela OU apertando a tecla "E"
    public void AvancarDialogo()
    {
        if (noAtualAtivo == null) return;

        // Se ainda tem falas, pula pra próxima linha e recarrega a tela
        if (indiceDaFala < noAtualAtivo.falas.Count - 1)
        {
            indiceDaFala++;
            MostrarFalaNaTela();
        }
        else
        {
            // Se já acabou o texto, tenta clicar no botão da tela (desde que só tenha 1 botão, pra não escolher resposta errada)
            if (areaDosBotoes.childCount == 1)
            {
                Button botaoUnico = areaDosBotoes.GetChild(0).GetComponent<Button>();
                if (botaoUnico != null) botaoUnico.onClick.Invoke();
            }
        }
    }
}
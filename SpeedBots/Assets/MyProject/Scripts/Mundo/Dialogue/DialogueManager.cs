using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PerfilPersonagem
{
    public string nome;
    public Sprite foto;
}

public class DialogueManager : MonoBehaviour
{
    // --- INSTÂNCIA BLINDADA ---
    private static DialogueManager _instance;
    public static DialogueManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<DialogueManager>();
            return _instance;
        }
    }

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

    private NoTwine noAtualAtivo;
    private int indiceDaFala = 0;
    private string cenaAoEncerrarAtual = "";

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(_instance.gameObject);
        }
        _instance = this;

        if (painelDialogo != null) painelDialogo.SetActive(false);
    }

    public bool isTalking
    {
        get
        {
            if (painelDialogo == null) return false;
            return painelDialogo.activeInHierarchy;
        }
    }

    public void IniciarDialogo(string tituloDoNo, string cenaDestino = "")
    {
        cenaAoEncerrarAtual = cenaDestino;
        ExibirNo(tituloDoNo);
    }

    public void ExibirNo(string tituloDoNo)
    {
        if (painelDialogo == null) return;

        painelDialogo.SetActive(true);
        if (!LeitorTwine.Instance.historia.ContainsKey(tituloDoNo)) return;

        noAtualAtivo = LeitorTwine.Instance.historia[tituloDoNo];
        indiceDaFala = 0;

        MostrarFalaNaTela();
    }

    private void MostrarFalaNaTela()
    {
        if (noAtualAtivo == null || noAtualAtivo.falas.Count == 0) return;

        LinhaDeFala fala = noAtualAtivo.falas[indiceDaFala];
        textoPrincipal.text = fala.texto;

        if (fala.nome == "Narrador")
        {
            if (textoNome != null) textoNome.text = "";
            if (fotoPersonagem != null) fotoPersonagem.gameObject.SetActive(false);
        }
        else
        {
            if (textoNome != null) textoNome.text = fala.nome;
            if (fotoPersonagem != null)
            {
                Sprite fotoEncontrada = null;
                foreach (var perfil in perfis)
                {
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
                else fotoPersonagem.gameObject.SetActive(false);
            }
        }

        foreach (Transform filho in areaDosBotoes) Destroy(filho.gameObject);

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

                    // Teletransporte direto, sem depender de StoryManager!
                    if (!string.IsNullOrEmpty(cenaAoEncerrarAtual))
                    {
                        Time.timeScale = 1;
                        SceneManager.LoadScene(cenaAoEncerrarAtual);
                    }
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

    public void AvancarDialogo()
    {
        if (noAtualAtivo == null) return;

        if (indiceDaFala < noAtualAtivo.falas.Count - 1)
        {
            indiceDaFala++;
            MostrarFalaNaTela();
        }
        else
        {
            if (areaDosBotoes.childCount == 1)
            {
                Button botaoUnico = areaDosBotoes.GetChild(0).GetComponent<Button>();
                if (botaoUnico != null)
                {
                    // --- TRAVA ANTI-STACKOVERFLOW: O sistema não clica sozinho se o botão for "Continuar" ---
                    string textoBotao = botaoUnico.GetComponentInChildren<TextMeshProUGUI>().text;
                    if (!textoBotao.Contains("Continuar"))
                    {
                        botaoUnico.onClick.Invoke();
                    }
                }
            }
        }
    }

    public void ExibirMensagemRapida(string mensagem)
    {
        if (painelDialogo == null) return;
        painelDialogo.SetActive(true);
        if (textoNome != null) textoNome.text = "";
        textoPrincipal.text = mensagem;
        if (fotoPersonagem != null) fotoPersonagem.gameObject.SetActive(false);
        foreach (Transform filho in areaDosBotoes) Destroy(filho.gameObject);
        GameObject btn = Instantiate(prefabBotao, areaDosBotoes);
        btn.GetComponentInChildren<TextMeshProUGUI>().text = "Fechar";
        btn.GetComponent<Button>().onClick.AddListener(() => { painelDialogo.SetActive(false); });
    }
}
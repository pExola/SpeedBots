using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabletUIManager : MonoBehaviour
{
    public static TabletUIManager Instance { get; private set; }

    [Header("Painéis Principais")]
    public GameObject painelTablet; // O painel mestre do tablet
    public Transform gridDeItens;   // A pasta onde os botões vão ser criados
    public GameObject prefabSlotItem; // O botão que você salvou como Prefab

    [Header("Área de Detalhes (Direita)")]
    public Image iconeDetalhe;
    public TextMeshProUGUI textoNomeDetalhe;
    public TextMeshProUGUI textoDescricaoDetalhe;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // O tablet começa desligado
        painelTablet.SetActive(false);
        LimparDetalhes();
    }

    private void Update()
    {
        // Pressionar TAB abre/fecha o Tablet
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (painelTablet.activeSelf) FecharTablet();
            else AbrirTablet();
        }
    }

    public void AbrirTablet()
    {
        painelTablet.SetActive(true);
        ConstruirGridDeItens();
        LimparDetalhes(); // Começa com o lado direito vazio

        // Pausa o jogo (opcional, comum em RPGs)
        Time.timeScale = 0f;
    }

    public void FecharTablet()
    {
        painelTablet.SetActive(false);
        Time.timeScale = 1f; // Despausa o jogo
    }

    private void ConstruirGridDeItens()
    {
        // 1. Destrói os botões antigos
        foreach (Transform filho in gridDeItens)
        {
            Destroy(filho.gameObject);
        }

        List<PecaSpeedBot> mochila = InventarioManager.Instance.pecasGuardadas;

        // 2. MÁGICA DE INVENTÁRIO: Agrupa e conta itens repetidos
        Dictionary<PecaSpeedBot, int> contagemPecas = new Dictionary<PecaSpeedBot, int>();
        foreach (PecaSpeedBot peca in mochila)
        {
            if (contagemPecas.ContainsKey(peca)) contagemPecas[peca]++;
            else contagemPecas[peca] = 1;
        }

        // 3. Cria um botão único para cada TIPO de peça que existe no Dicionário
        foreach (var kvp in contagemPecas)
        {
            PecaSpeedBot peca = kvp.Key;
            int quantidade = kvp.Value;

            GameObject novoSlot = Instantiate(prefabSlotItem, gridDeItens);

            // Coloca o Ícone
            Image iconeSlot = novoSlot.transform.Find("Image").GetComponent<Image>();
            if (iconeSlot != null && peca.icone != null)
            {
                iconeSlot.sprite = peca.icone;
            }

            // Coloca a Quantidade (Procura o objeto "Qtd" no Prefab)
            Transform objQtd = novoSlot.transform.Find("Qtd");
            if (objQtd != null)
            {
                TextMeshProUGUI textoQtd = objQtd.GetComponent<TextMeshProUGUI>();
                if (textoQtd != null)
                {
                    // Se tiver só 1, deixa o texto invisível para ficar mais limpo. Se tiver mais, mostra o número.
                    textoQtd.text = quantidade > 1 ? quantidade.ToString() : "";
                }
            }

            Button botaoSlot = novoSlot.GetComponent<Button>();
            botaoSlot.onClick.AddListener(() => MostrarDetalhesDoItem(peca));
        }

        StartCoroutine(ArrumarGridDelay());
    }

    private IEnumerator ArrumarGridDelay()
    {
        // Espera o fim do frame atual para a Unity terminar de criar os botões
        yield return new WaitForEndOfFrame();

        // Força a atualização do layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridDeItens.GetComponent<RectTransform>());
    }

    // Método chamado quando você clica em um item no Grid
    public void MostrarDetalhesDoItem(PecaSpeedBot peca)
    {
        iconeDetalhe.gameObject.SetActive(true);
        iconeDetalhe.sprite = peca.icone;
        textoNomeDetalhe.text = peca.nomeDaPeca;

        // Monta a descrição com os atributos técnicos!
        string descricaoFormatada = peca.descricao + "\n\n";

        if (peca.tipoPeca == TipoPeca.Chassi)
        {
            descricaoFormatada += $"<color=#AADDFF>Classe:</color> {peca.classe}\n";
            descricaoFormatada += $"<color=#AADDFF>Arrancada:</color> {peca.arrancadaBase * 100}%\n";
            descricaoFormatada += $"<color=#AADDFF>Durabilidade:</color> {peca.durabilidadeBase * 100}%";
        }
        else if (peca.tipoPeca == TipoPeca.Motor)
        {
            descricaoFormatada += $"<color=#FFAAAA>Velocidade Máx:</color> {peca.velocidadeMaxima}\n";
            descricaoFormatada += $"<color=#FFAAAA>Aceleração:</color> {peca.aceleracao}";
        }
        else if (peca.tipoPeca == TipoPeca.Modulo)
        {
            descricaoFormatada += $"<color=#AAFFAA>Habilidade:</color> {peca.habilidadeEspecial}";
        }

        textoDescricaoDetalhe.text = descricaoFormatada;
    }

    private void LimparDetalhes()
    {
        iconeDetalhe.gameObject.SetActive(false);
        textoNomeDetalhe.text = "Selecione um item";
        textoDescricaoDetalhe.text = "";
    }
}
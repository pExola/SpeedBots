using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
        // 1. Destrói os botões antigos para não duplicar
        foreach (Transform filho in gridDeItens)
        {
            Destroy(filho.gameObject);
        }

        // 2. Busca a mochila do jogador (Milestone 2)
        List<PecaSpeedBot> mochila = InventarioManager.Instance.pecasGuardadas;

        // 3. Cria um botão para cada item
        foreach (PecaSpeedBot peca in mochila)
        {
            GameObject novoSlot = Instantiate(prefabSlotItem, gridDeItens);

            // Procura a Imagem dentro do Prefab para colocar o ícone da peça
            Image iconeSlot = novoSlot.transform.Find("Image").GetComponent<Image>();
            if (iconeSlot != null && peca.icone != null)
            {
                iconeSlot.sprite = peca.icone;
            }

            // O SEGREDO: Configura o botão para mostrar os detalhes quando clicado
            Button botaoSlot = novoSlot.GetComponent<Button>();
            botaoSlot.onClick.AddListener(() => MostrarDetalhesDoItem(peca));
        }
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
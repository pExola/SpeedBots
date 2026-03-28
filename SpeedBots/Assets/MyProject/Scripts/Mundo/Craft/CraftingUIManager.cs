using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CraftingUIManager : MonoBehaviour
{
    public static CraftingUIManager Instance { get; private set; }

    [Header("Configuração Geral")]
    public GameObject painelCrafting;
    public List<ReceitaCraft> bancoDeReceitas;

    [Header("Área Esquerda (Lista de Peças)")]
    public Transform gridListaReceitas;
    public GameObject prefabBotaoReceita;

    [Header("Área Direita (Painel Inteiro)")]
    // NOVA GAVETA: O objeto pai que engloba tudo da direita
    public GameObject painelDetalhesDireita;

    [Header("Área Direita (Detalhes)")]
    public Image iconeDetalhe;
    public TextMeshProUGUI textoNomeDetalhe;
    public TextMeshProUGUI textoDescricaoDetalhe;
    public Button botaoFabricar;

    [Header("Área Direita (Grid Dinâmico)")]
    public Transform gridIngredientesDinamico;
    public GameObject prefabSlotIngrediente;

    private ReceitaCraft receitaSelecionada;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        painelCrafting.SetActive(false);
    }

    public void AbrirBancada()
    {
        painelCrafting.SetActive(true);
        LimparDetalhes(); // Começa com a direita invisível
        FiltrarReceitas(TipoPeca.Chassi);
    }

    public void FecharBancada() { painelCrafting.SetActive(false); }

    public void BotaoFiltroChassi() { FiltrarReceitas(TipoPeca.Chassi); }
    public void BotaoFiltroMotor() { FiltrarReceitas(TipoPeca.Motor); }
    public void BotaoFiltroModulo() { FiltrarReceitas(TipoPeca.Modulo); }

    private void FiltrarReceitas(TipoPeca tipoFiltro)
    {
        foreach (Transform filho in gridListaReceitas) Destroy(filho.gameObject);
        if (bancoDeReceitas == null) return;

        bool selecionouPrimeiro = false;

        foreach (ReceitaCraft receita in bancoDeReceitas)
        {
            if (receita == null || receita.pecaResultado == null) continue;

            if (receita.pecaResultado.tipoPeca == tipoFiltro)
            {
                GameObject novoBotao = Instantiate(prefabBotaoReceita, gridListaReceitas);

                Transform objTexto = novoBotao.transform.Find("Text_Nome");
                Transform objIcone = novoBotao.transform.Find("Image_Icone");

                if (objTexto != null) objTexto.GetComponent<TextMeshProUGUI>().text = receita.pecaResultado.nomeDaPeca;

                // Proteção para não ficar um quadrado branco se faltar ícone
                if (objIcone != null && receita.pecaResultado.icone != null)
                {
                    objIcone.GetComponent<Image>().sprite = receita.pecaResultado.icone;
                }

                ReceitaCraft receitaAtual = receita;
                novoBotao.GetComponent<Button>().onClick.AddListener(() => SelecionarReceita(receitaAtual));

                if (!selecionouPrimeiro)
                {
                    // Comentamos isso para ele NÃO selecionar automaticamente o primeiro
                    // SelecionarReceita(receitaAtual);
                    selecionouPrimeiro = true;
                }
            }
        }

        LimparDetalhes(); // Força a direita a ficar invisível até o jogador clicar em algo
    }

    private void SelecionarReceita(ReceitaCraft receita)
    {
        receitaSelecionada = receita;

        // 1. ATIVA O PAINEL DA DIREITA
        if (painelDetalhesDireita != null) painelDetalhesDireita.SetActive(true);

        if (receita.pecaResultado.icone != null) iconeDetalhe.sprite = receita.pecaResultado.icone;
        textoNomeDetalhe.text = receita.pecaResultado.nomeDaPeca;
        textoDescricaoDetalhe.text = receita.pecaResultado.descricao;

        foreach (Transform filho in gridIngredientesDinamico) Destroy(filho.gameObject);

        bool podeFabricar = true;

        foreach (Ingrediente ing in receita.ingredientes)
        {
            // PROTEÇÃO: Se esquecer de preencher o ingrediente na receita
            if (ing == null || ing.recursoNecessario == null)
            {
                podeFabricar = false;
                continue;
            }

            GameObject novoSlot = Instantiate(prefabSlotIngrediente, gridIngredientesDinamico);
            int tenhoNaMochila = ContarNoInventario(ing.recursoNecessario);

            Transform objIcone = novoSlot.transform.Find("Image_Icone");
            Transform objNome = novoSlot.transform.Find("Text_Nome");
            Transform objDesc = novoSlot.transform.Find("Text_Descricao");
            Transform objQtd = novoSlot.transform.Find("Text_Qtd");

            // PROTEÇÃO: Avisa se o Prefab dos Ingredientes estiver com nome errado
            if (objIcone == null || objNome == null || objDesc == null || objQtd == null)
            {
                Debug.LogError($"<color=red>[CRAFT]</color> O prefab {prefabSlotIngrediente.name} não tem os filhos exatos: Image_Icone, Text_Nome, Text_Descricao, Text_Qtd");
                continue;
            }

            if (ing.recursoNecessario.icone != null) objIcone.GetComponent<Image>().sprite = ing.recursoNecessario.icone;
            objNome.GetComponent<TextMeshProUGUI>().text = ing.recursoNecessario.nomeDaPeca;
            objDesc.GetComponent<TextMeshProUGUI>().text = ing.recursoNecessario.descricao;

            TextMeshProUGUI textoQtd = objQtd.GetComponent<TextMeshProUGUI>();
            textoQtd.text = $"{tenhoNaMochila} / {ing.quantidadeNecessaria}";

            if (tenhoNaMochila < ing.quantidadeNecessaria)
            {
                textoQtd.color = Color.red;
                podeFabricar = false; // Falta material, desliga o botão!
            }
            else
            {
                textoQtd.color = Color.black;
            }
        }

        // Ativa ou desativa o botão baseado nos materiais
        botaoFabricar.interactable = podeFabricar;
    }

    private int ContarNoInventario(PecaSpeedBot pecaBuscada)
    {
        int total = 0;
        foreach (PecaSpeedBot p in InventarioManager.Instance.pecasGuardadas)
        {
            if (p == pecaBuscada) total++;
        }
        return total;
    }

    public void FabricarItem()
    {
        if (receitaSelecionada == null) return;

        foreach (Ingrediente ing in receitaSelecionada.ingredientes)
        {
            for (int i = 0; i < ing.quantidadeNecessaria; i++)
            {
                InventarioManager.Instance.pecasGuardadas.Remove(ing.recursoNecessario);
            }
        }

        InventarioManager.Instance.AdicionarPeca(receitaSelecionada.pecaResultado);
        Debug.Log($"<color=green>[CRAFT]</color> {receitaSelecionada.pecaResultado.nomeDaPeca} fabricado!");
        SelecionarReceita(receitaSelecionada);
    }

    private void LimparDetalhes()
    {
        // ESCONDE O PAINEL INTEIRO DA DIREITA
        if (painelDetalhesDireita != null) painelDetalhesDireita.SetActive(false);
        receitaSelecionada = null;
    }
}
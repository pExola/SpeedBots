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
    public TextMeshProUGUI tituloCategoriaEsquerda; // NOVA GAVETA: O Título da Categoria
    public Transform gridListaReceitas;
    public GameObject prefabBotaoReceita;

    [Header("Área Direita (Painel Inteiro)")]
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

    // Guarda o estado atual para podermos atualizar a tela automaticamente depois de fabricar
    private TipoPeca filtroAtual;
    private string tituloAtual;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        painelCrafting.SetActive(false);
    }

    public void AbrirBancada()
    {
        painelCrafting.SetActive(true);
        LimparDetalhes();
        BotaoFiltroChassi(); // Abre no Chassi por padrão
    }

    public void FecharBancada() { painelCrafting.SetActive(false); }

    // --- OS BOTÕES DO TOPO AGORA PASSAM O TÍTULO ---
    public void BotaoFiltroChassi() { FiltrarReceitas(TipoPeca.Chassi, "Chassis"); }
    public void BotaoFiltroMotor() { FiltrarReceitas(TipoPeca.Motor, "Motores"); }
    public void BotaoFiltroModulo() { FiltrarReceitas(TipoPeca.Modulo, "Módulos"); }

    private void FiltrarReceitas(TipoPeca tipoFiltro, string tituloFiltro)
    {
        filtroAtual = tipoFiltro;
        tituloAtual = tituloFiltro;

        // Atualiza o texto do título acima da lista
        if (tituloCategoriaEsquerda != null) tituloCategoriaEsquerda.text = tituloFiltro;

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

                // Baseado na sua imagem anterior, o Qtd fica dentro da Image_Icone!
                Transform objQtd = novoBotao.transform.Find("Image_Icone/Qtd");

                if (objTexto != null) objTexto.GetComponent<TextMeshProUGUI>().text = receita.pecaResultado.nomeDaPeca;
                if (objIcone != null && receita.pecaResultado.icone != null) objIcone.GetComponent<Image>().sprite = receita.pecaResultado.icone;

                // MÁGICA DA QUANTIDADE: Calcula quantos você pode fazer e atualiza o número no botão
                if (objQtd != null)
                {
                    int maxCraftavel = CalcularQuantidadeCraftavel(receita);
                    TextMeshProUGUI textoQtd = objQtd.GetComponent<TextMeshProUGUI>();

                    textoQtd.text = maxCraftavel.ToString();

                    // Fica vermelhinho/cinza se você não puder fazer nenhum
                    textoQtd.color = maxCraftavel > 0 ? Color.white : new Color(1f, 0.5f, 0.5f);
                }

                ReceitaCraft receitaAtual = receita;
                novoBotao.GetComponent<Button>().onClick.AddListener(() => SelecionarReceita(receitaAtual));
            }
        }

        LimparDetalhes(); // Esconde a direita até o jogador clicar
    }

    // --- A CALCULADORA DE MATERIAIS ---
    private int CalcularQuantidadeCraftavel(ReceitaCraft receita)
    {
        if (receita.ingredientes.Count == 0) return 0;

        int maxCraftavel = 999; // Começa alto e vai abaixando conforme acha o limite

        foreach (Ingrediente ing in receita.ingredientes)
        {
            int tenho = ContarNoInventario(ing.recursoNecessario);

            // Divisão inteira: Ex: Tenho 5, Preciso de 2. 5/2 = Posso fazer 2.
            int possoFazerDesteIngrediente = tenho / ing.quantidadeNecessaria;

            if (possoFazerDesteIngrediente < maxCraftavel)
            {
                maxCraftavel = possoFazerDesteIngrediente;
            }
        }

        return maxCraftavel;
    }

    private void SelecionarReceita(ReceitaCraft receita)
    {
        receitaSelecionada = receita;
        if (painelDetalhesDireita != null) painelDetalhesDireita.SetActive(true);

        if (receita.pecaResultado.icone != null) iconeDetalhe.sprite = receita.pecaResultado.icone;
        textoNomeDetalhe.text = receita.pecaResultado.nomeDaPeca;
        textoDescricaoDetalhe.text = receita.pecaResultado.descricao;

        foreach (Transform filho in gridIngredientesDinamico) Destroy(filho.gameObject);

        bool podeFabricar = true;

        foreach (Ingrediente ing in receita.ingredientes)
        {
            if (ing == null || ing.recursoNecessario == null) continue;

            GameObject novoSlot = Instantiate(prefabSlotIngrediente, gridIngredientesDinamico);
            int tenhoNaMochila = ContarNoInventario(ing.recursoNecessario);

            Transform objIcone = novoSlot.transform.Find("Image_Icone");
            Transform objNome = novoSlot.transform.Find("Text_Nome");
            Transform objDesc = novoSlot.transform.Find("Text_Descricao");
            Transform objQtd = novoSlot.transform.Find("Text_Qtd");

            if (ing.recursoNecessario.icone != null) objIcone.GetComponent<Image>().sprite = ing.recursoNecessario.icone;
            objNome.GetComponent<TextMeshProUGUI>().text = ing.recursoNecessario.nomeDaPeca;
            objDesc.GetComponent<TextMeshProUGUI>().text = ing.recursoNecessario.descricao;

            TextMeshProUGUI textoQtd = objQtd.GetComponent<TextMeshProUGUI>();
            textoQtd.text = $"{tenhoNaMochila} / {ing.quantidadeNecessaria}";

            if (tenhoNaMochila < ing.quantidadeNecessaria)
            {
                textoQtd.color = Color.red;
                podeFabricar = false;
            }
            else { textoQtd.color = Color.black; }
        }

        botaoFabricar.interactable = podeFabricar;
    }

    private int ContarNoInventario(PecaSpeedBot pecaBuscada)
    {
        int total = 0;
        foreach (PecaSpeedBot p in InventarioManager.Instance.pecasGuardadas) { if (p == pecaBuscada) total++; }
        return total;
    }

    public void FabricarItem()
    {
        if (receitaSelecionada == null) return;

        // 1. SALVAMENTO DE EMERGÊNCIA: Guarda a receita atual numa variável local 
        // antes que o comando FiltrarReceitas apague ela!
        ReceitaCraft receitaSalva = receitaSelecionada;

        // 2. Consome os materiais
        foreach (Ingrediente ing in receitaSalva.ingredientes)
        {
            for (int i = 0; i < ing.quantidadeNecessaria; i++)
            {
                InventarioManager.Instance.pecasGuardadas.Remove(ing.recursoNecessario);
            }
        }

        // 3. Entrega a peça nova
        InventarioManager.Instance.AdicionarPeca(receitaSalva.pecaResultado);

        // 4. Recarrega a lista da esquerda (Isso vai zerar a 'receitaSelecionada' global)
        FiltrarReceitas(filtroAtual, tituloAtual);

        // 5. Usa a nossa cópia salva para recarregar a tela da direita sem dar erro!
        SelecionarReceita(receitaSalva);
    }

    private void LimparDetalhes()
    {
        if (painelDetalhesDireita != null) painelDetalhesDireita.SetActive(false);
        receitaSelecionada = null;
    }
}
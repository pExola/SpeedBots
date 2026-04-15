using System.Collections.Generic; // Importa a biblioteca de coleções para podermos usar Listas (a nossa mochila).
using UnityEngine; // Importa as ferramentas principais do motor da Unity.

public class InventarioManager : MonoBehaviour // É a mochila e a garagem do jogador. Guarda o loot e gerencia o equipamento atual.
{
    // Usa o padrão Singleton: cria uma instância única e global para poder ser acessada facilmente de qualquer outro script.
    public static InventarioManager Instance { get; private set; }

    [Header("Mochila")] // Organiza a visualização no Inspector da Unity.
    // Guarda tudo o que você coleta no mundo dentro desta lista expansível.
    public List<PecaSpeedBot> pecasGuardadas = new List<PecaSpeedBot>();

    [Header("SpeedBot Montado (Loadout)")]
    // Gerencia qual "Loadout" exato o seu SpeedBot está usando no momento da corrida.
    public PecaSpeedBot chassiEquipado;
    public PecaSpeedBot motorEquipado;
    public PecaSpeedBot moduloEquipado;

    void Awake() // Função chamada no exato momento em que o objeto nasce no jogo.
    {
        if (Instance == null) // Se ainda não existe nenhum inventário no jogo...
        {
            Instance = this; // Ele se declara como o inventário oficial e único.

            // O comando sagrado da Unity: torna este objeto IMORTAL.
            // Isso significa que quando o jogador sair da cena "Overworld" e carregar a cena "Pista de Corrida",
            // o inventário NÃO será deletado. Ele viaja junto com você carregando suas peças equipadas!
            DontDestroyOnLoad(gameObject);
        }
        else // Se já existir outro inventário (por exemplo, quando a cena do Overworld for carregada de novo)...
        {
            // Ele se destrói imediatamente, garantindo que o inventário original (que tem seus itens salvos) continue existindo sozinho.
            Destroy(gameObject);
        }
    }

    public void AdicionarPeca(PecaSpeedBot novaPeca) // Função para guardar coisas que você acha pelo mapa (ex: em baús).
    {
        pecasGuardadas.Add(novaPeca); // Adiciona a peça recebida na lista da mochila.
        // Manda um aviso colorido para o console do desenvolvedor confirmando a coleta.
        Debug.Log($"<color=cyan>[INVENTÁRIO]</color> Você encontrou uma nova peça: {novaPeca.nomeDaPeca} ({novaPeca.tipoPeca})!");
    }

    public void EquiparPeca(PecaSpeedBot peca) // Aplica a lógica da "troca justa" de equipamentos.
    {
        // Trava de segurança: Se a peça que você está pedindo para equipar não estiver na sua mochila, a função cancela.
        if (!pecasGuardadas.Contains(peca)) return;

        // 1. Remove a peça nova de dentro da sua mochila (já que agora ela vai ser anexada ao corpo do robô).
        pecasGuardadas.Remove(peca);

        // 2. Lê a categoria da peça e aplica a "troca justa": guarda no espaço certo e devolve a antiga (se houver) para a mochila.
        switch (peca.tipoPeca)
        {
            case TipoPeca.Chassi: // Se for um Chassi...
                // Se você já tiver um chassi velho lá, ele pega esse chassi velho e devolve para a mochila (Add).
                if (chassiEquipado != null) pecasGuardadas.Add(chassiEquipado);
                chassiEquipado = peca; // Agora sim, equipa a peça nova no espaço do robô.
                break; // Encerra o fluxo desta categoria.

            case TipoPeca.Motor: // Se for um Motor...
                // Faz a troca justa: devolve o motor velho para a mochila.
                if (motorEquipado != null) pecasGuardadas.Add(motorEquipado);
                motorEquipado = peca; // Equipa o motor novo.
                break;

            case TipoPeca.Modulo: // Se for um Módulo Especial...
                // Faz a troca justa: devolve o módulo velho para a mochila.
                if (moduloEquipado != null) pecasGuardadas.Add(moduloEquipado);
                moduloEquipado = peca; // Equipa o módulo novo.
                break;
        }

        // Avisa no console que a troca matemática e a devolução da peça antiga foram um sucesso.
        Debug.Log($"<color=yellow>[OFICINA]</color> {peca.nomeDaPeca} foi equipado e a peça antiga voltou pra mochila!");
    }
}
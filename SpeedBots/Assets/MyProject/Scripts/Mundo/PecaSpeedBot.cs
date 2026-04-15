using UnityEngine; // Importa as ferramentas principais da engine da Unity.

// --- LISTAS FECHADAS (ENUMS) ---
// As Enums criam categorias estritas (listas fechadas). Isso garante que o item só possa pertencer a um desses tipos, evitando bugs e erros de digitação.
public enum TipoPeca { Chassi, Motor, Modulo, Recurso } // Define a categoria principal da peça.
public enum ClasseChassi { Nenhum, Crawler, Slider, Aerial } // Define o tipo físico/peso do chassi.
public enum TipoHabilidade { Nenhuma, Nitro, Gancho, Armadilha } // Define a habilidade atrelada ao módulo.

// A grande mágica de Interface: Isso cria um botão nativo no menu da Unity. 
// Permite que você clique com o botão direito nos seus arquivos e crie um novo item facilmente do zero!
[CreateAssetMenu(fileName = "NovaPeca", menuName = "SpeedBot/Peça de Inventário")]
public class PecaSpeedBot : ScriptableObject // É a "forma de bolo". Sendo um ScriptableObject, ele não fica grudado num boneco no mapa, ele é apenas um molde para gerar arquivos de dados ultraleves na memória do jogo.
{
    [Header("Informações Básicas")] // Cria um cabeçalho organizado no Inspector da Unity.
    public string nomeDaPeca; // A gaveta que guarda o nome que aparecerá no tablet/bancada.
    public TipoPeca tipoPeca; // A gaveta de múltipla escolha para definir o que esse item é.
    public Sprite icone; // Onde você arrasta a imagem/desenho da peça.
    [TextArea(2, 4)] public string descricao; // Cria uma caixa de texto maior (com 2 a 4 linhas) para você digitar a história/lore da peça.

    [Header("Atributos do Chassi")]
    public ClasseChassi classe; // Define se é um trator (Crawler), patinador (Slider) ou voador (Aerial).

    // Usa o [Range] para transformar o número em um Slider (barra de arrastar) de 0.0 até 1.0 lá na Unity. 
    // Isso é perfeito para fazer o balanceamento de RPG de forma visual e segura.
    [Range(0f, 1f)] public float arrancadaBase;
    [Range(0f, 1f)] public float durabilidadeBase;

    [Header("Atributos do Motor")]
    public float velocidadeMaxima; // Define a velocidade bruta limite que esse motor aguenta.
    public float aceleracao; // Define o quão rápido o motor atinge a velocidade máxima.

    [Header("Atributos do Módulo")]
    public TipoHabilidade habilidadeEspecial; // Define qual é o super-poder que esse item concede (se for um módulo).
}
using UnityEngine;

public enum TipoPeca { Chassi, Motor, Modulo }
public enum ClasseChassi { Nenhum, Crawler, Slider, Aerial }
public enum TipoHabilidade { Nenhuma, Nitro, Gancho, Armadilha } // Baseado nos seus itens de corrida

// Isso cria um botão no menu da Unity para você fabricar novas peças facilmente
[CreateAssetMenu(fileName = "NovaPeca", menuName = "SpeedBot/Peça de Inventário")]
public class PecaSpeedBot : ScriptableObject
{
    [Header("Informações Básicas")]
    public string nomeDaPeca;
    public TipoPeca tipoPeca;
    public Sprite icone;
    [TextArea(2, 4)] public string descricao;

    [Header("Atributos do Chassi")]
    public ClasseChassi classe;
    [Range(0f, 1f)] public float arrancadaBase;
    [Range(0f, 1f)] public float durabilidadeBase;

    [Header("Atributos do Motor")]
    public float velocidadeMaxima;
    public float aceleracao;

    [Header("Atributos do Módulo")]
    public TipoHabilidade habilidadeEspecial;
}

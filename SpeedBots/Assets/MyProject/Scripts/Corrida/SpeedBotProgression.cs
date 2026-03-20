using UnityEngine;

public class SpeedBotProgression : MonoBehaviour
{
    [Header("Progresso")]
    public int nivel = 1;
    public float currentXP = 0;
    public float xpToNextLevel = 100;

    [Header("Ganhos por Nível")]
    public float ganhoVelocidadePorNivel = 2f;
    public float ganhoAceleracaoPorNivel = 5f;

    private SpeedBotMovment movementScript;

    void Awake()
    {
        movementScript = GetComponent<SpeedBotMovment>();
        AtualizarAtributosNoMotor();
    }

    // Pega os status atuais para a UI mostrar
    public float GetVelocidadeAtual() { return movementScript.velocidadeMaximaBase; }
    public float GetAceleracaoAtual() { return movementScript.aceleracaoBase; }

    public void AtualizarAtributosNoMotor()
    {
        // A base é o nível 1. A cada nível extra, ele soma os ganhos.
        movementScript.velocidadeMaximaBase = 15f + ((nivel - 1) * ganhoVelocidadePorNivel);
        movementScript.aceleracaoBase = 30f + ((nivel - 1) * ganhoAceleracaoPorNivel);
    }

    public void AdicionarXP(float valor)
    {
        currentXP += valor;
        Debug.Log($"Ganhou {valor} XP! Total: {currentXP}/{xpToNextLevel}");

        if (currentXP >= xpToNextLevel)
        {
            SubirDeNivel();
        }
    }

    private void SubirDeNivel()
    {
        nivel++;
        currentXP -= xpToNextLevel; // Guarda o XP que sobrou
        xpToNextLevel = xpToNextLevel * 1.5f; // O próximo nível exige mais XP

        AtualizarAtributosNoMotor();
        Debug.Log($"LEVEL UP! Nível {nivel} alcançado!");
    }

}

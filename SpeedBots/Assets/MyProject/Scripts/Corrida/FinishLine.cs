using UnityEngine; // Importa a biblioteca principal da Unity para que o script funcione.

public class FinishLine : MonoBehaviour // Cria a classe que atua como o "juiz da corrida".
{
    private bool raceEnded = false; // Trava de segurança importantíssima: começa como falsa, indicando que a corrida ainda está rolando.

    private void OnTriggerEnter2D(Collider2D collision) // Função ativada no exato milissegundo em que um corpo físico encosta na linha de chegada.
    {
        if (raceEnded) return; // Se a corrida já acabou (o primeiro colocado já cruzou), o 'return' impede que o restante do código rode de novo.

        if (collision.CompareTag("Player")) // Lê as Tags da Unity para verificar se o corpo que bateu na linha tem a tag "Player" (o jogador).
        {
            raceEnded = true; // Aciona a trava de segurança, marcando que a partida foi encerrada.

            // 1. Aplica o XP
            SpeedBotProgression progresso = collision.GetComponent<SpeedBotProgression>(); // Acessa o script de progressão que está no robô do jogador.
            if (progresso != null) progresso.AdicionarXP(100f); // Se encontrou o script, injeta +100 de XP pela vitória.

            // 2. Procura a Tela de Resultados e ativa a Vitória
            TelaResultados tela = Object.FindFirstObjectByType<TelaResultados>(); // Procura na cena do jogo pelo script que controla a tela final.
            if (tela != null) // Se a tela existir e for encontrada...
            {
                tela.MostrarResultados(true); // ...manda a TelaResultados exibir a mensagem de Vitória (passando o valor 'true').
            }
        }
        else if (collision.CompareTag("Inimigo")) // Caso não seja o jogador, verifica se quem cruzou a linha tem a tag "Inimigo" (a Inteligência Artificial).
        {
            raceEnded = true; // Aciona a trava de segurança, encerrando a partida porque a IA chegou primeiro.

            // Procura a Tela de Resultados e ativa a Derrota
            TelaResultados tela = Object.FindFirstObjectByType<TelaResultados>(); // Procura novamente pela tela final na cena.
            if (tela != null) // Se a tela for encontrada...
            {
                tela.MostrarResultados(false); // ...manda a TelaResultados exibir a mensagem de Derrota (passando o valor 'false').
            }
        }
    }
}
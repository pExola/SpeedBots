using UnityEngine; // Importa as ferramentas principais da Unity.

public class CaixaItem : MonoBehaviour // Cria a classe da nossa clássica caixa de interrogação (estilo Mario Kart/Crash Team Racing).
{
    private void OnTriggerEnter2D(Collider2D collision) // Função ativada no momento exato em que um competidor passa por cima da caixa na pista.
    {
        // 1. O primeiro aviso: bateu em qualquer coisa! (Prática de testes para avisar no console o que tocou na caixa).
        Debug.Log($"[CAIXA] Algo bateu em mim! Nome do objeto: {collision.gameObject.name} | Tag: {collision.tag}");

        // Confere primeiro se a pessoa/objeto que bateu possui o script de inventário de corrida.
        RaceItemController inventario = collision.GetComponent<RaceItemController>();

        if (inventario != null) // Se TIVER o script de inventário (ou seja, é um corredor válido)...
        {
            Debug.Log($"[CAIXA] O objeto {collision.gameObject.name} tem um ItemController!");

            // Faz uma checagem de segurança importantíssima: o inventário deste robô está completamente vazio?
            if (inventario.itemGuardado == RaceItemController.TipoItem.Nenhum)
            {
                // Rola os dados virtuais! Como a Unity ignora o último número em Random Range de inteiros, ele sorteia de 1 a 3 (1=Nitro, 2=Armadilha, 3=Gancho).
                int sorteio = Random.Range(1, 4);

                // Converte o número sorteado para o item correspondente e entrega para o inventário do robô.
                inventario.PegarItem((RaceItemController.TipoItem)sorteio);
                Debug.Log($"[CAIXA] Sucesso! Dei o item {(RaceItemController.TipoItem)sorteio} para {collision.gameObject.name}. Destruindo a caixa...");

                // A caixa cumpriu seu papel, então ela comete suicídio (se destrói) e some da fase.
                Destroy(gameObject);
            }
            else // Caso o robô JÁ TENHA uma arma guardada no inventário...
            {
                // A caixa ignora a batida (não se destrói e não dá item), para não subscrever/apagar a arma que o corredor já tem.
                Debug.Log($"[CAIXA] Falhou: O {collision.gameObject.name} já tem o item {inventario.itemGuardado} guardado. Não vou dar outro.");
            }
        }
        else // Se o objeto que bateu não for um corredor (ex: um tiro perdido ou uma parede móvel)...
        {
            Debug.Log($"[CAIXA] Falhou: O objeto {collision.gameObject.name} não possui o script ItemController anexado.");
        }
    }
}
using UnityEngine; // Importa as ferramentas principais da engine da Unity.

[RequireComponent(typeof(Camera))] // Exige que o objeto tenha uma Camera anexada para o script funcionar (evita que o jogo quebre).
public class FixarProporcaoTela : MonoBehaviour // Cria a classe do "diretor de fotografia", que garante que a tela não fique esticada ou esmagada.
{
    [Header("Qual é a proporção oficial do seu jogo?")] // Organiza a visualização no Inspector da Unity.
    [Tooltip("O padrão da indústria hoje é 16:9")] // Adiciona uma dica visual que aparece ao passar o mouse por cima.
    public float proporcaoLargura = 16f; // Define a largura da proporção ideal desejada pelo criador do jogo (ex: 16).
    public float proporcaoAltura = 9f; // Define a altura da proporção ideal desejada (ex: 9).

    void Start() // Função chamada uma única vez, assim que a fase carrega.
    {
        AjustarCamera(); // Chama a função que faz a matemática de ajustar a tela logo no início.
    }

    // Se quiser que ajuste em tempo real ao redimensionar a janela no PC, 
    // mude 'Start()' para 'Update()', mas no Start consome menos processamento.
    private void AjustarCamera() // Função responsável por calcular e aplicar as barras pretas.
    {
        // Pega a proporção ideal que você configurou (ex: 16 dividido por 9 = ~1.77).
        float targetAspect = proporcaoLargura / proporcaoAltura;

        // Pega a resolução real da tela do jogador neste momento e descobre a proporção dela (Screen.width / Screen.height).
        float windowAspect = (float)Screen.width / (float)Screen.height;

        // Compara a proporção do monitor do jogador com a proporção ideal do seu jogo.
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = GetComponent<Camera>(); // Pega a referência do componente de câmera deste objeto.

        // Se o resultado for menor que 1, significa que a tela do jogador é mais "larga/comprida" que o jogo (ex: Ultrawide).
        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect; // Pega as configurações do retângulo de renderização atual da câmera.

            rect.width = 1.0f; // Mantém a largura da renderização preenchendo 100% (1.0f).
            rect.height = scaleHeight; // "Esmaga" a altura da renderização proporcionalmente ao monitor.
            rect.x = 0; // Prende a imagem no centro horizontal.

            // Calcula o espaço vazio que sobrou na tela e divide por 2 para deixar a imagem perfeitamente centralizada.
            rect.y = (1.0f - scaleHeight) / 2.0f;

            // Aplica as novas medidas. Onde a câmera não desenha imagem, a Unity automaticamente preenche com barras pretas (Letterbox).
            cam.rect = rect;
        }
        else // Caso contrário, se a tela for mais "quadrada" que o jogo (ex: monitores antigos 4:3)...
        {
            float scaleWidth = 1.0f / scaleHeight; // Descobre o quanto vai precisar "esmagar" a tela na horizontal.

            Rect rect = cam.rect; // Pega o retângulo da câmera novamente.

            rect.width = scaleWidth; // Esmaga a largura da renderização do jogo.
            rect.height = 1.0f; // Mantém a altura da imagem preenchendo 100% da tela.

            // Calcula o espaço vazio lateral e divide por 2 para centralizar a imagem.
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0; // Prende a imagem no centro vertical.

            // Aplica as novas medidas, gerando barras pretas nas laterais esquerda e direita (Pillarbox).
            cam.rect = rect;
        }
    }
}
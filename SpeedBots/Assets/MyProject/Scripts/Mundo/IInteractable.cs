using UnityEngine; // Importa as ferramentas da engine da Unity (embora opcional aqui, mantém o padrão dos seus scripts).

// Cria a Interface (O "Contrato" universal). 
// Ao usar a palavra 'interface' em vez de 'class', você cria uma regra. 
// Objetos completamente diferentes (Bancada, Porta, NPC) podem assinar isso e falar a mesma língua.
public interface IInteractable
{
    // Esta é apenas a "assinatura" do que deve ser feito.
    // Perceba que não há chaves {} ou lógica aqui dentro, apenas o nome da função e um ponto e vírgula (;).
    // Isso dita a regra: Qualquer script que assinar este contrato DEVERÁ possuir um método chamado "Interagir()".
    // É graças a essa linha minúscula que o seu 'PlayerInteractor' não precisa saber no que bateu, ele só aciona esse gatilho!
    void Interagir();
}
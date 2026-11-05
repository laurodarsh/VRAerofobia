namespace Aerofobia.Flight
{
    /// <summary>
    /// Etapas do voo, na ordem em que acontecem (ESPECIFICACAO, seção 3.3).
    /// Só duas passagens dependem do médico: Pronto → Decolagem e Cruzeiro → Aproximação.
    /// </summary>
    public enum FlightState
    {
        /// <summary>Parado na cabeceira, esperando o médico.</summary>
        Pronto,

        /// <summary>Acelerando na pista até sair do chão.</summary>
        Decolagem,

        /// <summary>Subindo até a altitude de cruzeiro, acima das nuvens.</summary>
        Subida,

        /// <summary>Voando em volta do aeroporto, pelo tempo que o médico quiser.</summary>
        Cruzeiro,

        /// <summary>Descendo pelo oval até alinhar e tocar a pista.</summary>
        Aproximacao,

        /// <summary>Na pista, freando até parar.</summary>
        Pouso,

        /// <summary>Parado depois do pouso. Fim do voo.</summary>
        Concluido,
    }
}

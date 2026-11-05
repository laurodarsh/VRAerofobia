namespace Aerofobia.Core
{
    /// <summary>
    /// Nomes das cenas do projeto, em um único lugar.
    /// Use estas constantes em vez de digitar o nome da cena como string solta:
    /// se uma cena for renomeada, só este arquivo muda.
    /// A ordem de <see cref="Scenarios"/> define a sequência da experiência.
    /// </summary>
    public static class SceneNames
    {
        public const string Menu = "00_Menu";
        public const string Compra = "01_Compra";
        public const string Observacao = "02_Observacao";
        public const string Voo = "03_Voo";

        /// <summary>Cenários na ordem em que o usuário deve vivenciá-los (exposição gradual).</summary>
        public static readonly string[] Scenarios = { Compra, Observacao, Voo };

        /// <summary>Todas as cenas que entram no build, na ordem do Build Profile.</summary>
        public static readonly string[] All = { Menu, Compra, Observacao, Voo };
    }
}

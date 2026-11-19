using System;
using System.Text.RegularExpressions;

namespace WarConquer
{
    // Presentation only. The source text also carries timing permissions in TimingRules.
    public static class CardReading
    {
        static readonly Regex SentenceBreak = new Regex(@"(?<=[.!?])\s+(?=[A-ZÁÉÍÓÚÑ¿¡])");
        static readonly Regex StatsPrefix = new Regex(@"^(?:MOV/HP/ATK )?\d+/\d+/\d+\.\s*");

        public static string Effect(CardData card)
        {
            string source = card.description?.Trim() ?? "";
            if (source.Length == 0) return "Consulta las estadísticas y la ventana de uso de la carta.";
            if (card.id == "espora" || card.id == "obelisco-de-arena")
                return "Ficha generada por un efecto. No forma parte del mazo principal.";
            if (card.id == "alligator-token")
                return "Mientras esté en Pantano, gana +3 MOV.\n\nEl efecto de muerte «give +1hp» está pendiente de definición.";
            switch (card.id)
            {
                case "engage":
                    return "Elige una unidad aliada y después una unidad enemiga a un máximo de 6 casillas de radio de tu aliada.\n\n"
                        + "La unidad enemiga recibe daño igual al ATK actual de tu aliada. Si muere, mueve a tu aliada a la casilla que ocupaba.";
                case "time-to-move":
                    return "Elige una casilla como centro del efecto. Todas las unidades a un máximo de 8 casillas de radio ganan +4 MOV hasta que termine el turno actual.";
                case "clearing-space":
                    return "Selecciona una casilla que puedas terraformar. Conviértela en Yermo.\n\nDespués roba 2 cartas.";
                case "cleansing-conquest":
                    return "Selecciona una casilla que puedas terraformar y conviértela en Yermo.\n\n"
                        + "Mueve a esa casilla una unidad aliada situada a un máximo de 8 casillas de radio del destino.";
                case "bomb":
                    return "Selecciona una estructura. Inflígele exactamente 3 puntos de daño.";
                case "last-gift":
                    return "Elige una unidad aliada y elimínala. Después roba tantas cartas como puntos de vida tenía esa unidad justo antes de morir.";
            }
            source = StatsPrefix.Replace(source, "");
            source = Regex.Replace(source, @"\b(\d+)rad\b", "$1 casillas de radio");
            source = Regex.Replace(source, @"\b1 casillas de radio\b", "1 casilla de radio");
            source = Regex.Replace(source, @"\bSlabs\b", "casillas");
            source = Regex.Replace(source, @"\bSlab\b", "casilla");
            source = Regex.Replace(source, @"\bUnits\b", "unidades");
            source = Regex.Replace(source, @"\bUnit\b", "unidad");
            source = Regex.Replace(source, @"\bStructure\b", "estructura");
            source = Regex.Replace(source, @"\bATQ\b", "ATK");
            source = source.Replace("Al entrar:", "Al entrar,").Replace("Al jugar:", "Al jugarla,");
            source = source.Replace("Robar 1.", "Roba 1 carta.").Replace("Robar 2.", "Roba 2 cartas.");
            source = source.Replace("Convertir un casilla", "Convierte una casilla")
                .Replace("Terraformar un casilla", "Terraforma una casilla")
                .Replace("Elegir un casilla", "Elige una casilla")
                .Replace("ese casilla", "esa casilla").Replace("su casilla seleccionado", "su casilla seleccionada")
                .Replace("del casilla", "de la casilla").Replace("un casilla adyacente", "una casilla adyacente")
                .Replace("un casilla válido", "una casilla válida").Replace("casillas adyacentes libres", "casillas libres adyacentes")
                .Replace("en un casilla", "en una casilla").Replace("la casilla seleccionado", "la casilla seleccionada");
            var sentences = SentenceBreak.Split(source);
            return string.Join("\n\n", sentences);
        }

        public static string Summary(string effect)
        {
            int paragraph = effect.IndexOf("\n\n", StringComparison.Ordinal);
            return paragraph < 0 ? effect : effect.Substring(0, paragraph) + "…";
        }
    }
}

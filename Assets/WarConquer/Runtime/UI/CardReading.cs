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
                return "Mientras esté en Pantano, gana +3 MOV.";
            switch (card.id)
            {
                case "its-a-trap":
                    return "Elige una unidad aliada o enemiga que pueda ser objetivo. Se elimina del tablero directamente, sin calcular daño.";
                case "charming-mongoose":
                    return "Al desplegarla en Bosque, roba 1 carta. Si la despliegas en otro bioma permitido, no robas.";
                case "sandy-anthill":
                    return "Al desplegar esta estructura, elige una casilla libre adyacente e invoca un Battle Ant Token.";
                case "battle-ant-token":
                    return "Esta ficha gana +1 ATK si hay otra hormiga viva a 3 casillas de radio. El bono se aplica una sola vez, aunque haya varias hormigas cerca.";
                case "engage":
                    return "Elige una unidad aliada despierta y luego una unidad enemiga a 6 casillas de radio de ella.\n\nTu aliada inflige daño directo igual a su ATK. Si elimina al objetivo y puede ocupar esa casilla, avanza hasta ella. El terreno inestable puede impedir la acción.";
                case "cleansing-conquest":
                    return "Elige una casilla libre que puedas terraformar y que no sea una base. Luego elige una unidad aliada despierta a 8 casillas de radio de ese destino.\n\nLa casilla se vuelve Yermo. Tu unidad intenta moverse allí y se aplican las tiradas de terreno inestable.";
                case "clearing-space":
                    return "Elige una casilla que puedas terraformar. Conviértela en Yermo y después roba 2 cartas.";
                case "terraform":
                    return "Elige una casilla que puedas terraformar y el bioma que quieres crear. No puedes escoger Neutro ni Tierra Ceniza.\n\nDespués de terraformar, roba 1 carta.";
                case "time-to-move":
                    return "Elige una casilla como centro. Tus unidades y las rivales a 8 casillas de radio reciben +4 MOV hasta el final del turno actual. Las unidades dormidas no ganan movimiento disponible mientras están dormidas.";
                case "bomb":
                    return "Elige una estructura aliada o enemiga. Inflígele 3 puntos de daño directo.";
                case "from-the-ashes":
                    return "Elige una casilla con Yermo. Se elimina su bioma usando las reglas de destrucción de terreno. Puede quedar neutra o revelar Tierra Ceniza si estaba marcada.";
                case "last-gift":
                    return "Elige una unidad aliada y elimínala. Roba tantas cartas como puntos de vida tenía la unidad justo antes de eliminarla. Se usa su vida actual.";
                case "one-for-the-team":
                    return "Necesitas otra carta en la mano para jugar esta magia. Elige una carta de tu mano, descártala y luego roba 2.";
                case "a-price-to-pay":
                    return "Primero roba 3 cartas. Después elige y descarta 2 cartas de tu mano, incluyendo las que acabas de robar si quieres. Si quedan menos de 2, descarta las disponibles.";
                case "resourceful-replenish":
                    return "Cuenta las fichas propias que están en el tablero al resolver la magia. Roba 1 carta por cada una.";
                case "marcha-micelial":
                    return "Elige una unidad aliada y luego un destino libre conectado a ella por una Vía Rápida propia. Puedes repetir el proceso con una segunda unidad.\n\nEl traslado no usa su MOV normal. Se aplican las tiradas de terreno inestable.";
                case "explosion-de-esporas":
                    return "Elige hasta 3 objetivos enemigos adyacentes a tus piezas. Por cada objetivo seleccionado gastas 1 espora e infliges 1 punto de daño.";
                case "espora-somnifera":
                    return "Elige una unidad enemiga que esté en Bosque o Pantano. Queda Dormida y pierde su movimiento disponible. No puede moverse ni atacar durante su próximo turno.";
                case "sueno-micelial":
                    return "Elige una unidad enemiga. Queda Dormida. Si ya estaba envenenada al resolver la magia, también recibe 1 punto de daño.";
                case "nube-de-esporas":
                    return "Elige hasta 2 unidades enemigas que estén en Bosque. Aplica veneno a cada una. El veneno causa daño al inicio del turno de su dueño y no acumula aplicaciones.";
                case "old-mummy":
                    return "Al inicio de tu turno, si está en Desierto, se cura hasta su vida máxima y recibe +1 MOV para ese turno. El bono temporal no se suma al de turnos anteriores.";
                case "sandy-ember":
                    return "Al desplegarla, elige cualquier estructura en Desierto, tuya o rival, e inflígele 1 punto de daño. Si no hay estructuras en Desierto, elige y descarta 1 carta de tu mano.";
                case "dune-cammel":
                    return "Al moverse, puedes trasladar una unidad vecina al origen en la misma dirección, si su destino es válido.\n\nSi el camello pasa de Desierto a otro bioma, las piezas aliadas a 1 casilla de radio del origen ganan +1 HP y +1 ATK hasta tu próximo turno.";
                case "baby-phoenix":
                    return "Al morir, elige gratis el bioma de la casilla que ocupaba si es posible terraformarla. Su carta vuelve a tu mano.\n\nSolo puede atacar en el turno en que entra si fue desplegado en Yermo.";
                case "wandering-beast":
                    return "Despliégala en Yermo. Una vez por turno puede infligir 2 puntos de daño directo a una unidad o estructura a 3 casillas de radio. Al morir, roba 4 cartas.";
                case "panther-stealthy-assassin":
                    return "Cuando sale de Bosque hacia otro bioma, puedes infligir 2 puntos de daño directo a una unidad a 2 casillas de radio del destino.\n\nCuando entra en Bosque desde otro bioma, se cura hasta su vida máxima.";
                case "ferret-eluding-hunter":
                    return "Cuando elimina una unidad, se cura hasta su vida máxima.\n\nUna vez por turno puede saltar a una casilla libre junto a una estructura aliada en Bosque o con etiqueta Bosque situada a 4 casillas de radio.";
                case "bober-reinforcer":
                    return "Al inicio de tu turno, cura 1 HP a todas tus estructuras. Si el castor está en Bosque, puedes construir hasta 2 casillas de radio de él respetando el terreno y la ocupación.";
                case "berry-bush":
                    return "Al inicio de tu turno, elige una casilla libre adyacente para invocar un Rabbit Token.";
                case "rabbit-token":
                    return "Es una ficha invocada. Mientras esté en Bosque tiene +2 HP máximos. Este bono propio de la ficha no se acumula. No forma parte del mazo ni va al descarte.";
                case "frogpit":
                    return "Al desplegarla, elige una casilla libre adyacente e invoca allí un Frog Token.";
                case "micelium-root":
                    return "Al desplegarla, elige una casilla libre adyacente e invoca allí un Mushroom Token.";
                case "firefly-nest":
                    return "Al desplegarla, elige hasta 2 casillas libres adyacentes. Invoca un Firefly Token en cada casilla disponible, de uno en uno.";
                case "frog-token":
                    return "Al atacar, envenena a la unidad objetivo. En Pantano recibe +2 MOV. El bono se pierde al dejar ese bioma.";
                case "mushroom-token":
                    return "Una vez por turno, elige y envenena una unidad a 2 casillas de radio. Suma 1 casilla de alcance por cada otro Mushroom Token propio en el tablero.";
                case "firefly-token":
                    return "Elige una unidad a 2 casillas de radio y sacrifica esta ficha para envenenarla. Si su dueño es Zukgrok, su muerte puede activar la terraformación gratuita de su casilla.";
                case "alligator-revengeful-bite":
                    return "Gana +1 HP máximo y +1 ATK por cada ficha propia en el tablero. Cuando muere una unidad que no es ficha, se cura 1 HP.\n\nSi él elimina una unidad que no es ficha, puedes invocar un Alligator Token.";
                case "lushroom":
                    return "Mientras Lushroom esté en Pantano, las unidades rivales envenenadas reciben 1 punto adicional de daño por veneno. Si ellas también están en Pantano, no pueden curarse.";
                case "under-the-darkness":
                    return "Elige una unidad del descarte y una casilla donde puedas desplegarla. Volver a jugarla no cobra su coste base, pero sí el recargo de Eternal Eye si corresponde.\n\nAl final de cada turno esa unidad muere si no está en Void.";
                case "void-beacon":
                    return "Cuando muere una unidad aliada en Void, puedes sacrificar un Void Beacon para volver a jugarla en la casilla del Beacon.\n\nPaga el coste original +2, o difiere ese pago hasta el final de tu turno. Si entonces no lo pagas, la unidad muere.";
                case "portal":
                    return "Una vez por turno, paga 2 de energía y elige una unidad despierta en Void a 6 casillas de radio del Portal. Luego elige una casilla válida libre junto al Portal e intenta trasladarla allí.";
                case "insolator":
                    return "Activa la habilidad para infligir 2 puntos de daño directo a todas las unidades en un radio de 3 casillas, incluidas tus aliadas. Si mata al menos una, puedes terraformar una casilla válida a Void.";
                case "crystal-minion":
                    return "Si empieza en Void y tiene movimiento disponible, puede saltar a otra casilla de Void libre y válida a 3 casillas de radio. El salto usa 1 MOV.";
                case "dark-fly":
                    return "Al moverse, se cura hasta su vida máxima. Tras su primer ataque puede atacar una segunda vez si no se ha movido y aún tiene MOV disponible. Ese segundo ataque consume todo su movimiento restante.";
                case "the-eternal-eye":
                    return "En Void no puede ser objetivo de cartas ni efectos. Cada carta rival cuesta 1 energía más y sus rivales pueden terraformar la casilla de Eye desde cualquier lugar.\n\nPaga 1 de energía para moverte a otra casilla válida de Void, sin límite de usos. Eye no puede curarse.";
                case "void-hole":
                    return "Si otra unidad aliada a 5 casillas de radio va a recibir daño, puedes hacer que Void Hole reciba 1 punto de daño para impedir ese impacto. Solo una vez por turno. Void Hole no puede curarse.";
                case "sprinting-sprite":
                    return "Mientras esté en Void, paga 1 de energía, elige otra unidad despierta que pueda ocupar su casilla y sacrifica a Sprinting Sprite. Luego la unidad elegida intenta trasladarse a esa casilla.";
                case "med-camp":
                    return "Una vez por turno, elige entre 1 y 4 unidades aliadas a 5 casillas de radio. Cada una recupera hasta 4 HP sin superar su vida máxima.";
                case "resting-camp":
                    return "Una vez por turno, todas las unidades aliadas a 4 casillas de radio recuperan hasta 2 HP sin superar su vida máxima.";
                case "vigilant-tower":
                    return "Una vez por turno, elige una unidad a 6 casillas de radio. Inflígele 1 punto de daño.";
                case "emergency-seal":
                    return "Cuando se destruye esta estructura, puedes elegir una unidad aliada e intentar trasladarla a la casilla que acaba de quedar libre.";
                case "mana-pool":
                    return "Al inicio de tu turno, obtén 1 energía adicional. El límite permanente de energía no sube.";
                case "valiant-seal":
                    return "Mientras esta estructura esté en juego, tus unidades ganan +1 ATK. Pierden ese bono si la estructura deja el tablero.";
                case "courage-seal":
                    return "Mientras esta estructura esté en juego, tus unidades ganan +1 MOV. Pierden ese bono si la estructura deja el tablero.";
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

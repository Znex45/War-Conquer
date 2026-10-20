# War & Conquer — Graybox de Unity

## Jugar

Unity 6000.6.2f1 → War & Conquer → Abrir Graybox → Play.
Elige 1 persona contra IA o 2, 3, 4 personas locales y un mazo de 50 cartas para cada participante. La preparación oculta el tablero y la interfaz y mantiene la partida en pausa hasta COMENZAR PARTIDA.

SAHRIA usa amarillo, ZUKGROK morado, FAUNAR verde y XIL'THAR cian. Los números J1–J4 distinguen participantes con el mismo mazo. El juego conserva tablero, unidades y estructuras 3D, interfaz oscura, acciones claras, mano paginada y cartas no disponibles oscurecidas.

La preparación ya no muestra el botón Cambiar semilla. Los estados de cartas tienen símbolos animados: veneno, sueño, ralentización, mejoras de ATK/HP/MOV, protección, evolución, crecimiento, bloqueo de curación y terreno inestable. Curación y daño muestran partículas y su cambio de HP. Cada punto de conquista ganado reproduce una breve fanfarria de trompeta; las actualizaciones de interfaz y la carga de una partida no repiten puntos antiguos.

## Mazos actuales

| Mazo | Spells | Combat Spells | Structures | Units | Total |
|---|---:|---:|---:|---:|---:|
| SAHRIA | 16 | 8 | 13 | 13 | 50 |
| ZUKGROK | 19 | 8 | 18 | 5 | 50 |
| FAUNAR | 15 | 5 | 18 | 12 | 50 |
| XIL'THAR | 13 | 6 | 18 | 13 | 50 |

La composición oficial está en `Assets/WarConquer/Resources/WarConquer/cards.json`, campo `deckCopies`. Un diseño compartido tiene una sola definición de estadísticas y efectos, con copias por Líder. Los Tokens están fuera de los mazos y nunca se descartan. Los diseños antiguos se conservan para compatibilidad de efectos y pruebas históricas; no se añaden a los mazos actuales.

Cleansing Conquest fue eliminada del mazo de Zukgrok. Alligator, Revengeful Bite fue aumentado a 3 copias.

## Turnos y controles

- Despliegue → Asalto → Terraformación. Finalizar turno resuelve estados, energía, robo y producción del siguiente jugador.
- Rewind / Deshacer revierte decisiones del turno actual, incluidos los cambios de etapa. Los efectos y elecciones de una misma acción se revierten juntos. El historial se cierra al finalizar el turno, cargar o iniciar otra partida. Repetir una acción deshecha conserva su secuencia aleatoria.
- Carta → casillas resaltadas. Los objetivos múltiples se confirman al completar la selección o con Resolver selección. Cancelar no consume energía.
- La carta ampliada muestra coste real, MOV / HP / ATK, subtipos, Líder, biomas, restricciones y ventanas de uso.
- Unit propia → mover, atacar o habilidad. RAD usa distancia hexagonal, independiente de MOV y rutas rápidas.
- Solo energía / Pago mixto permite pagar con recursos compatibles. `EnergyManager.Quote` calcula tanto la presentación como el pago.
- Menú reúne pausa, nueva partida, guardar, cargar, registro y ayuda. La IA se detiene mientras hay un diálogo abierto.
- Arrastrar desplaza la cámara; botón derecho gira; rueda acerca. Ver datos y Detalles recuperan información adicional.
- Las pilas abren mazo y descarte. El mazo no muestra su orden de robo.
- Escenario de pruebas prepara cartas y piezas de los mazos actuales conservando sus 50 instancias.

## Líderes

**SAHRIA:** activa Usar SAHRIA +1 E antes de elegir una Unit. Permite desplegar en una casilla que haya establecido, aunque tenga otro bioma, cobrando realmente 1 Energía adicional. Se conservan la ocupación, las bases y las restricciones explícitas: Wandering Beast solo puede jugarse en Yermo. El coste actualizado aparece en todas las vistas de la carta. `HexTile.setBy` registra quién terraformó el Slab; moverse o conquistarlo no falsifica esa procedencia. Las siete casillas iniciales cuentan como establecidas por su jugador.

**ZUKGROK:** al morir un Token propio, el jugador elige el bioma de la casilla donde murió y terraforma por 0 Energía. Se activa con combate, daño, Poison y sacrificio. Nunca se activa por una Unit normal. No modifica ceniza permanente, casillas bloqueadas, ocupantes enemigos ni bases enemigas vivas.

**FAUNAR:** la primera Structure de su turno cuesta 1 Energía menos, mínimo 0. AVAILABLE / USED indica el estado. Se reinicia al comienzo del siguiente turno propio.

**XIL'THAR:** cada cuatro terraformaciones realmente resueltas durante su turno genera +1 Energía, sin límite de grupos completos. El contador y la energía generada aparecen fuera de la mano y se reinician al inicio del siguiente turno propio. Solo cambiar el bioma mediante terraformación cuenta; repetir el mismo bioma, mover, conquistar o desplegar no cuenta. Void es un bioma del tablero existente, distinto de Yermo.

## Cartas de Xil'thar

`XiltharRules` conecta sus efectos a los gestores existentes. Under the Darkness elige una Unit real del descarte y un destino legal; conserva su identidad y un efecto persistente que la mata al final de cualquier turno si está fuera de Void. Void Beacon se ofrece al morir una Unit propia no Token en Void: destruir el Beacon recupera esa misma carta en su Slab, pagando coste original +2 o aceptando la muerte al final del turno propio. La opción diferida permite pagar entonces para conservarla. Los recargos del Eye se muestran y se pagan al recastear.

Portal cuesta 2 Energía una vez por turno: primero se elige una Unit en Void a 6rad y después un destino adyacente libre. Insolator usa la convención existente de una activación por turno: hace 2 daño a todas las Units a 3rad; espera a resolver las prevenciones y, solo si provocó alguna muerte, permite terraformar un Slab válido a Void.

Crystal Minion puede gastar 1 MOV para saltar de Void a Void a 3rad. Dark Fly se cura con el sistema existente al moverse; su segundo ataque reemplaza todo su movimiento y bloquea nuevos desplazamientos ese turno. The Eternal Eye puede repetir saltos Void → Void pagando 1 cada vez; en Void impide seleccionarlo mediante cartas o efectos y aumenta en 1 el coste de las cartas enemigas. El rival puede terraformar su Slab desde cualquier ubicación. Eye y Void Hole no pueden curarse.

Void Hole ofrece una elección antes de aplicar un impacto a una Unit aliada a 5rad (incluyéndose): recibir 1 daño para prevenirlo, una vez por turno global, u omitir y resolver el daño original. Sprinting Sprite en Void puede pagar 1 y morir mediante el sistema real de muerte para mover otra Unit a su casilla, siempre que sea un destino legal.

Las elecciones, recargos, contadores y efectos persistentes son parte del estado serializado. Deshacer restaura todos los efectos de la acción juntos, incluidas sus elecciones. Confirmar el fin del turno cierra el historial aunque queden pagos de Beacon pendientes. Las cartas compartidas conservan las reglas existentes, incluido Valiant Seal y su aura global de ataque.

## Efectos de cartas

Poison inflige **1 daño al inicio de cada turno de su controlador**. Reaplicar no multiplica ni acumula daño. Un Lushroom enemigo en Pantano aumenta ese daño a 2; varios no lo acumulan. Mientras existe esa aura, una Unit enemiga envenenada sobre Pantano no puede curarse. Calavera morada, pulso al aplicar y rótulo de daño muestran el estado real. Sueño conserva burbujas, Zzz y bloqueo de movimiento, ataque y habilidades.

Sandy Anthill, Frogpit, Micelium Root y Firefly Nest invocan al entrar; se eligen los Slabs adyacentes libres. Firefly Nest invoca dos cuando hay espacio. Sus vidas confirmadas son **3, 4, 3, 3**, con costes **2, 3, 3, 3** respectivamente.

Sandy Ember permite elegir cualquier Structure en Desierto; si no existe ninguna, se elige el descarte. Dune Cammel permite acompañar cada desplazamiento en la misma dirección, sin cadenas recursivas; al pasar de Desierto a otro bioma mejora aliados a 1rad del origen hasta su siguiente turno. Old Mummy se cura al inicio en Desierto y gana 1 MOV de ese turno. Wandering Beast tiene habilidad de 2 daño a 3rad y roba 4 al morir. Baby Phoenix terraforma su casilla de muerte antes de devolver la misma carta a mano; solo ataca el turno en que se juega si entró en Yermo.

Battle Ant obtiene un único +1 ATK si hay otra Ant a 3rad. Frog envenena al atacar y gana 2 MOV en Pantano. Mushroom envenena a 2rad +1 por cada otro Mushroom Token propio. Firefly puede sacrificarse, resolver la terraformación de Zukgrok y después envenenar su objetivo a 2rad. Alligator gana HP y ATK dinámicos por Token propio, cura 1 cuando muere una Unit no Token y al matar una puede invocar un Alligator Token. Este Token gana 3 MOV en Pantano.

**Pendiente indicado por la especificación:** el efecto final del Alligator Token está incompleto y requiere definición. «When I die, give +1hp» no tiene destinatario ni duración y no se ha inventado un efecto.

## Tablero y victoria

Los mapas conservan 223, 331 y 459 casillas para duelo, tres y cuatro participantes. Son continuos y simétricos, sin puentes. Cada base consta de siete casillas. Hay tres objetivos centrales separados. En tres jugadores están al menos a siete pasos de todas las bases principales.

Cada objetivo y cada base enemiga derrotada concede +1 PC al volver el turno del controlador si mantuvo una Unit propia sobre la casilla y un bioma terraformado. Perder la guarnición o el bioma reinicia la espera. Gana quien alcanza 10 PC o conserva el último Líder.

## Arquitectura y guardado

El código está en `Assets/WarConquer/Runtime`. Se reutilizan GameManager, BoardManager, TerrainManager, CombatManager, MovementManager, EffectManager, AbilityManager, TurnManager, DeckManager, EnergyManager, BattleManager, ChoiceManager, ConquestManager y GamePersistence.

`FactionCardRules` integra los efectos de Sahria y Zukgrok sobre esos sistemas. No crea otro tablero ni otro motor de combate. Las elecciones serializables guardan invocaciones, descartes, acompañamiento, bioma de muerte, Phoenix en tránsito y Poison posterior al sacrificio. El validador contabiliza la carta del Phoenix pendiente para no perderla ni duplicarla.

`GameState.version` es **7**. Los guardados anteriores permanecen en disco, pero requieren comenzar una partida con los mazos actualizados. La escena original, URP, mapas y ajustes del equipo permanecen conservados.

## Pruebas

Unity → War & Conquer → Ejecutar pruebas de reglas. La consola muestra PASS y WAR_CONQUER_TESTS_PASSED. Las pruebas históricas de cartas anteriores usan exclusivamente la fixture de Editor; las de Sahria, Zukgrok y Faunar usan el catálogo actual.

Unity → War & Conquer → Comprobar y mostrar Graybox 3D verifica clics reales, selección de mazos, costes, invocaciones, sacrificio, elección de bioma, cartas, estados, modelos y dados. Las capturas se guardan en `Library/WarConquer3D`. Estas comprobaciones explícitas preparan un escenario y terminan en la pantalla de selección de partida.

El informe de esta actualización está en `Documentation/Informe_Actualizacion_Sahria_Zukgrok.md`.

### Validación de Xil'thar (13 de octubre de 2026)

182 comprobaciones de regresión aprobadas, más una comprobación adicional del turno de IA de Xil'thar. Su batería específica completa aprueba 25 pruebas (24 incluidas en la regresión). La interfaz y el tablero se comprobaron también en el Unity abierto del proyecto: selección para cuatro jugadores, recast con coste alternativo, descarte, Portal, Beacon, prevención, etapas, Rewind, modelos, dados y sonido. Capturas: `Library/WarConquer3D/xilthar-seleccion.png` y `xilthar-partida.png`. Sin errores de compilación ni fallos de estas pruebas.

Archivos de esta integración, dentro de `Assets/WarConquer`:

- Datos: `Resources/WarConquer/cards.json`, `Runtime/CardCatalog.cs`, `Runtime/Model.cs`.
- Reglas nuevas: `Runtime/XiltharRules.cs`.
- Conexiones a reglas existentes: `Runtime/AbilityManager.cs`, `AiPlayer.cs`, `ChoiceManager.cs`, `CombatManager.cs`, `EffectManager.cs`, `EnergyManager.cs`, `GameManager.cs`, `GameManager.History.cs`, `GenericCardRules.cs`, `MovementManager.cs`, `PrototypeScenario.cs`, `TerrainManager.cs`, `TimingRules.cs`, `TurnManager.cs`.
- Interfaz: `Runtime/UI/WarConquerController.Xilthar.cs`, `WarConquerController.cs`, `WarConquerController.Choices.cs`, `WarConquerController.Dialogs.cs`, `WarConquerController.Status.cs`, `WarConquerController.Turn.cs`, `CardPresentation.cs`, `GrayboxUI.cs`, `PieceStatusPresentation.cs`, `BoardStatusVisual.cs`.
- Pruebas: `Editor/XiltharRulesTests.cs`, `XiltharPlayTests.cs`, `XiltharVisualPreview.cs`, `GrayboxTests.cs`, `Board3DPlayTests.cs`, `FeedbackVisualPreview.cs`; además de los `.meta` de archivos nuevos y este README.

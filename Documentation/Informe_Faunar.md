# Informe de integración de Faunar

Integración aplicada al proyecto existente War-Conquer. Estado de partida versión 6.

## 1 Archivos modificados

- `Assets/WarConquer/Editor/Board3DPlayTests.cs`
- `Assets/WarConquer/Editor/GrayboxTests.cs`
- `Assets/WarConquer/Editor/InterfacePlayModeTests.cs`
- `Assets/WarConquer/Editor/InterfaceRulesTests.cs`
- `Assets/WarConquer/Editor/MatchSetupTests.cs`
- `Assets/WarConquer/Editor/RevisionRulesTests.cs`
- `Assets/WarConquer/Editor/RevisionVisualPreview.cs`
- `Assets/WarConquer/Resources/WarConquer/cards.json`
- `Assets/WarConquer/Runtime/AbilityManager.cs`
- `Assets/WarConquer/Runtime/AiPlayer.cs`
- `Assets/WarConquer/Runtime/BattleManager.cs`
- `Assets/WarConquer/Runtime/BoardManager.cs`
- `Assets/WarConquer/Runtime/CardCatalog.cs`
- `Assets/WarConquer/Runtime/CombatManager.cs`
- `Assets/WarConquer/Runtime/EffectManager.cs`
- `Assets/WarConquer/Runtime/EnergyManager.cs`
- `Assets/WarConquer/Runtime/GameManager.cs`
- `Assets/WarConquer/Runtime/Model.cs`
- `Assets/WarConquer/Runtime/MovementManager.cs`
- `Assets/WarConquer/Runtime/PrototypeScenario.cs`
- `Assets/WarConquer/Runtime/TerrainManager.cs`
- `Assets/WarConquer/Runtime/TimingRules.cs`
- `Assets/WarConquer/Runtime/TurnManager.cs`
- `Assets/WarConquer/Runtime/UI/BoardMeshFactory.cs`
- `Assets/WarConquer/Runtime/UI/CardPresentation.cs`
- `Assets/WarConquer/Runtime/UI/GrayboxUI.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.Cards.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.Dialogs.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.Status.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.Turn.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.cs`
- `README_WAR_CONQUER.md`

## 2 Archivos creados

- `Assets/WarConquer/Editor/FaunarPlayTests.cs`
- `Assets/WarConquer/Editor/FaunarRulesTests.cs`
- `Assets/WarConquer/Editor/FaunarVisualPreview.cs`
- `Assets/WarConquer/Runtime/ChoiceManager.cs`
- `Assets/WarConquer/Runtime/GenericCardRules.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.Choices.cs`

Se generaron también los archivos .meta correspondientes y los documentos `Como_se_construyo_War_Conquer.docx` e `Informe_Faunar.md` en Documentation.

## 3 Sistemas reutilizados

GameState, HexTile, CardCatalog, DeckManager, EnergyManager, GameManager, TerrainManager, MovementManager, CombatManager, TurnManager, AbilityManager, BattleManager, ConquestManager y la interfaz Canvas con tablero 3D. Los mazos de Zukgrok y Sahria conservan sus cartas y cantidades.

## 4 Extensiones nuevas

GenericCardRules agrega operaciones de cartas al motor existente. ChoiceManager mantiene elecciones pendientes, sus propietarios y sus objetivos; el controlador presenta esas elecciones y AiPlayer resuelve las que le corresponden. No se creó otro tablero, otra energía ni otro sistema de biomas.

## 5 Cartas genéricas

Terraform, Clearing Space, Cleansing Conquest, It's a Trap, One for the Team, A Price to Pay, From the Ashes, Resourceful Replenish, Last Gift, Engage, Time to Move, Bomb, Valiant Seal, Courage Seal, Med Camp, Resting Camp, Vigilant Tower, Emergency Seal y Mana Pool. Las cinco genéricas no incluidas en la lista cerrada de Faunar tienen cantidad cero; sus efectos están implementados y probados, sin introducir copias extra en los mazos.

## 6 Faunar y sus cartas

Faunar se elige en la preparación de cualquier puesto humano o IA. Berry Bush crea Rabbit Token; Charming Mongoose roba al jugarse en Bosque; Ferret cura al matar y dispone de salto especial; Bober cura estructuras y amplía la colocación mientras está en Bosque; Panther permite elegir el daño al abandonar Bosque y cura al entrar desde otro bioma.

## 7 Slabs

Slab corresponde al HexTile existente. Se conserva la retícula y sus mapas de 223, 331 y 459 casillas, sus bases de siete casillas y los tres objetivos de conquista.

## 8 Radios

SlabsWithinRadius, UnitsWithinRadius y StructuresWithinRadius reutilizan AxialDistance. Los radios 2, 3, 4, 5, 6 y 8 son distancia hexagonal, separados del movimiento disponible. Se respetan propietario, ocupación y condiciones de cada efecto.

## 9 Yermo

Yield utiliza Biome.Wasteland. Clearing Space y Cleansing Conquest convierten el Slab en Yermo; From the Ashes selecciona solamente Yermo y llama a la destrucción existente, incluidas sus reglas de ceniza oculta. No se creó un recurso Yield.

## 10 Bosque

Forest utiliza Biome.Forest. Las condiciones consultan el bioma del Slab real. Las transiciones de Panther comparan origen y destino; un movimiento entre dos Bosques no dispara salida ni curación. Bober pierde la extensión al dejar Bosque.

## 11 Tokens

Rabbit no entra a mano, mazo ni descarte. Es una unidad con MOV 2, HP 2 y ATK 1; en Bosque su máximo de HP es 4. El ajuste conserva el daño y no acumula el bono. Puede moverse, atacar, recibir daño y morir.

## 12 Descuento de Faunar

EnergyManager.Quote calcula el mismo coste que paga EnergyManager.Pay. La primera estructura cuesta 1 menos, con mínimo cero. El indicador firstStructureUsed cambia solo tras un pago válido de estructura y la interfaz muestra AVAILABLE o USED. No reduce Units ni Spells.

## 13 Inicio del turno

TurnManager.Start reinicia el descuento y los usos de habilidades y ejecuta Berry Bush, Bober y Mana Pool. lastStartedTurn evita repetir el mismo inicio. Mana Pool añade energía disponible sin incrementar permanentemente maxEnergy.

## 14 Efectos una vez por turno

Las habilidades usan el estado de cada pieza. Dos Ferret no comparten disponibilidad. Los sellos son auras recalculadas; Time to Move concede +4 MOV a las unidades de todos los bandos dentro de 8rad y expira al final del turno actual.

## 15 Pruebas

La suite completa de Unity aprobó 116 casos: 93 de regresión y 23 de Faunar. Se comprobaron los conteos, costes, radios, robo y descarte, tipos de objetivos, efectos, fichas, ventanas de combate, selección de terceros, guardado y decisiones de IA. Las pruebas de Play del Canvas aprobaron selección de tres mazos, ficha del Líder, descuento, descarte elegido, Bomb, Time to Move y posiciones MOV/ATK. La comprobación se ejecutó también en el Unity del proyecto instalado y generó las capturas faunar-seleccion, faunar-partida y faunar-descarte en Library/WarConquer3D.

## 16 Incidencias encontradas

Las pruebas antiguas esperaban el orden anterior de etapas y la etiqueta FUERZA en las cartas. Una preparación de prueba contaba la extracción manual desde el mazo como si fuera parte del robo del efecto. La primera captura de GameView tenía resolución insuficiente. En una ejecución de validación apareció una excepción interna del índice de búsqueda de Unity Editor; su pila pertenece a UnityEditor.Search, no a los scripts del juego.

## 17 Correcciones

Se adaptaron las pruebas al orden solicitado Despliegue, Terraformación y Asalto y a MOV/HP/ATK. Se corrigió la preparación de la prueba de robo. La captura espera una resolución legible; las imágenes finales del editor instalado son de 1920 × 888. Se conservó la escena original y su cámara URP, retirando la regeneración producida por la prueba de compilación.

## 18 Criterios provisionales y continuidad

Los datos no especificados usan criterios explícitos de graybox: Berry Bush tiene 3 HP; “Heal me” restaura hasta el máximo; el Rabbit aparece en el primer vecino libre y legal; Med Camp, Resting Camp y Vigilant Tower se activan una vez por turno en Asalto. Se considera Forest Structure la estructura aliada etiquetada BOSQUE o situada en Bosque. Panther mide su radio desde el destino del movimiento. Cleansing Conquest toma un Slab y una unidad aliada dentro del radio 8 del destino. Estas decisiones están descritas también en README_WAR_CONQUER.md. Quedan como evolución del producto el balance con partidas humanas y el arte definitivo; las funcionalidades solicitadas cuentan con implementación y comprobaciones.

## 19 Lista exacta de Faunar

| Carta | Copias |
|---|---:|
| Terraform | 3 |
| It's a Trap | 3 |
| One for the Team | 3 |
| A Price to Pay | 1 |
| Resourceful Replenish | 3 |
| Last Gift | 2 |
| Time to Move | 2 |
| Bomb | 3 |
| Valiant Seal | 3 |
| Courage Seal | 2 |
| Resting Camp | 3 |
| Vigilant Tower | 3 |
| Emergency Seal | 1 |
| Mana Pool | 3 |
| Berry Bush | 3 |
| Charming Mongoose | 3 |
| Ferret, Eluding Hunter | 3 |
| Bober, Reinforcer | 3 |
| Panther, Stealthy Assassin | 3 |

**Total: 50 / 50.** Spells 15, Combat Spells 5, Structures 18, Units 12. Máximo tres copias, sin Slashing Bear ni cartas adicionales en el mazo.

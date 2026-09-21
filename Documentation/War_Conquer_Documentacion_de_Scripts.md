# War Conquer documentación de los scripts

Guía técnica del graybox de Unity. Actualización del 21 de septiembre de 2026.

Este documento explica la responsabilidad de cada script de `Assets/WarConquer`, sus funciones principales y los cambios de interfaz, juego y representación 3D incorporados al prototipo. Está dirigido a los dos desarrolladores del proyecto para localizar reglas y modificar un sistema sin desajustar los demás. Describe el código actual, incluidos los mapas nuevos, la conquista con guarnición, los dados y los estados; no atribuye modificaciones al resto de assets del repositorio.

## Flujo de una partida

1. El controlador carga `cards.json` y `rules.json` y muestra preparación en pausa.
2. El jugador elige participantes y mazos. `GameManager.NewGame` crea el mapa correspondiente y los mazos independientes de 50 cartas.
3. `TurnManager` abre Despliegue, seguido de Ataque y Terraformación. `TimingRules` controla las ventanas legales.
4. El HUD consulta objetivos y costes; `GameManager` valida la acción completa antes de modificar la partida.
5. Los gestores de movimiento, combate, terreno y efectos ejecutan la regla. `Notify` actualiza guarniciones y reconstruye los controles necesarios.
6. `Board3DScene` sincroniza modelos. `BoardDiceVisual` presenta las tiradas ya calculadas y `BoardStatusVisual` muestra veneno, sueño y disponibilidad de ataque.
7. Al regresar el turno de un jugador, `ConquestManager` puntúa las posiciones que mantuvo. Después se aplican veneno, energía, producción y robo.

## Reglas vigentes de esta revisión

| Sistema | Comportamiento |
|---|---|
| Una persona o dos personas | Óvalo simétrico de 223 casillas; en modo individual J2 es IA |
| Tres personas | Hexágono regular de 331 casillas; bases por rotaciones de 120 grados |
| Cuatro personas | Hueso simétrico de 459 casillas; cuatro bases equivalentes en los extremos |
| Base inicial | Siete casillas neutras propias, centro más seis vecinas |
| Conquista | Tres objetivos centrales separados; cada uno puede dar un PC por turno propio |
| Condición de puntuación | Unidad propia presente y casilla terraformada, mantenidas hasta el siguiente turno propio |
| Base enemiga | Solo puntúa tras derrotar al Líder y mantener las mismas dos condiciones |
| Recaptura | Reinicia la espera; no otorga puntos instantáneos acumulables |
| Veneno | Daño directo 1, 2, 4, 8 y sucesivos al inicio de turnos propios hasta morir |
| Sueño | Bloquea movimiento, ataques y habilidades hasta terminar el próximo turno propio |
| Dados | Resultado único del motor, rebote 3D y cara final coherente; acciones pausadas durante la animación |
| Magias | Objetivos según texto y bioma; sin alcance artificial cuando la carta no lo define |
| Victoria | Diez PC o último Líder vivo |

La igualdad de los mapas se verifica por geometría, distancias a los tres objetivos y cantidad de casillas a cada distancia desde las bases. Esto no implica que los dos mazos tengan un balance competitivo idéntico. Los puntos se comprueban antes del veneno del comienzo del turno. El orden de asientos sigue siendo J1 a J4, omitiendo los puestos vacíos o eliminados.

## Scripts del motor

### Estado y datos del juego

**Archivo:** `Assets/WarConquer/Runtime/Model.cs`

Define las enumeraciones de biomas, categorías, etapas, ventanas y victoria. CardData representa un diseño de carta; CardInstance identifica cada copia. Piece guarda dueño, casilla, vida, movimiento, ataques, habilidades y estados. HexTile contiene las coordenadas axiales, vecinos, bioma, base, ocupantes y control. Player conserva mazo, mano, descarte, energía, recursos, Esporas y puntos.

GameState reúne la partida serializable, semilla y generador aleatorio reproducible. La versión 5 incorpora mapPlayers, tres objetivos identificados con conquestSite, las fechas de guarnición y el historial DiceRoll. poison almacena el siguiente daño; poisonApplications distingue nuevas aplicaciones para la animación. Los valores numéricos antiguos de TurnStage se conservan para no cambiar el significado del catálogo. Los campos heredados poisonDuration, spellRange y lastScoredRound no gobiernan las reglas actuales.

### Catálogo y mazos

**Archivo:** `Assets/WarConquer/Runtime/CardCatalog.cs`

CardCatalog.Load lee cards.json desde Resources; LoadRules carga rules.json. El constructor comprueba que cada Líder tenga exactamente 50 cartas y que las cantidades respeten el máximo de copias. El índice por identificador evita buscar cartas por su nombre visible.

DeckManager.Build crea instancias independientes y baraja con la semilla de la partida. Draw mueve cartas reales del mazo a la mano. Un mazo vacío deja de robar; no duplica cartas ni añade fatiga. Las fichas de cantidad cero no forman parte de las 50 cartas.

### Generación de los tres mapas

**Archivo:** `Assets/WarConquer/Runtime/BoardManager.cs`

Create selecciona la geometría según los participantes efectivos. Una persona contra IA y dos personas usan el óvalo de 223 hexágonos, formado por la unión de tres hexágonos de radio 6. Tres personas usan un hexágono regular de radio 10 y 331 casillas. Cuatro usan un hueso de 459 casillas, con reflexión horizontal y vertical.

Cada base controla exactamente el centro y sus seis vecinos. Create asigna tres objetivos centrales separados, recursos ocultos simétricos, identificadores contiguos y conexiones únicamente entre hexágonos adyacentes. AxialDistance calcula distancia sobre la retícula; Distance recorre el grafo real. ConnectedBiome sigue biomas propios y vías rápidas. Pieces y Nearby consultan las unidades y estructuras sin mantener listas duplicadas.

### Coordinación y validación de acciones

**Archivo:** `Assets/WarConquer/Runtime/GameManager.cs`

NewGame crea entre dos y cuatro participantes efectivos; solo la opción de una persona añade IA. Los asientos restantes quedan inactivos. El parámetro startImmediately permite preparar la partida sin iniciar turnos. AdvanceStage aplica Despliegue, Ataque y Terraformación de forma explícita; EndTurn solo acepta la etapa final.

CardBlockReason explica por qué no puede jugarse una carta. CardTargets y ValidSpellTarget calculan casillas legales según tipo, dueño, bioma, adyacencia y ventana. Las magias sin alcance explícito no reciben una restricción artificial de tres casillas. Play valida toda la selección antes de pagar, retirar la carta y resolver sus efectos. Place crea la pieza; Terraform y RevealAsh comprueban las condiciones y costes. TerraformTarget rechaza ocupantes enemigos y bases enemigas vivas. RollDie genera una única tirada autoritativa y la registra para su visualización. Notify sincroniza guarniciones y avisa a la interfaz.

### Etapas y permisos

**Archivo:** `Assets/WarConquer/Runtime/TimingRules.cs`

Order establece el orden visible y funcional Despliegue, Ataque y Terraformación. StageName proporciona sus rótulos. CardAllowed comprueba la etapa y la ventana permitidas por cada carta. AbilityStage y LeaderStage asignan la etapa de habilidades según su función.

ResponseAllowed solo permite intervenir cuando existe un permiso explícito en los datos y en el texto de la carta. No convierte automáticamente cualquier magia en una respuesta. Los catálogos sintéticos de las pruebas ejercitan estas ventanas sin alterar los dos mazos reales.

### Inicio y cierre del turno

**Archivo:** `Assets/WarConquer/Runtime/TurnManager.cs`

Start puntúa las guarniciones mantenidas al comenzar el turno de su propietario. Si se alcanza la victoria, termina inmediatamente. Después repone energía, actualiza efectos temporales, reinicia acciones de piezas, aplica veneno, calcula movimiento, genera recursos y producción, roba y abre Despliegue.

El veneno resta 1, 2, 4, 8 y sucesivos puntos de vida en los turnos propios; su siguiente daño se duplica sin desbordar el entero. Una unidad muerta no continúa produciendo. End retira sueño y ralentización al terminar su turno correspondiente, limpia mejoras y avanza saltando asientos inactivos o eliminados. La ronda global ya no concede puntos por sí sola.

### Puntuación y victoria

**Archivo:** `Assets/WarConquer/Runtime/ConquestManager.cs`

EligibleOwner exige que haya una unidad del controlador sobre la casilla y que el bioma no sea Neutro. La casilla debe ser uno de los tres objetivos o la base de un enemigo derrotado. Una estructura sola no sirve como guarnición. Refresh detecta cambios de elegibilidad y reinicia la espera cuando se abandona o pierde la posición.

ScoreStart concede un punto por cada guarnición mantenida desde antes del turno actual. lastConquestTurn impide duplicar el premio, incluso después de guardar y cargar. Recapturar reinicia la espera; no concede puntos inmediatos repetibles. Income muestra cuántas posiciones reúnen actualmente las condiciones. Finish declara victoria a los 10 PC; CheckLastLeader cubre la eliminación de los demás Líderes.

### Coste mostrado y coste cobrado

**Archivo:** `Assets/WarConquer/Runtime/EnergyManager.cs`

Quote devuelve el coste original, descuento, recursos compatibles y energía final. La carta y el pago usan esta misma cotización, evitando diferencias entre lo mostrado y lo consumido. El pago mixto solo usa etiquetas compatibles y requiere selección explícita.

CanPay valida la energía; Pay descuenta una sola vez y consume el descuento de estructura cuando corresponde. AddResource mantiene depósitos separados por bioma y respeta su límite. El terreno Neutro no genera recursos gastables.

### Biomas y terreno inestable

**Archivo:** `Assets/WarConquer/Runtime/TerrainManager.cs`

Terraform incluye una segunda barrera de validación, compartida por la IA, cartas y efectos: no transforma casillas con unidades o estructuras enemigas, bases enemigas vivas, obstáculos o ceniza permanente. Actualiza dueño y bioma, elimina efectos anteriores, conserva o elimina rutas y refresca las guarniciones. DestroyBiome neutraliza el terreno o revela ceniza marcada; RevealAsh cambia el bioma conservando la casilla.

CheckUnstable controla tiradas al entrar y las salidas exigidas por el efecto. CheckAction añade tiradas al atacar o activar habilidades desde terreno afectado. Vuelo, inmunidad y compatibilidad evitan esas tiradas. Un fallo consume el intento y aplica el daño correspondiente; el bonus compartido de Sahria se consume una sola vez. CreateFastRoute y MaintainRoutes gestionan las conexiones de la red micelial.

### Movimiento y captura física

**Archivo:** `Assets/WarConquer/Runtime/MovementManager.cs`

Paths busca destinos alcanzables respetando presupuesto, ocupación, bases, obstáculos, vuelo y vías rápidas. Move recorre la ruta y consulta las tiradas de terreno antes de aterrizar en cada paso. RouteDestinations sirve a Marcha Micelial; FreeStep consume el paso adicional de los efectos que lo conceden.

Relocate vacía primero la casilla de origen y refresca sus condiciones de conquista. Después traslada la pieza, cambia el control, actualiza rutas y ejecuta efectos de entrada. Esta secuencia hace que abandonar y volver a una posición reinicie la espera de puntuación, aunque se haga en el mismo turno.

### Ataque daño y eliminación

**Archivo:** `Assets/WarConquer/Runtime/CombatManager.cs`

Targets permite ataques según etapa, propietario, alcance, uso previo y sueño. Attack comprueba el terreno del atacante y abre la batalla. ResolvePending vuelve a validar al atacante y su objetivo antes de aplicar daño; si el atacante desapareció o quedó dormido, el golpe puede cancelarse.

AttackValue y Damage calculan mejoras, auras y defensas. PoisonDamage descuenta vida directamente para que las defensas no anulen la progresión del veneno. Remove retira piezas, descarta cartas reales y actualiza guarniciones. Eliminate derrota al Líder y permite conquistar su base; la victoria por último Líder se comprueba por separado.

### Respuestas durante el combate

**Archivo:** `Assets/WarConquer/Runtime/BattleManager.cs`

Open identifica atacante, defensor y terceros y organiza su prioridad. SeekPriority busca respuestas realmente legales; si nadie puede responder, el daño se resuelve directamente. Pass cede prioridad y AfterResponse continúa la secuencia tras una carta o habilidad.

RequestOutsideTurn permite ventanas externas solo con autorización explícita de la carta. El motor comprueba dueño, prioridad, energía y objetivos. No habilita respuestas ficticias en los mazos importados.

### Efectos de cartas y estados

**Archivo:** `Assets/WarConquer/Runtime/EffectManager.cs`

ResolveSpell ejecuta terraformación, mejora, veneno, sueño, daño, curación, ralentización, destrucción, reactivación, terreno inestable y Marcha. Los objetivos ya han sido validados por GameManager. Para EnemyPiece elige la unidad enemiga o, si corresponde, la estructura enemiga; no daña a la unidad aliada que comparta una estructura transitable rival.

Poison comienza en un punto y conserva la progresión al reaplicarse; incrementa el contador de aplicaciones y mantiene las sinergias de Esporas. Sleep fija el próximo turno afectado y anula inmediatamente el movimiento. OnEnter, OnTileEnter y OnStart concentran disparos de despliegue, entrada y producción: redes, fichas, recursos ocultos, auras y Latentes. El Titán usa el mismo RollDie que alimenta el dado visual.

### Habilidades activadas

**Archivo:** `Assets/WarConquer/Runtime/AbilityManager.cs`

HasActive, TargetCount y Targets determinan disponibilidad y selección de habilidades de piezas. CanActivate verifica etapa, dueño, sueño, uso y requisitos. Activate valida los objetivos antes de tirar por terreno o gastar Esporas; aplica red, descuento, paso, destrucción o extensión de mejora y marca su uso.

LeaderTargets y Leader implementan las habilidades de Zukgrok y Sahria con coste y selección propios. La bonificación compartida de Sahria se agrupa para impedir que ambas casillas consuman el mismo daño adicional más de una vez.

### Rival automático

**Archivo:** `Assets/WarConquer/Runtime/AiPlayer.cs`

Step toma una decisión por llamada mediante las mismas acciones públicas que usa el jugador. Lee el tablero y su propia mano; no consulta la mano rival ni el orden de robo. Cada etapa intenta acciones adecuadas y después avanza. Un presupuesto finito de decisiones evita bloqueos.

Goal elige objetivos centrales aún no asegurados y luego bases enemigas. La evaluación de movimiento mantiene unidades guarneciendo objetivos. TryTerraform utiliza TerraformTarget y el motor común, de modo que no puede terraformar bajo enemigos. Tras cambiar el orden de etapas, ya no reserva energía en Terraformación para un Ataque que ocurrió antes. La IA conserva su configuración al guardar y cargar.

### Preparación de pruebas e integridad

**Archivo:** `Assets/WarConquer/Runtime/PrototypeScenario.cs`

Take y Spawn trasladan copias reales del mazo o mano al escenario, evitando inventar cartas adicionales. Load prepara explícitamente biomas, ceniza, una red y piezas de ambos bandos para probar sistemas. Solo se activa al elegir Escenario de pruebas.

StateValidator verifica versión, mapa esperado, participantes, etapas, casillas, ocupación, piezas, recursos y conservación exacta de las 50 cartas. También valida el historial de dados y las fechas de guarnición. Los guardados incompatibles se rechazan sin borrar el archivo del jugador.

### Guardado y carga

**Archivo:** `Assets/WarConquer/Runtime/GamePersistence.cs`

Serialize crea un GameSnapshot con el estado completo y listas de casillas que realmente contienen unidad, estructura y efecto de terreno. Estas máscaras evitan que Unity reconstruya objetos fantasma en campos que eran nulos.

Deserialize comprueba el formato, restaura las ausencias y recupera la batalla solo si existía. La validación semántica se realiza antes de aceptar la carga. Se conservan semilla, cartas, etapas, IA, estados, puntos, guarniciones y tiradas.

### Captura explícita de una compilación

**Archivo:** `Assets/WarConquer/Runtime/GrayboxCapture.cs`

Es un gancho de comprobación que solo se instala si se pasa --wc-capture por línea de comandos. Puede preparar el escenario con --wc-demo, guardar una captura y un estado JSON y cerrar con --wc-exit. No se ejecuta durante una partida normal.


## Interfaz y mundo tridimensional

### Estilo y controles comunes

**Archivo:** `Assets/WarConquer/Runtime/UI/GrayboxUI.cs`

Centraliza fondos oscuros, texto claro, acciones claras, estados deshabilitados y colores de facción: morado para Zukgrok y amarillo para Sahria. Rect, Box, Text y Button construyen controles uGUI coherentes. Clear oculta controles anteriores antes de destruirlos para evitar clics sobre elementos viejos.

HexGraphic y PieceGraphic proporcionan geometría plana para insignias y elementos del HUD. Su permanencia no significa que el tablero jugable siga siendo plano: ese tablero lo genera Board3DScene.

### Controlador principal de interfaz

**Archivo:** `Assets/WarConquer/Runtime/UI/WarConquerController.cs`

Awake carga el catálogo, crea Canvas y tablero y abre la preparación pausada. Render sincroniza etapa, Líder, puntuación, selección, mano, pilas y vista 3D. Cambiar de actor limpia selecciones y actualiza la mano consultada.

ValidTargets obtiene objetivos del motor. TileClick distingue inspección, movimiento, ataque, terraformación y selección de cartas; Confirm resuelve objetivos múltiples. El Canvas y los clics de acción se bloquean durante una tirada visual. Update ajusta el escalado y coordina el tablero y la IA.

### Mano consulta y selección de cartas

**Archivo:** `Assets/WarConquer/Runtime/UI/WarConquerController.Cards.cs`

DrawHand pagina seis cartas, filtra por tipo o disponibilidad y oscurece las no utilizables mediante alpha 0,40. SelectCard activa la selección y muestra el motivo real de bloqueo. El modo táctil permite abrir el detalle y elegir objetivos desde allí.

ShowTooltip y ShowCardModal amplían la carta sin jugarla. DrawPiles y ShowPile presentan mazo y descarte con contadores reales; la galería del mazo no revela el orden de robo. El pago mixto se elige desde la mano y vuelve a calcular costes y objetivos.

### Acciones de etapa y ficha del Líder

**Archivo:** `Assets/WarConquer/Runtime/UI/WarConquerController.Turn.cs`

DrawTurnActions usa TimingRules.Order y muestra únicamente acciones legales, intervención o estado de IA. El botón inferior avanza a Ataque, Terraformación o final de turno según corresponda. Los filtros de unidad, estructura y magia conservan todas las funciones con menos controles simultáneos.

DrawAbilityAction y SelectActionPiece ayudan a elegir piezas disponibles. DrawLeader separa la información del Líder de las cartas del mazo y muestra vida, coste, etapa, condición y botón de su habilidad.

### Puntuación e inspector

**Archivo:** `Assets/WarConquer/Runtime/UI/WarConquerController.Status.cs`

DrawScoreboard muestra PC, vida e ingresos potenciales por turno; Detalles despliega el control de zonas B1 a B4 y centro. Los asientos inactivos se señalan sin convertirlos en jugadores.

DrawSelection presenta datos de carta, pieza o casilla, acciones de movimiento y ataque, disponibilidad visible, bioma elegido y confirmación de objetivos. ShowVictory bloquea la partida terminada y permite preparar otra.

### Preparación menús y persistencia

**Archivo:** `Assets/WarConquer/Runtime/UI/WarConquerController.Dialogs.cs`

ShowSetup permite elegir una a cuatro personas y ambos mazos para cada participante, incluida la IA. SetMatchVisible oculta tanto el HUD como el mundo 3D durante la preparación. StartMatch aplica las elecciones y reinicia cámara, selección e IA; Cerrar permite regresar a la partida anterior cuando existe.

ShowMatchMenu, ShowHelp, ShowLeaderDetails y ShowLog agrupan consultas para reducir la carga del HUD. Save y Load gestionan el archivo local en persistentDataPath, con validación antes de restaurar. La ayuda explica las tres posiciones de conquista y el nuevo orden de etapas.

### Ritmo de la IA en pantalla

**Archivo:** `Assets/WarConquer/Runtime/UI/WarConquerController.AI.cs`

Mantiene una instancia de AiPlayer y separa sus decisiones por un intervalo visible. UpdateAI se detiene en preparación, diálogos, partidas terminadas y animaciones de dados. Al devolver el turno, los controles y la mano del humano vuelven a activarse.

### Presentación reutilizable de las cartas

**Archivo:** `Assets/WarConquer/Runtime/UI/CardPresentation.cs`

Draw construye versiones compactas y ampliadas con coste, vida, fuerza, movimiento, tipo, descripción y ventanas. La banda de etapa conserva contraste aunque el resto de la carta esté atenuado; no cambia las reglas de disponibilidad.

El detalle usa EnergyManager.Quote para mostrar el coste real. Las magias sin alcance explícito indican que sus objetivos dependen del texto, evitando mostrar un alcance cero confuso. CardHover coordina la ampliación al entrar y salir el puntero.

### Cámara 3D integrada en el HUD

**Archivo:** `Assets/WarConquer/Runtime/UI/BoardView.cs`

BoardView coloca la RenderTexture del tablero dentro de un RawImage y conserva un plano superpuesto para controles. Tick ajusta la resolución a la escala del Canvas. Reset, Rotate, Zoom y Pan controlan la cámara sin modificar la partida.

BoardViewportInput convierte las coordenadas de pantalla al espacio normalizado de la cámara. Distingue clic, arrastre, giro con botón derecho y rueda. Un arrastre no termina provocando un clic de juego accidental.

### Sincronización del mundo tridimensional

**Archivo:** `Assets/WarConquer/Runtime/UI/Board3DScene.cs`

Initialize crea cámara, luces, suelo, fábrica de mallas y controlador de dados. Sync genera prismas, biomas, bases, piezas, rótulos y vías rápidas a partir del estado. Mantiene diccionarios para actualizar objetos existentes y retira los destruidos. Cambiar de partida reconstruye la geometría, permitiendo alternar mapas de tamaños distintos.

Los objetivos tienen borde dorado y +1 PC. Cada miniatura recibe vida y estados; los colliders conservan el identificador de su casilla. Pick hace raycast para seleccionar y UpdateCamera encuadra el mapa, permite giro y zoom y orienta etiquetas. La vista no decide daño, costes ni puntos.

### Modelos procedurales del graybox

**Archivo:** `Assets/WarConquer/Runtime/UI/BoardMeshFactory.cs`

Genera y comparte prismas hexagonales, anillos y formas sencillas usadas en biomas, castillos, unidades y estructuras. PieceModel y BiomeModel distinguen piezas y terrenos mediante siluetas y colores. No exige modelos artísticos externos.

Part crea renderizadores y, cuando se requiere, colliders. Paint usa propiedades de material para colorear. Label crea texto en el mundo. Release desactiva objetos antes de retirarlos y Dispose libera las mallas y materiales que posee.

### Veneno sueño y ataque visibles

**Archivo:** `Assets/WarConquer/Runtime/UI/BoardStatusVisual.cs`

Construye una calavera con huesos cruzados de morado oscuro, burbujas azules y el texto Zzz. Sync activa los indicadores según la pieza y detecta cada nueva aplicación de veneno para repetir el pulso.

La etiqueta de ataque distingue disponibilidad, uso, falta de objetivo, espera y sueño. Update anima burbujas y pulso y orienta los indicadores hacia la cámara. Los efectos visuales no alteran la duración ni el daño de los estados.

### Dado tridimensional y resultado

**Archivo:** `Assets/WarConquer/Runtime/UI/BoardDiceVisual.cs`

Sync recibe el historial autoritativo de RollDie y encola únicamente las tiradas nuevas. Build crea un cubo con los puntos de sus seis caras. Update anima lanzamiento, giro y rebotes y orienta hacia arriba la cara del resultado que ya resolvió el motor.

El texto muestra tirada, umbral y SUPERA o FALLA. IsAnimating mantiene en pausa nuevas acciones e IA durante la presentación. Al cambiar o cargar partida, limpia la cola y los modelos sin repetir tiradas antiguas. Es una animación determinista de rebote, no un segundo cálculo aleatorio mediante física.


## Herramientas y pruebas de Unity

### Acceso a la escena y compilación

**Archivo:** `Assets/WarConquer/Editor/GrayboxProject.cs`

Añade menús para abrir o generar la escena Graybox. Open permite guardar cambios pendientes antes de abrirla. CreateScene construye cámara y controlador en una escena independiente. ValidateAndBuild ejecuta las pruebas y, si se indica una ruta de salida, genera una compilación Windows de desarrollo. Estas herramientas no forman parte del ejecutable del jugador.

### Vista del mapa sin Play

**Archivo:** `Assets/WarConquer/Editor/GrayboxScenePreview.cs`

Dibuja en Scene la misma retícula obtenida de BoardManager, con colores de base y tres objetivos. FrameMap selecciona el controlador y encuadra la vista tridimensional. La previsualización usa el mapa de cuatro participantes y no añade objetos de juego a la escena.

### Materiales del tablero

**Archivo:** `Assets/WarConquer/Editor/Board3DAssets.cs`

Ensure crea, cuando faltan, los materiales URP y Standard dentro de Resources. Comprueba que los shaders existan y habilita instancing. Mantenerlos como assets evita que el material necesario desaparezca en una compilación por depender únicamente de una búsqueda de shader en ejecución.

### Pruebas generales de reglas

**Archivo:** `Assets/WarConquer/Editor/GrayboxTests.cs`

RunAll carga el catálogo y ejecuta casos sobre mazos, tablero, energía, colocación, magia, biomas, estados, movimiento, combate, rutas, producción y guardado. Incluye una partida larga con validación de integridad e invoca las suites especializadas. El marcador WAR_CONQUER_TESTS_PASSED solo aparece cuando todos los casos terminan correctamente.

### Reglas que afectan a la interfaz

**Archivo:** `Assets/WarConquer/Editor/InterfaceRulesTests.cs`

Prueba la secuencia de etapas, disponibilidad y pago, permisos de intervención, prioridad, respuestas, conquista y victoria. Usa catálogos sintéticos para comprobar permisos que no figuran en los mazos reales; esas modificaciones no se escriben en cards.json.

### Participantes e inteligencia artificial

**Archivo:** `Assets/WarConquer/Editor/MatchSetupTests.cs`

Comprueba pausa inicial, uno a cuatro humanos, selección de mazos, puestos vacíos, rotación y guardado. Ejercita acciones de IA con ambos Líderes y partidas completas contra un jugador pasivo, además de respuestas autorizadas y reanudación de una partida cargada.

### Integridad de la representación tridimensional

**Archivo:** `Assets/WarConquer/Editor/Board3DTests.cs`

Crea fixtures aislados que comparan prismas, coordenadas, conexiones, selección por collider, cámara, vida, movimiento y retirada de modelos. Comprueba que renderizar no modifique el estado ni los mazos y que el modo solitario solo muestre dos bases.

### Comprobación del Canvas en Play

**Archivo:** `Assets/WarConquer/Editor/InterfacePlayModeTests.cs`

Ejercita el controlador real, los textos de etapas y Líderes, las dos condiciones de victoria y la consulta de cartas. Invoca las pruebas de preparación y refinamiento. Restaura el estado anterior al terminar y no escribe el guardado del jugador.

### Botones de preparación reales

**Archivo:** `Assets/WarConquer/Editor/SetupPlayModeTests.cs`

Pulsa los selectores de participantes y mazos mediante sus callbacks. Verifica que no avancen turnos durante la pausa, que el HUD y el mundo estén ocultos, que se revelen al comenzar y que la IA pause y reanude al abrir y cerrar la preparación.

### Legibilidad y conservación de controles

**Archivo:** `Assets/WarConquer/Editor/InterfaceRefinementTests.cs`

Comprueba atenuación de cartas, banda de etapa, consulta disponible, controles de detalle y separación entre preparación y partida. Protege las funciones existentes mientras se reduce la carga visual de la interfaz.

### Clics y captura del juego real

**Archivo:** `Assets/WarConquer/Editor/Board3DPlayTests.cs`

Abre Play, ejecuta las suites del Canvas y comprueba despliegue y movimiento mediante eventos de puntero normalizados sobre el tablero 3D. Verifica correspondencia entre piezas lógicas y modelos, captura el tablero y deriva a RevisionVisualPreview. Su marcador local de solicitud se consume una sola vez.

### Pruebas de la revisión de mapas y estados

**Archivo:** `Assets/WarConquer/Editor/RevisionRulesTests.cs`

Compara distancias a objetivos y distribución de casillas alcanzables desde cada base; verifica simetría geométrica, recursos, conectividad, siete casillas iniciales y equivalencia de solo y duelo. Cubre guarnición sostenida, recaptura, pérdida de bioma, bases derrotadas y ausencia de doble puntuación al cargar.

También comprueba protección frente a terraformación bajo enemigos, veneno 1 2 4, magias lejanas según texto, mejora, sueño, ralentización, Erosión en casilla compartida, Mar de Arena y dados de ataques y habilidades. Las pruebas se ejecutan contra el motor real.

### Selección de magias y estados en el controlador

**Archivo:** `Assets/WarConquer/Editor/RevisionPlayTests.cs`

Selecciona Espora Somnífera, Nube de Esporas y Crecimiento Descontrolado en el controlador real. Comprueba clic de objetivo, confirmación parcial, selección múltiple, elección de Pantano y descarte correcto. Verifica que calavera, burbujas y Zzz estén activos y que el estado conserve sus cartas.

### Capturas de mapas y dado

**Archivo:** `Assets/WarConquer/Editor/RevisionVisualPreview.cs`

Alterna partidas de dos, tres y cuatro participantes, espera a que se rendericen y guarda capturas en Library/WarConquer3D. Después prepara una escena explícita con veneno, sueño y una tirada y comprueba que el texto del dado coincida con el número registrado por el motor. Solo se ejecuta desde la comprobación de Editor.

## Datos y escena

`Resources/WarConquer/cards.json` conserva las cantidades de las tablas detalladas de los dos mazos finales: 30 diseños de Zukgrok y 28 de Sahria, más Espora y Obelisco de Arena como fichas de cantidad cero. Sus campos enlazan estadísticas, descripciones, rasgos, efectos y ventanas. Para Mar de Arena se corrigió el selector a Desierto libre, sin imponer propiedad no escrita en la carta; se mantiene la prohibición de modificar el terreno bajo enemigos.

`Resources/WarConquer/rules.json` configura mano, energía, vida de Líder, costes, producción, fichas y reglas de prototipo. Algunos campos históricos permanecen serializados por compatibilidad de estructura pero ya no controlan veneno ni alcance general. Cambiar `poisonDuration` o `spellRange` no cambia las reglas nuevas.

`Scenes/WarConquer_Graybox.unity` es la escena ejecutable. Contiene el controlador que genera la interfaz y el mundo al entrar en Play. `Materials/BoardURP.mat` y `BoardStandard.mat` soportan los modelos procedurales. Los archivos `.meta` conservan los identificadores que Unity usa para enlazar los assets.

## Ejemplos de uso y mantenimiento

**Añadir un efecto de carta.** Definir sus datos en el catálogo, incorporar su objetivo a `GameManager.ValidSpellTarget` si es nuevo y resolver su operación en `EffectManager`. Ajustar la evaluación de IA si necesita otra estrategia y añadir una prueba que compruebe resultado, coste y descarte. Mantener exactamente 50 copias por mazo.

**Cambiar una geometría.** Modificar `BoardManager.Create`; conservar coordenadas únicas, adyacencia por borde y siete casillas de cada base. Volver a comprobar equivalencia de distancias, recursos y expansión. Una geometría incompatible exige revisar la versión de guardado y la validación.

**Cambiar el estilo.** La paleta está en `GrayboxUI`; la carta reutilizable en `CardPresentation`; los paneles en las partes de `WarConquerController`. Los modelos y su iluminación están separados en `BoardMeshFactory` y `Board3DScene`. El estilo no debe cambiar costes ni objetivos.

**Revisar un resultado del dado.** `GameState.diceRolls` contiene valor, umbral, razón, casilla y pieza. El registro muestra el mismo número que la animación. No se debe volver a tirar desde el componente visual, porque produciría discrepancias con el daño ya resuelto.

**Revisar la conquista.** Inspeccionar dueño, bioma, unidad, garrisonOwner, garrisonSinceTurn y lastConquestTurn de la casilla. Controlar terreno sin unidad no genera PC. Una estructura tampoco reemplaza a la unidad exigida.

## Validación y límites del prototipo

La suite final de reglas supera 93 casos, incluidos los de ambos mazos, simulación de partidas de IA y los nuevos mapas, estados y objetivos. Las comprobaciones de Play ejercitan Canvas, configuración, clics 3D, magias, estados y presentación del dado. Las imágenes de QA se guardan en `Library/WarConquer3D`; no son assets necesarios para ejecutar el juego.

El guardado actual requiere `GameState.version = 5`. Una partida antigua no se convierte automáticamente al nuevo mapa; su archivo se conserva y la interfaz solicita empezar una nueva. El formato externo del contenedor `GameSnapshot` sigue siendo 2, porque sus máscaras de ocupación no cambiaron.

La partida es local y comparte pantalla. Consultar manos de otros participantes sigue disponible; no hay multijugador por red ni información privada. La IA usa heurísticas y un único nivel. Los disparos opcionales de entrada mantienen selección automática determinista; las magias jugadas y las habilidades activadas sí permiten elegir objetivos. Arte, animaciones finales, audio y balance competitivo siguen siendo trabajo posterior.


Los mapas duplican sus dimensiones sobre la retícula anterior y superan el triple de casillas. En tres jugadores, todos los objetivos quedan al menos a siete pasos de cualquier base principal. La cámara permite zoom hasta cinco aumentos y desplazamiento ampliado.

# War & Conquer — Graybox funcional de Unity

## Ejecutar

1. Abre este proyecto en **Unity 6000.6.2f1**.
2. En el menú superior elige **War & Conquer → Abrir Graybox**. También puedes abrir `Assets/WarConquer/Scenes/WarConquer_Graybox.unity`.
3. Pulsa **Play**. La pantalla de preparación mantiene la partida en pausa y oculta completamente la interfaz y el tablero hasta pulsar **COMENZAR PARTIDA**. Al abrirla durante una partida, Cerrar permite volver a la partida anterior.
4. Elige **1, 2, 3 o 4 personas**. Una persona juega contra una IA; las otras opciones son partidas locales con ese número de personas, sin rellenar puestos con IA. Elige Zukgrok o Sahria para cada participante, incluido el rival automático. Cada mazo conserva sus 50 cartas.
5. El mapa cambia con los participantes: duelo ovalado de 223 hexágonos para 1 persona contra IA o 2 personas; hexágono regular de 331 casillas para 3; hueso simétrico de 459 para 4. Cada base tiene su casilla principal y seis vecinas. Los puestos vacíos no reciben turnos ni cuentan para ganar.
6. Para comprobar combate y efectos sin esperar varios turnos: **Menú → Nueva partida → Escenario de pruebas**. Este escenario prepara explícitamente unidades, biomas, una red y ceniza; el inicio normal sigue siendo completamente neutro.

El juego es local por turnos compartiendo pantalla. Los controles de jugador permiten consultar las manos de los participantes; solo el jugador activo puede ejecutar acciones. El morado identifica a Zukgrok y el amarillo a Sahria, incluso si se cambia la selección de Líder. Los números J1–J4 distinguen a jugadores con el mismo mazo. La IA decide despliegues, terraformación, habilidades, movimiento y ataques con las mismas reglas y costes, sin consultar manos rivales ni el orden de robo. Se detiene al abrir un diálogo y reanuda al cerrarlo. El guardado conserva los participantes, sus mazos y quién controla cada puesto.

## Controles

- **Carta → hexágono verde:** paga el coste, retira la carta de la mano y resuelve su efecto. Las cartas se amplían en el inspector.
- **Selección múltiple:** selecciona los objetivos y usa **Resolver selección** si quieres menos del máximo. Al completar el máximo se resuelve automáticamente. **Cancelar** no consume recursos.
- **Unidad propia:** muestra movimientos alcanzables. Pulsa un destino verde para mover. Usa **Atacar** para cambiar a objetivos de combate.
- **Casilla / Cancelar → bioma:** inicia una terraformación. El coste base se muestra en el inspector.
- **Revelar Tierra Ceniza:** selecciona un bioma normal propio y paga el coste de la regla de prototipo. Las Latentes solo se despliegan sobre ceniza propia y libre.
- **Estructura o unidad con habilidad:** usa el botón de habilidad del inspector. La Red Micelial permite seleccionar sus dos extremos.
- **Habilidad del Líder:** Zukgrok necesita una red micelial y un enemigo en Bosque conectado o adyacente a ella; Sahria elige hasta dos Desiertos propios.
- **Marcha Micelial:** elige una unidad situada en un extremo de una vía rápida y después el destino. Puedes resolver un par o añadir otra unidad y su destino.
- **Solo energía / Pago mixto:** por defecto se paga exactamente la energía indicada. El pago mixto es una elección explícita; la carta muestra el coste original → energía final y su vista ampliada desglosa recursos y descuentos. Solo se admiten recursos compatibles con las etiquetas.
- **Despliegue → Ataque → Terraformación → Finalizar turno:** avanza mediante el botón izquierdo; al finalizar se resuelve el fin de turno, rota de jugador, genera energía, resuelve veneno/producción y roba automáticamente.
- **Menú:** reúne Nueva partida, Guardar, Cargar, Registro y Ayuda. Pausa las decisiones de la IA mientras está abierto. Guardar/Cargar conserva la partida completa en `Application.persistentDataPath/war-conquer-save.json`, incluidos mazos, instancias, rutas, turnos y estado aleatorio.
- **Registro:** muestra cartas resueltas, daño, tiradas, movimiento, producción y cambios de turno.
- **Rueda / + / −:** acercan o alejan la cámara 3D. Arrastra con el botón izquierdo para desplazarla y con el derecho para girarla; también puedes usar las flechas y **Girar − / +**. **1:1** restablece la vista completa.

## Tablero

Los tres mapas usan una única retícula de hexágonos con bordes compartidos, sin puentes. El duelo forma un óvalo con bases opuestas. El mapa de tres es un hexágono regular de radio 10 con bases distribuidas por rotaciones de 120 grados. El de cuatro forma un hueso con cuatro extremos equivalentes por reflexión horizontal y vertical. Las pruebas comparan las distancias a los objetivos y la cantidad de casillas a cada distancia de todas las bases.

Cada jugador controla siete casillas iniciales neutras: su base y sus seis vecinas. Fuera de ellas el terreno comienza sin dueño ni bioma. Los colores de las bases identifican J1–J4, y las miniaturas y los mazos usan morado para Zukgrok y amarillo para Sahria. Hay tres objetivos centrales separados, marcados con un borde dorado y `+1 PC`. Los recursos ocultos también se distribuyen simétricamente.

El tablero tiene casillas hexagonales con volumen, iluminación y sombras. Las unidades son miniaturas 3D y las estructuras y bases son edificios 3D. Los colores iniciales distinguen territorios: todas las casillas siguen **sin bioma** hasta terraformarse. Al terraformar, el terreno y su decoración muestran el bioma; la marca J1–J4 indica el control actual. Las unidades y los mazos conservan morado para Zukgrok y amarillo para Sahria. Los rótulos muestran vida y próximo daño de veneno. Una calavera con huesos cruzados de color morado oscuro pulsa al envenenar; burbujas y Zzz indican sueño. Cada unidad distingue ataque disponible, ataque usado, sin objetivo o espera. Las vías rápidas aparecen como líneas sobre el suelo, sin puentes.

La interfaz de cartas e información se conserva sobre una vista de cámara 3D. Tanto los hexágonos como las miniaturas responden a clics para seleccionar, desplegar, mover y atacar. `Board3DScene` sincroniza los modelos con la partida y `BoardMeshFactory` genera las mallas compartidas del graybox. El escenario de pruebas permite ver unidades y estructuras de las dos facciones desde el inicio.

**War & Conquer → Ver mapa en escena** permite inspeccionar el mapa antes de pulsar Play. Los guardados antiguos del mapa de islas no se cargan en esta versión; se conservan en disco y se puede iniciar una partida nueva.

## Interfaz y reglas de etapas

- Izquierda: etapa activa y acciones legales; debajo, ficha independiente del Líder con vida, facción, retrato graybox, habilidad, coste, condición y disponibilidad.
- Centro: tablero continuo con los datos esenciales. **Ver datos** recupera números y control de casillas, y puntos sobre las bases; los objetivos seleccionados siempre conservan su indicación.
- Derecha: cuatro marcadores compactos de Conquista y vida; **Detalles** despliega el control por zona y los puntos restantes. El detalle de selección aparece debajo. **Ver habilidad** abre el texto completo del Líder sin cambiar sus reglas ni su botón de activación.
- Abajo: mano de seis cartas por página, coste/vida arriba, fuerza/movimiento debajo del área central y una banda destacada con la etapa de uso. Las cartas no disponibles se oscurecen más, manteniendo legible la etapa y disponible su consulta. Hover abre una carta ampliada con todos los biomas y etiquetas sin modificar el estado. En pantalla táctil, tocar abre el detalle y permite seleccionar objetivos desde allí.
- Esquina inferior derecha: mazo y descarte como pilas, contador real y carta superior del descarte. Al pulsar una pila se abre una galería paginada. El mazo no revela su orden de robo. Robar fuera de la ventana correspondiente se rechaza.
- La pantalla de victoria indica ganador y motivo; no se pueden ejecutar más acciones.

`EnergyManager.Quote` es la fuente compartida de coste para interfaz y pago. Los descuentos proceden de efectos existentes (por ejemplo, Sacerdote Solar), se muestran y se consumen. Los recursos no reducen energía automáticamente: se requiere seleccionar Pago mixto, cuyo desglose también aparece en la carta.

Cada carta define `allowedPhases`, `allowedTiming`, `canRespond`, `canInterrupt`, `canReact` y `timingPermissionText`. Los textos finales suministrados no contienen permisos para jugar cartas en una intervención: se conservan y **no se les inventan permisos**. Las magias territoriales se asignan a Terraformación y las de combate a Asalto; Despertar de las Ruinas admite ambas etapas para reactivar habilidades de estructura compatibles. Las habilidades activas siguen su función (descuento: Despliegue, red/destrucción: Terraformación, movimiento/mejoras: Asalto).

El motor de batalla admite respuestas explícitas del atacante, defensor y los dos terceros, en ese orden. Un permiso externo requiere texto presente en la propia descripción y el indicador correspondiente. Se comprueban objetivos, energía, dueño y prioridad; se puede responder o pasar. Después se revalida y resuelve el ataque. Si no hay respuestas legales, se resuelve directamente. Una carta que duerme o elimina al atacante puede cancelar el golpe. También existe una ventana solicitada para cartas con permiso explícito de turno ajeno. Estas rutas se prueban con catálogos sintéticos exclusivamente en Editor; las listas reales siguen siendo las dos de 50 cartas.

La nueva partida guarda etapa, puntos, ronda puntuada, prioridad de intervención, batalla pendiente y motivo de victoria. Los guardados de versiones anteriores se conservan en disco pero no se cargan con las reglas nuevas.

Módulos añadidos: `EnergyManager`, `TimingRules`, `BattleManager`, `ConquestManager`, `CardPresentation`, vistas parciales `WarConquerController.Cards`, `.Turn`, `.Status`, `.Dialogs` y pruebas `InterfaceRulesTests`. La lógica de biomas, ceniza, veneno, sueño, vuelo, terrenos inestables, rutas, cartas y cuatro jugadores permanece integrada.

## Fuente de los mazos

Se han importado las **filas de las dos tablas detalladas** de `War_and_Conquer_Mazos_50_Cartas_FINAL.docx`: nombres, cantidades, categorías, subtipos, costes, estadísticas, biomas, etiquetas y descripciones. No se han inventado cartas para completar los mazos.

| Mazo | Unidades | Estructuras | Magias | Latentes | Total |
|---|---:|---:|---:|---:|---:|
| Zukgrok | 22 | 15 | 11 | 2 | 50 |
| Sahria | 20 | 17 | 11 | 2 | 50 |

Hay 30 diseños de Zukgrok y 28 de Sahria. Ninguno supera tres copias. El documento conserva un encabezado de Sahria que dice 72, una lista rápida con cartas adicionales sin estadísticas y un resumen final con 18/15/15/2. Estos resúmenes contradicen la tabla detallada, que sí suma 50. La implementación usa esa tabla y conserva sus cantidades exactas.

El Líder, la energía, los recursos y las fichas generadas no se incluyen en las 50 cartas. Espora y Obelisco de Arena son definiciones de ficha con cantidad cero en el catálogo.

## Sistemas implementados

- Estado real e independiente de mazo, mano, descarte, unidades, estructuras, control, recursos, biomas y estados temporales.
- Barajado reproducible por semilla, instancias únicas y conservación de cartas; sin reutilizar cartas ya jugadas.
- Costes de energía, pago opcional mediante recursos compatibles, ingresos y descuentos de estructuras.
- Colocación de unidades, estructuras y Latentes; validación previa de energía, propiedad, ocupación y compatibilidad.
- Magias de objetivo único y múltiple; daño, curación, mejoras, veneno, sueño, ralentización, terraformación, destrucción y reactivación.
- Movimiento entre hexágonos adyacentes, ocupación, obstáculos, vuelo, rutas rápidas y tiradas de entrada/salida del terreno inestable.
- Ataque por alcance, auras, defensa, salud, muerte y descarte. Las bases reciben ataques de unidades; al derrotar al Líder la base pasa al atacante. Gana el último Líder vivo o quien alcanza 10 Puntos de Conquista.
- Producción de Esporas y fichas, evolución de Larva, Red Micelial, recursos de bioma y centro, habilidades de Líder y de piezas.
- Guardado/carga local con validación, HUD, consultas de mazo/descarte, colores por facción y escenario explícito de pruebas.

## Reglas de prototipo y decisiones por confirmar en balance

Los documentos no cierran todos los valores ni todas las ambigüedades. `Assets/WarConquer/Resources/WarConquer/rules.json` contiene la configuración general; `cards.json` contiene los datos de cartas y los rasgos de efectos.

- Mano inicial: 5. Robo: 1 por turno después del primer turno propio. Un mazo vacío deja de robar; no se ha añadido daño de fatiga.
- Energía: 3 en el primer turno propio, +1 máximo por turno propio hasta 10; se repone al comenzar el turno. La energía adicional de Trono Solar puede superar el máximo base.
- Vida del Líder: 25. Una unidad puede mover y atacar el turno en que se coloca; `summoningSickness` permite cambiarlo.
- Cada turno recorre Despliegue, Ataque y Terraformación, en ese orden. No se puede finalizar antes de Terraformación. Los valores numéricos de las etapas del catálogo se mantienen; el orden se controla explícitamente. Inicio, energía, producción y robo ocurren una sola vez por turno. Unidades y estructuras se juegan en Despliegue; terraformación manual y magia territorial en Terraformación; movimiento, ataque y magias de combate en Asalto.
- Terraformar: 1 Energía base, sobre terreno propio o adyacente al propio, sin transformar ceniza permanente ni una casilla ocupada por un enemigo. Los santuarios descuentan la primera terraformación de su bioma del turno.
- Se permite desplegar sobre casillas neutras propias al inicio (`allowNeutralDeployment`), para que una partida sin biomas pueda arrancar. Sobre un bioma ya definido se exige afinidad; las Latentes siempre exigen ceniza.
- Recursos: cada bioma normal propio genera al menos 1 por turno propio, con 2 en un objetivo cuyo recurso oculto haya sido revelado. Tope 20 por bioma; solo sirven para cartas con su etiqueta. Los colores de territorio no generan recursos en casillas neutras.
- Las magias seleccionan los tipos de objetivo y biomas escritos en la carta. Cuando su dato `range` es 0 no se impone una distancia artificial de tres casillas. Un alcance explícito positivo sí se comprueba; Torre de Esporas amplía ese alcance para veneno. Las condiciones de adyacencia y red siguen vigentes. Mar de Arena admite desiertos libres de cualquier dueño, sin modificar casillas con ocupantes enemigos.
- Veneno: pierde 1, 2, 4, 8 y sucesivos puntos de vida al inicio de sus turnos propios, sin reducción por defensa y hasta morir. Reaplicar muestra otra vez el indicador y conserva la progresión. Sueño bloquea movimiento, ataque y habilidades durante el próximo turno propio y se retira al terminarlo.
- Tormenta de Arena reduce MOV en el próximo turno afectado de la unidad, para que tenga efecto en un juego sin interrupciones durante el turno rival.
- Revelar ceniza: acción de 3 Energías sobre bioma normal propio, rebajada por Templo de Ceniza. Además, las casillas principales de las bases están marcadas simétricamente cuya destrucción revela ceniza.
- Cada paso cruza un borde compartido entre dos hexágonos. Los disparos usan distancia de grafo; no se ha definido línea de visión.
- Las estructuras normales bloquean el paso. Arena Profunda, Dunas Movedizas y Oasis de Cristal permiten una unidad encima de su casilla de terreno. Guardián del Oasis obtiene su defensa al estar sobre el Oasis.
- Fallar una tirada de entrada deja a la unidad en la casilla anterior, le causa el daño y termina su movimiento. Fallar al salir, cuando el efecto exige tirada de salida, la mantiene en su sitio. Atacar o activar una habilidad desde terreno afectado también exige tirada; fallar consume el intento y aplica el daño. Vuelo y compatibilidad evitan la tirada. Cada resultado real se representa mediante un dado 3D que rebota, se detiene mostrando su cara y anuncia el resultado. Durante la animación se bloquean nuevas acciones y decisiones de IA.
- Red Micelial elige inicialmente los dos terrenos compatibles más cercanos y permite reconfigurarlos con su habilidad. Hongo Explorador recibe su bonificación una vez por turno; Ciervo cruza sin coste adicional.
- Los efectos opcionales de entrada usan selección automática determinista de los primeros objetivos válidos. La interfaz permite seleccionar los objetivos de cartas jugadas, magias, líderes y habilidades activadas; decidir manualmente cada disparo de entrada queda como mejora posterior.
- Chamán extiende una mejora temporal de ATQ desde otra unidad adyacente a la unidad aliada elegida y consume 1 Espora. El documento no enumera qué otras clases de efecto pueden extenderse; ampliar esa selección requiere cerrar la regla.
- Las estadísticas de Espora, Espora Evolucionada y Obelisco de Arena no aparecen en la fuente: sus valores están identificados como configurables de prototipo.
- Mar de Arena dura dos cambios de ronda y requiere 5+ al entrar o salir. El daño de entrada no se especifica: se usa 1 como valor de prototipo.
- Cada uno de los tres objetivos centrales y cada base enemiga derrotada puede producir +1 PC. Hace falta una unidad del controlador sobre esa misma casilla y un bioma distinto de Neutro. La guarnición debe mantenerse hasta el comienzo del siguiente turno de su dueño. La puntuación ocurre antes del daño de veneno de ese inicio. Salir, perder el bioma o perder la unidad reinicia la espera; recapturar nunca concede puntos inmediatos repetibles. El guardado conserva la guarnición y la última puntuación de cada casilla para impedir duplicados. Gana quien alcanza 10 PC o queda como último Líder vivo.

Quedan pendientes el balance competitivo, arte final, audio, animaciones finales, niveles de dificultad de la IA, multijugador por red, información privada de manos y un editor visual de mazos. La partida local usa las 50 cartas de cada listado fijo.

## Arquitectura y archivos

Todo el sistema se añade bajo `Assets/WarConquer/` y usa el namespace `WarConquer`. La escena inicial `Assets/Scenes/SampleScene.unity`, la configuración de URP y las dependencias originales se conservan.

| Archivo | Responsabilidad |
|---|---|
| `Runtime/Model.cs` | Tipos de cartas, piezas, jugadores, turnos, recursos, reglas y estado serializable |
| `Runtime/CardCatalog.cs` | Carga y validación del catálogo, mazos, robo y pagos compatibles |
| `Runtime/BoardManager.cs` | Tres mapas simétricos, bases de siete, objetivos, adyacencia y distancias |
| `Runtime/GameManager.cs` | Partida, validación y resolución de acciones de usuario |
| `Runtime/TurnManager.cs` | Inicio/fin de turno, estados, energía, robo y producción |
| `Runtime/TerrainManager.cs` | Biomas, ceniza, rutas y tiradas de terreno |
| `Runtime/MovementManager.cs` | Búsqueda de rutas, movimiento normal y adicional |
| `Runtime/CombatManager.cs` | Ataque, defensa, daño, destrucción y eliminación |
| `Runtime/EffectManager.cs` | Operaciones de efectos y disparos de cartas |
| `Runtime/AbilityManager.cs` | Activación de habilidades de líderes y piezas |
| `Runtime/PrototypeScenario.cs` | Escenario de prueba y validador de integridad de guardados |
| `Runtime/GamePersistence.cs` | Guardado completo conservando los espacios vacíos de Unity |
| `Runtime/UI/GrayboxUI.cs` | Controles uGUI y geometría de hexágonos y fichas |
| `Runtime/UI/BoardView.cs` | Presentación del tablero continuo, biomas y selección |
| `Runtime/UI/WarConquerController.cs` | HUD, mano, inspector, selección, controles y guardado |
| `Runtime/GrayboxCapture.cs` | Captura de verificación activada solo por argumentos explícitos |
| `Resources/WarConquer/cards.json` | 58 diseños importados y 2 fichas, editables |
| `Resources/WarConquer/rules.json` | Valores y decisiones configurables de prototipo |
| `Editor/GrayboxProject.cs` | Menú para abrir/generar escena y compilación de comprobación |
| `Editor/GrayboxScenePreview.cs` | Vista previa del mapa en Scene sin iniciar la partida |
| `Editor/GrayboxTests.cs` | Pruebas de reglas, invariantes y partida automatizada |
| `Scenes/WarConquer_Graybox.unity` | Escena ejecutable del prototipo |

## Verificar

En Unity: **War & Conquer → Ejecutar pruebas de reglas**. La consola imprime `PASS` para cada prueba y `WAR_CONQUER_TESTS_PASSED` al finalizar. Una prueba fallida lanza una excepción con el caso afectado.

Con Play activo: **War & Conquer → Comprobar interfaz en Play** verifica los botones de participantes y mazos, la pausa, la reanudación de IA y las pantallas de victoria; restaura la partida anterior sin escribir el guardado del jugador. `MatchSetupTests` comprueba los cuatro modos, duelos con ambos mazos, respuestas autorizadas de IA, partidas completas y carga a mitad de turno.

Las pruebas cubren mazos, neutralidad, distribución territorial, conectividad resistente a un bloqueo, pago y colocación atómicos, descarte, biomas, Latentes, energía/robo, veneno/sueño, movimiento, combate, terreno inestable, vuelo, rutas, Marcha, producción, habilidades, guardado y una partida automatizada de 120 turnos con comprobación de integridad después de cada acción.

Para exportar una compilación, añade la escena Graybox a los Build Profiles de Unity. El proyecto mantiene la escena original en sus ajustes para no alterar el trabajo previo del equipo.

## Revisión de mapas y estados de septiembre de 2026

`GameState.version` es 5. Los guardados anteriores conservan su archivo, pero deben empezar una partida nueva para usar estas geometrías y reglas de conquista. `mapPlayers`, los campos de guarnición y `diceRolls` guardan las nuevas condiciones. `poisonDuration`, `spellRange` y `lastScoredRound` son campos antiguos que ya no gobiernan estas reglas.

`RevisionRulesTests` cubre simetría, rutas equivalentes de expansión, puntuación sostenida, recapturas, ocupantes enemigos, veneno creciente, magias y dados. `RevisionPlayTests` prueba la selección real del controlador y los indicadores 3D. `RevisionVisualPreview` captura los tres mapas y verifica el resultado del dado en Play; se ejecuta únicamente desde la comprobación explícita del editor.

Los mapas duplican sus dimensiones sobre la retícula anterior y superan el triple de casillas. En tres jugadores, todos los objetivos quedan al menos a siete pasos de cualquier base principal. La cámara permite zoom hasta cinco aumentos y desplazamiento ampliado.

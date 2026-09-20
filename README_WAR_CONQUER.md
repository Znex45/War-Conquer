# War & Conquer — Graybox funcional de Unity

## Ejecutar

1. Abre este proyecto en **Unity 6000.6.2f1**.
2. En el menú superior elige **War & Conquer → Abrir Graybox**. También puedes abrir `Assets/WarConquer/Scenes/WarConquer_Graybox.unity`.
3. Pulsa **Play**. La partida empieza con cuatro jugadores, cinco territorios, 87 casillas neutras y cinco cartas en cada mano.
4. **Nueva partida** permite elegir Zukgrok o Sahria para cada jugador, cambiar la semilla y comenzar de nuevo.
5. Para comprobar combate y efectos sin esperar varios turnos: **Nueva partida → Cargar escenario de pruebas**. Este escenario prepara explícitamente unidades, biomas, una red y ceniza; el inicio de una partida normal sigue siendo completamente neutro.

El juego es local por turnos compartiendo pantalla. Los controles de jugador permiten consultar las cuatro manos; solo el jugador activo puede ejecutar acciones. J1/J3 usan Zukgrok y J2/J4 Sahria por defecto. El morado identifica a Zukgrok y el amarillo a Sahria, incluso si se cambia la selección de Líder. Los números J1–J4 distinguen a jugadores con el mismo mazo.

## Controles

- **Carta → hexágono verde:** paga el coste, retira la carta de la mano y resuelve su efecto. Las cartas se amplían en el inspector.
- **Selección múltiple:** selecciona los objetivos y usa **Resolver selección** si quieres menos del máximo. Al completar el máximo se resuelve automáticamente. **Cancelar** no consume recursos.
- **Unidad propia:** muestra movimientos alcanzables. Pulsa un destino verde para mover. Usa **Atacar** para cambiar a objetivos de combate.
- **Casilla / Cancelar → bioma:** inicia una terraformación. El coste base se muestra en el inspector.
- **Revelar Tierra Ceniza:** selecciona un bioma normal propio y paga el coste de la regla de prototipo. Las Latentes solo se despliegan sobre ceniza propia y libre.
- **Estructura o unidad con habilidad:** usa el botón de habilidad del inspector. La Red Micelial permite seleccionar sus dos extremos.
- **Habilidad del Líder:** Zukgrok necesita una red micelial y un enemigo en Bosque conectado o adyacente a ella; Sahria elige hasta dos Desiertos propios.
- **Marcha Micelial:** elige una unidad situada en un extremo de una vía rápida y después el destino. Puedes resolver un par o añadir otra unidad y su destino.
- **Recursos compatibles SÍ/NO:** decide si se usan primero recursos de bioma para pagar cartas. Solo se gastan los que coinciden con las etiquetas de la carta.
- **Finalizar turno:** resuelve el fin de turno, rota de jugador, genera energía, resuelve veneno/producción y roba automáticamente.
- **Guardar/Cargar:** conserva la partida completa en `Application.persistentDataPath/war-conquer-save.json`, incluidos mazos, instancias, rutas, turnos y estado aleatorio.
- **Registro:** muestra cartas resueltas, daño, tiradas, movimiento, producción y cambios de turno.
- **+ / − / flechas / 1:1:** amplían y desplazan el tablero sin alterar las casillas.

## Tablero

El tablero es una retícula continua de **87 hexágonos que comparten sus bordes**, sin puentes ni separaciones de agua. J1 ocupa el norte (verde), J2 el este (amarillo), J3 el sur (rojo), J4 el oeste (azul), y el centro es gris cálido. Los territorios iniciales tienen **17 casillas** cada uno y el centro **19**. Cada zona comparte múltiples accesos con el centro y sus dos vecinos laterales.

Los colores iniciales distinguen territorios: todas las casillas siguen **sin bioma** hasta terraformarse. Al terraformar, el relleno muestra el bioma; la marca J1–J4 indica el control actual. Las unidades y los mazos conservan morado para Zukgrok y amarillo para Sahria. Los círculos son unidades, los cuadrados estructuras, `Vn` indica veneno, `Zz` sueño, `!` terreno inestable y `+1/+2` recurso estratégico central. Las vías rápidas se identifican con el mismo número `R` en ambos extremos, sin dibujar puentes.

**War & Conquer → Ver mapa en escena** permite inspeccionar el mapa antes de pulsar Play. Los guardados antiguos del mapa de islas no se cargan en esta versión; se conservan en disco y se puede iniciar una partida nueva.

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
- Ataque por alcance, auras, defensa, salud, muerte y descarte. Las bases reciben ataques de unidades, eliminan a su jugador al llegar a cero y gana el último Líder vivo.
- Producción de Esporas y fichas, evolución de Larva, Red Micelial, recursos de bioma y centro, habilidades de Líder y de piezas.
- Guardado/carga local con validación, HUD, consultas de mazo/descarte, colores por facción y escenario explícito de pruebas.

## Reglas de prototipo y decisiones por confirmar en balance

Los documentos no cierran todos los valores ni todas las ambigüedades. `Assets/WarConquer/Resources/WarConquer/rules.json` contiene la configuración general; `cards.json` contiene los datos de cartas y los rasgos de efectos.

- Mano inicial: 5. Robo: 1 por turno después del primer turno propio. Un mazo vacío deja de robar; no se ha añadido daño de fatiga.
- Energía: 3 en el primer turno propio, +1 máximo por turno propio hasta 10; se repone al comenzar el turno. La energía adicional de Trono Solar puede superar el máximo base.
- Vida del Líder: 25. Una unidad puede mover y atacar el turno en que se coloca; `summoningSickness` permite cambiarlo.
- Acciones de ataque, cartas, construcción y terraformación se realizan dentro de la fase de acciones. Inicio y fin de turno se resuelven automáticamente; fuera de acciones no se admiten órdenes.
- Terraformar: 1 Energía base, sobre terreno propio o adyacente al propio, sin transformar ceniza permanente ni una casilla ocupada por un enemigo. Los santuarios descuentan la primera terraformación de su bioma del turno.
- Se permite desplegar sobre casillas neutras propias al inicio (`allowNeutralDeployment`), para que una partida sin biomas pueda arrancar. Sobre un bioma ya definido se exige afinidad; las Latentes siempre exigen ceniza.
- Recursos: cada bioma normal propio genera al menos 1 por turno propio, con 2 en el recurso central principal. Tope 20 por bioma; solo sirven para cartas con su etiqueta. Los colores de territorio no generan recursos en casillas neutras.
- Alcance general de magias: 3 conexiones desde una unidad, estructura o base propia; configurable. Torre de Esporas amplía en 1 una magia de veneno por turno.
- Veneno: daño al inicio de dos turnos propios; reaplicar renueva duración y conserva la mayor intensidad. Sueño dura el próximo turno de su dueño y se retira al terminarlo.
- Tormenta de Arena reduce MOV en el próximo turno afectado de la unidad, para que tenga efecto en un juego sin interrupciones durante el turno rival.
- Revelar ceniza: acción de 3 Energías sobre bioma normal propio, rebajada por Templo de Ceniza. Además, hay cinco casillas ocultas marcadas (una por territorio) cuya destrucción revela ceniza.
- Cada paso cruza un borde compartido entre dos hexágonos. Los disparos usan distancia de grafo; no se ha definido línea de visión.
- Las estructuras normales bloquean el paso. Arena Profunda, Dunas Movedizas y Oasis de Cristal permiten una unidad encima de su casilla de terreno. Guardián del Oasis obtiene su defensa al estar sobre el Oasis.
- Fallar una tirada de entrada deja a la unidad en la casilla anterior, le causa el daño y termina su movimiento. Fallar al salir la mantiene en su sitio. Vuelo y compatibilidad evitan la tirada.
- Red Micelial elige inicialmente los dos terrenos compatibles más cercanos y permite reconfigurarlos con su habilidad. Hongo Explorador recibe su bonificación una vez por turno; Ciervo cruza sin coste adicional.
- Los efectos opcionales de entrada usan selección automática determinista de los primeros objetivos válidos. La interfaz permite seleccionar los objetivos de cartas jugadas, magias, líderes y habilidades activadas; decidir manualmente cada disparo de entrada queda como mejora posterior.
- Chamán extiende una mejora temporal de ATQ desde otra unidad adyacente a la unidad aliada elegida y consume 1 Espora. El documento no enumera qué otras clases de efecto pueden extenderse; ampliar esa selección requiere cerrar la regla.
- Las estadísticas de Espora, Espora Evolucionada y Obelisco de Arena no aparecen en la fuente: sus valores están identificados como configurables de prototipo.
- Mar de Arena dura dos cambios de ronda y requiere 5+ al entrar o salir. El daño de entrada no se especifica: se usa 1 como valor de prototipo.
- El control del hexágono central cuenta turnos de control; la victoria por puntos está desactivada por defecto (`centerVictoryScore = 0`). Se puede activar estableciendo un umbral.

Quedan pendientes el balance competitivo, arte final, audio, animaciones, IA rival, multijugador por red, información privada de manos y un editor visual de mazos. La partida local usa las 50 cartas de cada listado fijo.

## Arquitectura y archivos

Todo el sistema se añade bajo `Assets/WarConquer/` y usa el namespace `WarConquer`. La escena inicial `Assets/Scenes/SampleScene.unity`, la configuración de URP y las dependencias originales se conservan.

| Archivo | Responsabilidad |
|---|---|
| `Runtime/Model.cs` | Tipos de cartas, piezas, jugadores, turnos, recursos, reglas y estado serializable |
| `Runtime/CardCatalog.cs` | Carga y validación del catálogo, mazos, robo y pagos compatibles |
| `Runtime/BoardManager.cs` | 87 hexágonos continuos, adyacencia, distancia y regiones conectadas |
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

Las pruebas cubren mazos, neutralidad, distribución territorial, conectividad resistente a un bloqueo, pago y colocación atómicos, descarte, biomas, Latentes, energía/robo, veneno/sueño, movimiento, combate, terreno inestable, vuelo, rutas, Marcha, producción, habilidades, guardado y una partida automatizada de 120 turnos con comprobación de integridad después de cada acción.

Para exportar una compilación, añade la escena Graybox a los Build Profiles de Unity. El proyecto mantiene la escena original en sus ajustes para no alterar el trabajo previo del equipo.

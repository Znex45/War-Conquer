# Optimización de War & Conquer

Se optimizaron la lógica de consultas del tablero, la búsqueda de rutas, la cámara, la interfaz, el historial de Deshacer y los efectos. No se redujeron la resolución, las partículas, las animaciones, los materiales ni el número de triángulos. Los archivos originales se conservan.

## Mediciones de CPU en Unity Editor

Cada fila mide el mismo lote antes y después. Son pruebas locales de CPU, no una medición de FPS en un ejecutable final.

| Operación | Repeticiones | Antes (ms) | Después (ms) | Menos tiempo |
|---|---:|---:|---:|---:|
| Distancias por el grafo | 1500 | 140.87 | 32.27 | 77.1% |
| Enumeración de piezas | 1500 | 504.31 | 38.05 | 92.5% |
| Rutas de movimiento | 150 | 64.55 | 32.17 | 50.2% |
| Paletas de la interfaz | 5000 | 2.15 | 0.62 | 71.0% |
| Actualización de cámara | 300 | 189.95 | 106.10 | 44.1% |
| Sincronización del tablero | 15 | 109.67 | 42.47 | 61.3% |

## Modelos y renderizado

- Renderizadores de la escena de referencia: **1060 → 706** (33.4% menos).
- Cada bioma conserva una base hexagonal y una malla con submeshes por material. No se eliminaron materiales.
- Vértices de los siete biomas: **196,212 → 185,886**; se comparten únicamente vértices con atributos idénticos.
- Triángulos: **65,404 → 65,404**. La comprobación comparó todos los triángulos, posiciones, normales, UV, colores, tangentes y materiales.

## Conservación del comportamiento

- 146 comprobaciones existentes: mismos resultados antes y después; **124 pasan y 22 ya fallaban antes de esta optimización**. No hay fallos nuevos en la comparación. La suite antigua global se detiene en su primer fallo; también se ejecutaron individualmente las suites públicas listadas en los informes JSON.
- Dos simulaciones de IA de 64 decisiones: las ocho instantáneas de estado comparadas coinciden exactamente.
- 1.800 consultas de distancia contrastadas con BFS original en mapas de 2, 3 y 4 jugadores, incluidos límites y una topología desconectada.
- 40 comparaciones de rutas con el algoritmo anterior, conservando destinos y desempates.
- Compatibilidad entre las instantáneas compactas de Deshacer y el guardado anterior. El formato de guardado sigue siendo 2.
- Revisiones de biomas, cobertura hexagonal, neón, prioridades, reacciones y skyboxes superadas.
- Prueba real en Play Mode: inicio, menú, preparación, cámara, actualizaciones de Canvas y Deshacer superados. Se excluyó del detector una excepción del indexador UnityEditor.Search al iniciar el editor, ajena al código del juego.

## Cambios técnicos

- BFS usa buffers reutilizables; conserva el grafo real y los límites de alcance.
- La selección de rutas conserva el orden de desempate y evita ordenar la lista de candidatos repetidamente.
- Enumerar piezas ya no crea un array por casilla.
- La cámara y las paletas de la interfaz evitan arrays temporales repetidos.
- La interfaz conserva referencias a sus componentes de Canvas y al botón de Deshacer.
- El historial usa JSON compacto, conservando todas las decisiones y los datos de restauración.
- Los efectos actualizan solo los elementos animados o iluminados que necesitan trabajo; las partículas y luces se mantienen.

No se publicaron versiones ni se cambiaron los ajustes de calidad. No se afirma que los 22 fallos previos estén corregidos. El contador de bytes asignados no proporcionó datos útiles en este entorno y no se usa para afirmar mejoras de memoria medidas.
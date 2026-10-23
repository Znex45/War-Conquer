# Entornos importados

Los siete ZIP de terreno se integraron en los biomas Forest (bosque), Swamp (pantano), Desert (desierto), Tundra, Volcanic (Volcán), Wasteland (Yermo) y Void. AshLand reutiliza Yermo; Neutral conserva la casilla original.

Los originales que requieren conversión se conservan en Documentation/EnvironmentOriginals. Los archivos FBX/OBJ y las texturas importables están en Assets/WarConquer/EnvironmentSource. Los prefabs y materiales que usa el juego están en Resources/Environment.

La terraformación conserva la aparición gradual y las partículas de cada bioma. Los modelos importados conservan la forma del terreno; los detalles identificados como vegetación, agua o elementos cósmicos reciben movimiento suave sin deformar todo el diorama.

La cámara del tablero usa el panorama correspondiente a ZUKGROK, FAUNAR, SAHRIA o XIL'THAR. Sigue primero la elección pendiente, después la prioridad de combate, la reacción y finalmente el turno activo. Al cerrar una reacción vuelve al líder del turno. No depende del jugador cuya mano se esté consultando.

En Unity: War & Conquer > Entornos > Importar biomas y skyboxes regenera los recursos. Comprobar y capturar entornos valida los siete prefabs, los límites de tamaño, los materiales, los cuatro fondos y las transiciones de prioridad. Las capturas se guardan en Library/EnvironmentQA.

Los skyboxes usan las imágenes panorámicas de los ZIP; sus mallas FBX originales también se conservan. El fondo se asigna a la cámara del tablero y no cambia RenderSettings globales.

## Ajuste de las casillas completas

Los terrenos ahora se amplían para cubrir la huella del hexágono, con altura ajustada por separado. Se recorta una sección central de los modelos rectangulares contra los seis planos del hexágono. El recorte interpola las coordenadas UV, los colores y las normales de los vértices nuevos, y conserva los submeshes y sus materiales.

Una superficie hexagonal con el material predominante del suelo cierra los huecos de los dioramas redondos o irregulares. Las mallas resultantes se guardan como recursos compartidos en Resources/Environment/Meshes. El recorte se realiza en el editor, sin calcular geometría durante cada turno.

Para regenerar: War & Conquer > Entornos > Ajustar terrenos al hexagono. La comprobación verifica la cobertura de las seis esquinas, que ningún vértice salga del hexágono, los UV y el tamaño de los siete biomas.

## Partículas e iluminación neón

Cada bioma emite partículas con una paleta representativa de sus materiales y texturas (vegetación, arena, hielo, lava y detalles de Void). Se usa un shader aditivo con núcleo y halo, sin depender de Bloom global. Las partículas nacen por encima del relieve, con un máximo de 18 por casilla y tiempo independiente de la velocidad del juego.

Las casillas del jugador con prioridad y su plataforma de líder tienen contornos neón pulsantes del mismo color que la base. Dos luces locales sin sombras iluminan su base y su líder. Se reutilizan materiales compartidos; no se añade una luz por casilla.

El color y el fondo siguen la elección pendiente, la prioridad de combate, la reacción o el turno, en ese orden. Al terminar la reacción vuelve la iluminación del jugador activo. Los efectos no modifican las reglas ni el estado de la partida.

War & Conquer > Entornos > Comprobar neon y turnos verifica los cuatro colores, los contornos, el pulso de luz, las partículas, las transiciones y la limpieza al iniciar otra partida. Capturas y resultado: Library/NeonQA.

# Actualización de SAHRIA y ZUKGROK

22 de septiembre de 2026 · Unity 6000.6.2f1 · estado de partida versión 7.

**SAHRIA: 50 cartas. ZUKGROK: 50 cartas. FAUNAR: 50 cartas conservadas.**

Cleansing Conquest fue eliminada del mazo de Zukgrok.
Alligator, Revengeful Bite fue aumentado a 3 copias.
El efecto final del Alligator Token está incompleto y requiere definición.

## 1. Archivos modificados

- `Assets/WarConquer/Editor/RevisionVisualPreview.cs`: comprobación del resultado del dado basada en fotogramas renderizados, sin depender de una espera fija del editor.
- `Assets/WarConquer/Editor/Board3DPlayTests.cs`
- `Assets/WarConquer/Editor/FaunarVisualPreview.cs`
- `Assets/WarConquer/Editor/GrayboxTests.cs`
- `Assets/WarConquer/Editor/InterfacePlayModeTests.cs`
- `Assets/WarConquer/Editor/InterfaceRefinementTests.cs`
- `Assets/WarConquer/Editor/RevisionPlayTests.cs`
- `Assets/WarConquer/Editor/RevisionRulesTests.cs`
- `Assets/WarConquer/Runtime/AbilityManager.cs`
- `Assets/WarConquer/Runtime/AiPlayer.cs`
- `Assets/WarConquer/Runtime/CardCatalog.cs`
- `Assets/WarConquer/Runtime/ChoiceManager.cs`
- `Assets/WarConquer/Runtime/CombatManager.cs`
- `Assets/WarConquer/Runtime/EffectManager.cs`
- `Assets/WarConquer/Runtime/EnergyManager.cs`
- `Assets/WarConquer/Runtime/GameManager.cs`
- `Assets/WarConquer/Runtime/GenericCardRules.cs`
- `Assets/WarConquer/Runtime/Model.cs`
- `Assets/WarConquer/Runtime/MovementManager.cs`
- `Assets/WarConquer/Runtime/PrototypeScenario.cs`
- `Assets/WarConquer/Runtime/TerrainManager.cs`
- `Assets/WarConquer/Runtime/TimingRules.cs`
- `Assets/WarConquer/Runtime/TurnManager.cs`
- `Assets/WarConquer/Resources/WarConquer/cards.json`
- `Assets/WarConquer/Runtime/UI/Board3DScene.cs`
- `Assets/WarConquer/Runtime/UI/BoardDiceVisual.cs`
- `Assets/WarConquer/Runtime/UI/BoardMeshFactory.cs`
- `Assets/WarConquer/Runtime/UI/BoardStatusVisual.cs`
- `Assets/WarConquer/Runtime/UI/CardPresentation.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.AI.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.Cards.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.Choices.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.Dialogs.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.Status.cs`
- `Assets/WarConquer/Runtime/UI/WarConquerController.Turn.cs`
- `README_WAR_CONQUER.md`.

## 2. Archivos creados

- `Assets/WarConquer/Editor/FactionPlayTests.cs`
- `Assets/WarConquer/Editor/FactionRulesTests.cs`
- `Assets/WarConquer/Editor/FactionVisualPreview.cs`
- `Assets/WarConquer/Editor/LegacyTestCatalog.cs`
- `Assets/WarConquer/Runtime/FactionCardRules.cs`
- `Assets/WarConquer/Editor/Fixtures/cards-before-sahria-zukgrok.json`
- Este informe y metadatos `.meta` de Unity correspondientes.

## 3. Sistemas reutilizados

Estado serializable, catálogo, instancias, mazos, energía, recursos, tablero hexagonal, biomas, movimiento, combate, habilidades, efectos, turnos, elecciones, respuestas, guardado, validador, IA, Canvas y modelos 3D. Se mantiene la interfaz existente.

## 4. Sistemas nuevos

`FactionCardRules` concentra los disparadores de las nuevas cartas. `deckCopies` permite compartir una definición de carta entre mazos con cantidades distintas. `setBy` registra procedencia del terreno. Los modificadores temporales de HP/ATK y la contabilidad de MOV gastado se serializan. Se amplían las elecciones existentes para invocación, acompañamiento y terraformación de muerte; no se crea un segundo motor.

## 5. Cartas de Sahria implementadas

| Copias | Carta | Tipo | Energía | MOV / HP / ATK |
|---:|---|---|---:|---|
| 3 | Terraform | Spell | 1 | — |
| 3 | Clearing Space | Spell | 1 | — |
| 3 | Cleansing Conquest | Spell | 3 | — |
| 3 | One for the Team | Spell | 2 | — |
| 3 | A Price to Pay | Spell | 4 | — |
| 1 | From the Ashes | Spell | 4 | — |
| 3 | Engage | CSpell | 2 | — |
| 3 | Time to Move | CSpell | 1 | — |
| 2 | Bomb | CSpell | 2 | — |
| 2 | Valiant Seal | Structure | 3 | 0 / 3 / 0 |
| 2 | Courage Seal | Structure | 2 | 0 / 2 / 0 |
| 2 | Med Camp | Structure | 4 | 0 / 5 / 0 |
| 1 | Resting Camp | Structure | 5 | 0 / 3 / 0 |
| 2 | Vigilant Tower | Structure | 4 | 0 / 2 / 0 |
| 1 | Emergency Seal | Structure | 3 | 0 / 2 / 0 |
| 3 | Sandy Anthill | Structure | 2 | 0 / 3 / 0 |
| 3 | Sandy Ember | Unit | 2 | 3 / 2 / 2 |
| 3 | Dune Cammel | Unit | 2 | 4 / 2 / 1 |
| 3 | Old Mummy | Unit | 4 | 0 / 5 / 4 |
| 3 | Wandering Beast | Unit | 4 | 2 / 5 / 5 |
| 1 | Baby Phoenix | Unit | 6 | 2 / 1 / 7 |

Battle Ant Token: 1 / 1 / 2; no forma parte de las 50 cartas. Sandy Anthill tiene 3 HP confirmados por el usuario.

## 6. Cartas de Zukgrok implementadas

| Copias | Carta | Tipo | Energía | MOV / HP / ATK |
|---:|---|---|---:|---|
| 3 | Terraform | Spell | 1 | — |
| 3 | Clearing Space | Spell | 1 | — |
| 3 | One for the Team | Spell | 2 | — |
| 3 | A Price to Pay | Spell | 4 | — |
| 1 | From the Ashes | Spell | 4 | — |
| 3 | Resourceful Replenish | Spell | 1 | — |
| 3 | Last Gift | Spell | 2 | — |
| 3 | Engage | CSpell | 2 | — |
| 3 | Time to Move | CSpell | 1 | — |
| 2 | Bomb | CSpell | 2 | — |
| 3 | Valiant Seal | Structure | 3 | 0 / 3 / 0 |
| 3 | Courage Seal | Structure | 2 | 0 / 2 / 0 |
| 3 | Vigilant Tower | Structure | 4 | 0 / 2 / 0 |
| 1 | Mana Pool | Structure | 4 | 0 / 1 / 0 |
| 3 | Frogpit | Structure | 3 | 0 / 4 / 0 |
| 3 | Micelium Root | Structure | 3 | 0 / 3 / 0 |
| 2 | Firefly Nest | Structure | 3 | 0 / 3 / 0 |
| 2 | Lushroom | Unit | 3 | 0 / 5 / 1 |
| 3 | Alligator, Revengeful Bite | Unit | 5 | 1 / 4 / 1 |

Tokens MOV / HP / ATK: Frog 2/2/2, Mushroom 1/2/2, Firefly 4/1/1, Alligator 1/1/1. No forman parte del mazo ni del descarte. HP confirmados por el usuario: Frogpit 4, Micelium Root 3, Firefly Nest 3.

## 7. Habilidad de Sahria

Opción SAHRIA +1 E en el panel del Líder. Detecta Units, valida el Slab establecido, suma un coste real de energía antes de comprobar fondos y resuelve la entrada normalmente. No recarga Structures ni Spells. Un objetivo o pago rechazado no consume cartas, recursos ni energía.

Interpretación del territorio consistente con el control por casillas: cada Slab terraformado registra a su autor; las siete casillas iniciales pertenecen al establecimiento inicial. Una conquista por movimiento cambia el dueño, no el autor del bioma. La habilidad permite otros biomas en esos Slabs, conservando restricciones explícitas y ocupación. Destruir el bioma elimina la procedencia. No se impone una distancia nueva.

## 8. Habilidad de Zukgrok

Solo la muerte de un Token propio encola una elección de bioma en su Slab de muerte. El coste es 0. La elección corresponde al dueño del Token incluso durante el turno enemigo. Se respeta la protección existente de ceniza, ocupación y bases. Si el Slab no puede terraformarse, se registra el impedimento y se continúa el resto del efecto.

## 9. Poison

1 daño directo al inicio del turno del controlador, sin duplicación ni acumulación por reaplicar. Lushroom enemigo en Pantano lo aumenta a 2, sin sumarlo por cada Lushroom. Bloquea curación solamente cuando el objetivo es enemigo, está envenenado y está sobre Pantano mientras el Lushroom también está sobre Pantano. Toda curación pasa por la comprobación común. La calavera morada y el rótulo reflejan el daño efectivo.

## 10. Tokens

Instancias independientes creadas al resolver una invocación, sin tomar cartas del mazo. Los productores invocan al entrar, porque su texto dice Summon y no start of turn; Berry Bush conserva su disparador al inicio del turno. El jugador elige casillas adyacentes libres. Firefly Nest permite dos elecciones sucesivas; si solo cabe una, crea una; si no hay espacio, no sobrescribe piezas. Alligator invoca opcionalmente en un Slab propio libre usando la colocación existente.

## 11. Muerte de Units

Todas las causas pasan por `CombatManager.Remove`, que evita una segunda destrucción de la misma instancia. Wandering Beast roba 4. Baby Phoenix queda registrado como carta pendiente, terraforma y vuelve con el mismo ID a mano, sin descarte ni duplicado. Su estado intermedio se puede guardar y cargar. Alligator cura 1 ante la muerte de una Unit no Token, de cualquier jugador, sin superar el máximo ni ignorar Lushroom.

## 12. Muerte de Tokens

El indicador `token` separa su muerte de la de Units de mazo. Activa Zukgrok y no añade un descarte. Firefly conserva el ID de su objetivo; primero se sacrifica, después se resuelve el bioma gratuito y por último Poison si el objetivo sigue vivo. No se atribuye a Tokens el disparador de curación o de muerte de Units de Alligator.

## 13. Radios

RAD consulta distancia axial real. Ant 3, Cammel 1, Beast 3, Mushroom 2 más otros Mushroom Tokens propios, Firefly 2, Engage 6, Time to Move 8, Med Camp 5, Resting Camp 4 y Vigilant Tower 6. El alcance no depende de MOV ni de rutas rápidas. Sandy Ember puede seleccionar cualquier Structure sobre Desierto sin restricción de dueño o distancia.

## 14. Biomas

Se reutilizan Desert/Desierto, Swamp/Pantano, Forest/Bosque y Wasteland/Yermo. Yield no es un recurso ni un Token. Wandering Beast exige Yermo incluso con Sahria. From the Ashes afecta Yermo y usa la destrucción existente; no se confunde con Tierra Ceniza. Sandy Anthill no recibe una restricción a Desierto que no figura en su texto.

## 15. Modificadores

Alligator recalcula +1 HP y ATK por Token propio vivo, preservando daño. Battle Ant recibe un único +1 ATK si otra Ant está a 3rad, incluidas Structures con ese subtipo. Frog y Alligator Token ganan +2/+3 MOV solo en Pantano. El movimiento de aura ya gastado no reaparece al salir y volver al bioma. Old Mummy obtiene +1 MOV para ese turno y se cura al inicio en Desierto. Time to Move se retira al terminar el turno actual.

Dune Cammel utiliza la dirección axial de cada desplazamiento. La Unit acompañante debe caber en el destino correspondiente. No genera cadenas de acompañamiento. La salida de Desierto se interpreta como pasar a otro bioma, igual que la transición de Panther; afecta aliados a 1rad del origen y dura hasta el inicio del próximo turno de su controlador. Los bonus expirados no alteran las estadísticas base.

## 16. Energía

Una sola cotización alimenta carta, inspector, validación y descuento real. Sahria añade +1 energía después de los recursos compatibles; los recursos no sustituyen ese recargo de energía. Faunar conserva su descuento de la primera Structure. Mana Pool añade 1 energía al inicio sin incrementar permanentemente el máximo. Los Tokens de efectos no pagan coste de carta no especificado.

## 17. Pruebas realizadas

145 pruebas de reglas completas aprobadas: regresión histórica con fixture de Editor, Faunar y genéricas, nuevos mazos, costes, biomas, radios, invocaciones, Poison, curación, muerte, retorno de Phoenix, modificadores, guardado y decisiones de IA. El catálogo real se verifica en 1, 2, 3 y 4 personas. Play Mode comprueba botones reales, recargo visible, despliegue, dos Fireflies seleccionados, sacrificio, selector de bioma, calavera, elección múltiple de Engage, mazos, modelos, movimiento por clic y dados.

Capturas de comprobación en `Library/WarConquer3D`: `mazos-actualizados.png`, `sahria-actualizada.png`, `zukgrok-terraformacion-token.png`, además de las capturas de mapas y Faunar.

## 18. Errores encontrados

Los mazos anteriores y las habilidades activas de Líder no correspondían al nuevo documento. Poison mantenía la progresión antigua. No existían elecciones para estos Tokens ni conservación del Phoenix en tránsito. Recuperar un aura de MOV después de gastarla podía devolver movimiento incorrectamente. Las pruebas gráficas también mostraron una excepción interna de indexación de `UnityEditor.Search.SearchDatabase` al arrancar el editor; no procede del juego y no impidió las comprobaciones.

## 19. Errores corregidos

Composición por Líder compartiendo diseños, efectos reales, recargo único, procedencia del terreno, Poison constante, bloqueo de curación, disparadores de muerte, invocación elegida, retorno único de Phoenix, restricciones de Yermo, radios y expiración. Se evita el bucle de Cammels y la recuperación indebida de MOV de aura. Las elecciones sin objetivos permiten continuar y la IA resuelve las nuevas elecciones con las mismas reglas que un humano. Se actualizaron escenarios, textos y pruebas que referían mazos sustituidos.

## 20. Funcionalidades pendientes

**El efecto final del Alligator Token está incompleto y requiere definición.** No se implementa «When I die, give +1hp» hasta definir destinatario, duración y condiciones. Se implementa únicamente su +3 MOV en Pantano y sus estadísticas dadas. Las cuatro vidas faltantes de Structures quedaron resueltas con los valores expresamente confirmados por el usuario. Arte final, red y balance competitivo permanecen fuera de esta actualización.

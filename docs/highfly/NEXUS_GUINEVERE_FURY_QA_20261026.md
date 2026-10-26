# HIGHFLY NEXUS — Guinevere + FURY 360° 2.0: prueba Android

Estado: primer WebGL GREEN (RUN 40313125726) con escena RG POLY, Guinevere, botón FURY, dos apariencias Quaternius. **No constituye validación de jugabilidad.**

## Preview aislada
- RUN corregido y con automatización de Pages beta: https://github.com/drakentzy07/HIGHFLY-NEXUS-UNITY/actions
- La publicación, si tiene éxito, usará `/HIGHFLY-NEXUS-UNITY/preview/guinevere-fury/`.
- La raíz `/HIGHFLY-NEXUS-UNITY/` mantiene los binarios de City01 previo; la beta se añade en subcarpeta.
- Si la nueva CI o despliegue falla, no hay beta publicada: no anunciarla como accesible.

## Controles objetivo
- **Joystick izquierdo:** movimiento relativo a cámara en 360°, también diagonales.
- **Arrastre en lado derecho:** cámara libre (no secuestra el stick).
- **SALTAR ×2 MORTAL:** primer toque salto, segundo toque mortal en dirección de movimiento. Probar adelante, atrás, izquierda y derecha.
- **DASH:** evasión rápida independiente del salto.
- **ATK:** básico con cadena 1–2–3–4, testar caminando, corriendo y en aire.
- **S1 PRISMA:** proyectil, marcas, ralentización; arrastrar para apuntar y soltar.
- **S2 RUPTURA:** salto de daño y segunda activación; arrastrar y soltar.
- **S3 TEMPESTAD:** pulsos de área y combo contra enemigos levantados.
- **CANCELAR SKILL:** llevar el gesto a la zona superior derecha antes de soltar.
- **OBJETIVO:** elegir AUTO / CERCANO / MENOS HP; ataques deben apuntar al objetivo cuando corresponda; habilidades lanzadas fuera del enemigo deben fallar.
- **CAMBIAR HÉROE / EQUIPO:** alternar modelos Quaternius incluidos. *Es cambio de preset completo, no aún equipamiento modular por piezas.*
- **PC:** WASD, Espacio (doble salto), Shift (dash), J (ATK), 1/2/3 (skills).

## Zonas y colisiones
- Ciudad RG POLY: NPC, edificios, portales e interiores, SIN enemigos hostiles dentro del casco urbano.
- Primer esqueleto: zona exterior de bosque, ancla FOREST `(-60, 13.35, 22)` en el generador Unity, en vez de junto al Hunter inicial.
- Confirmar que el enemigo NO aparece dentro de la muralla ni bajo el suelo. El mapa tiene distintos niveles de terreno.
- Evitar agregar parkour/escalada todavía; será fase siguiente cuando los cuatro hitos hayan pasado QA.

## Aceptación en Samsung S23 Ultra, horizontal
1. Carga completa en WebGL sin pantalla negra ni pérdida de contexto gráfico.
2. Moverse y rotar cámara **simultáneamente** sin inversiones ni zonas de toque muertas.
3. Salto, doble salto y mortal, comprobar dirección correcta; no rotar físicamente el CharacterController.
4. Dash desde quieto, corriendo y desde el aire. Ningún bloqueo definitivo del motor.
5. ATK 1-2-3-4 y S1/S2/S3 en movimiento; animación tronco/brazos compatible con piernas caminando.
6. Guinevere VFX legibles y fluidos; colisiones reales y barra de HP del esqueleto.
7. Quaternius: cuerpo masculino/femenino visibles, clips Idle/Run/ATK/Cast ligados al mismo root; sin duplicaciones de visuales.
8. FPS/performance, temperaturas, carga en redes móviles, interfaz y posibilidad de pantalla completa.

### Bloqueos explícitos
- No se ha hecho prueba real con S23 Ultra.
- Combos aéreos, tiempos de cooldown, salto mortal y animaciones requieren ajuste jugable; WebGL compilado no basta.
- Las armaduras individuales y parkour/escalada se consideran trabajo futuro.
- Mantener el PR en borrador y la versión estable sin sustituir.

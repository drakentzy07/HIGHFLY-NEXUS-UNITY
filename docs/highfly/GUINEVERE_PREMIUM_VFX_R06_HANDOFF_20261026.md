# HIGHFLY NEXUS UNITY — GUINEVERE PREMIUM VFX R06 (aislado)

**Fecha:** 2026-10-26  
**Rama:** `feature/nexus-guinevere-premium-vfx-r06-isolated-20261026`  
**Base exacta:** `8df089128faf8e563af84bc470b8889b6a042c32`  
**Estado:** Código VFX integrado en rama aislada; **Unity/WebGL/S23 SIN compilar ni probar** en esta rama. No publicar ni fusionar hasta integración con FURY verificada.

## Reglas de aislamiento

- No tocar `HighflyThirdPersonMotor.cs`, `HighflyGuinevereCombat.cs`, animator/Quaternius installer, HUD, World/CITY01, escenas ni workflows.
- Quaternius CC0 es el personaje actual. KayKit únicamente conserva función de contingencia en el instalador existente.
- Unity C#, no Three.js. Partículas, LineRenderer, snapshot de malla generados sin materiales o texturas propietarios.
- La simulación de daño, marcas, cooldown, gasto de MP, lanzamiento/caída y orientación sigue en Combat/FURY. El VFX es una capa secundaria y se puede apagar.
- No incorporar arte ni código de Mobile Legends. Las cinco grabaciones son referencias de tiempo y mecánica.

## Implementación

### Nuevo archivo
`Assets/Highfly/Combat/HighflyGuineverePremiumVfx.cs`

- `PrismaSpawn`: halo transparente secundario parentado al proyectil **existente**, sin moverlo.
- `Launch`: dos anillos de impulso en distintos planos y estallido al despegar.
- `Impact`: destellos de núcleo y dos anillos con radios ajustables.
- `Mark`: aura visible por acumulaciones (1–3).
- `Lift`: partículas ascendentes y anillos a dos alturas.
- `Basic`: golpe de cierre / básico cargado.
- `Blink`: puntos luminosos distribuidos a lo largo del desplazamiento; **no** mueve al personaje.
- `Echo`: un snapshot visual de la pose de Quaternius (hasta 2 SkinnedMeshRenderer, límite 22.000 vértices por malla), sin IA, collider ni daño. Si la malla no está disponible sigue el efecto de luz. Las mallas se destruyen al finalizar o deshabilitar.
- `Tempestad`: 11 flashes provenientes de las **11 llamadas reales** de Combat; anillos especiales en los ticks 4/8/11, remate mayor y órbita visual alrededor del Hunter.
- Pools de partículas y LineRenderer; límites por perfiles `Fluido / Equilibrado / Ultra`, sin descargas externas.

### Adaptador mínimo
`Assets/Highfly/Combat/HighflyGuinevereVfx.cs`

Se incorpora `HighflyGuineverePremiumVfx` de forma automática desde `Awake`, se propaga `SetQuality` y se duplican **solo los eventos visuales** a la nueva clase. La clase base y sus firmas públicas siguen intactas; si se deshabilita el componente nuevo, los VFX originales siguen disponibles.

## Validación previa a integración (no afirmar GREEN)

1. Ejecutar `python3 scripts/check_guinevere_r06_vfx_contract.py` (auditoría estática, **no** compilación).
2. Rebasar/cherry-pick **solo los dos archivos de VFX** sobre el commit FURY aprobado, tras revisar los conflictos.
3. Compilar Unity 6 en rama aislada; verificar que shaders Built-In se resuelvan en el perfil gráfico WebGL. Si el proyecto usa URP y alguno no se resuelve, implementar materiales/shaders compatibles antes de declarar apto.
4. Verificar instalación Quaternius, prefabs y `Awake` en runtime real.
5. S23 Ultra horizontal, perfil Fluido → Equilibrado → Ultra: probar U1 (estela real, sin orbes huérfanos), U2 salto, lift y recast, U3 once señales de impacto, pasiva y básicos.
6. Revisar `Memory Profiler`/Unity profiler: conteo de partículas, pools, GC al emitir, FPS, ghost Mesh al recast, cambios de calidad y desactivación a mitad de habilidad.
7. A/B deshabilitar `HighflyGuineverePremiumVfx`, confirmar mismo daño, movimiento y cooldown.
8. No publicar Pages ni reemplazar la CITY01 estable antes de aprobación específica.

## Riesgos y límites

- **No fue compilado en Unity**: una auditoría estática no valida APIs, shaders, ni visual final.
- Esta R06 añade efectos sobre la capa R05 actual: quizá haya exceso de brillo/partículas; calibrar tras video del teléfono, no por estimación.
- El snapshot de Quaternius es **una postimagen estática de pose**, no una réplica animada; para Hayabusa necesitaremos el sistema separado de cuatro anclajes y réplicas.
- No modifica ni corrige FURY, Root Motion, foot pose, dash ni animaciones físicas.
- Shader fallback no equivale a certificación de renderizado real. Preferir ShaderGraph URP o materiales permitidos al confirmar el pipeline.
- Las 23 VFX Three.js de HIGHFLY R27/R29 son referencia de diseño; no se importan directamente aquí.

## Prueba de integración cuando FURY esté listo

`U1 → U2 → U3 (11 impactos) → recast U2`; repetir al correr y en salto. En paralelo, confirmar ataque básico 1-2-3-4. Asegurarse de que al fallar U1 **no se invente** impacto ni daño y que el costo se tramite únicamente en el Sim.

**Esta rama no es una release.**

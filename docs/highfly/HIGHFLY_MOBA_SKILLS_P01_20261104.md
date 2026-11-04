# HIGHFLY MOBA SKILLS LAB P01 — 2026-11-04

## Estado real
**Código P01 implementado en rama aislada; compilación Unity 6 / WebGL y prueba S23 PENDIENTES.**
No se modificó main, CITY01 ni el controlador FURY. No es una página publicada.
Base tomada de la integración FURY + Guinevere (commit 9fb2929576f480aebcd31cec3ce1875d7c4e2de9).

## Qué incluye esta entrega
- 3 kits originales inspirados en arquetipos de MOBA, SIN replicar nombres comerciales, assets, clips o VFX de Mobile Legends.
- Nueve acciones activas (S1, S2, S3 por kit), recast en S2 de cada kit.
- Casteos instantáneos con eventos diferidos de impacto; 11 golpes reales en la definitiva aérea.
- FURY sigue siendo la única autoridad para el desplazamiento del jugador (`TryDash`, `Jump`, `FaceForAction`).
- Enemigos: daño por `HighflyHealth`, elevación/aturdimiento lógico por `HighflySimpleEnemyAI.ApplyAirborne`,
  ralentización y desplazamiento con colisión del `CharacterController` del NPC.
- Apuntado en PC / Android: pulsar y soltar / mantener-arrastrar-soltar; cancelar por botón o esquina superior derecha;
  auto-target existente como fallback.
- VFX originales procedimentales, agrupados y con límites de objetos, sin texturas ni audio de terceros.
- HUD español con cooldown, recast y selector de estilo.
- Consumo mágico = `Mana`. Consumo físico = `Stamina` de la base Unity, etiqueta conceptual «Energía».
- ATK 1-2-3-4 preexistente se conserva: no se desactiva `HighflyGuinevereCombat` para que
  su callback/Update sigan funcionando, pero sus botones de S1/S2/S3 se deshabilitan SOLO en la escena copia.

## Skills P01 (VALORES DEL LAB, NO VALORES DE MOBILE LEGENDS)

| Kit | Skill | Mecánica | Coste | Cooldown |
|---|---|---|---:|---:|
| Hechicera aérea | S1 Lanza Violeta | Proyectil 28u/s, daño 65, slow 1.3s | 14 MP | 4.4s |
| Hechicera aérea | S2 Ascenso | Jump + dash FURY 7u/0.42s, daño área 84, levitación 1.05s, recast 4.4u | 23 MP | 15s |
| Hechicera aérea | S3 Tempestad | 11 ticks en 2s, daño 22 por tick + 55 último, suspensión reactivada | 40 MP | 40s |
| Asaltante abisal | S1 Cortes de furia | Cono 6.7u/47°, daño 54 | 18 Energía | 5s |
| Asaltante abisal | S2 Paso depredador | Dash 6.3u, daño 60, marca de vulnerabilidad +20%; recast/segundo golpe | 25 Energía | 12s |
| Asaltante abisal | S3 Corte final | Golpe de línea, daño 105 + 85 * fracción de vida faltante | 42 Energía | 32s |
| Guardián gravitacional | S1 Onda de hierro | Cono 7u/57°, daño 50, slow | 18 Energía | 6s |
| Guardián gravitacional | S2 Embestida | Dash 5.9u, empuja 0.8u con colisiones; recast aéreo | 25 Energía | 14s |
| Guardián gravitacional | S3 Colapso | Vórtice radio 4.9u/0.72s, atrae enemigos, 105 de daño y suspensión | 42 Energía | 44s |

**Notas:** cooldowns por kit no se resetean al alternar; recast S2 disponible 3.5s tras el primer cast,
sin coste adicional. Los valores son una primera propuesta de balance, no un producto final.

## Cómo abrirlo en Unity 6
1. Abrir `drakentzy07/HIGHFLY-NEXUS-UNITY` en rama
   `feature/moba-skills-vfx-lab-p01-20261104` (sin cambiar ni fusionar main).
2. Abrir una escena guardada de FURY + Guinevere con el
   `HighflyThirdPersonMotor`, Canvas HUD y EventSystem existente. Guardar cualquier
   cambio previo antes de generar el laboratorio.
3. Elegir el menú de Unity: **HIGHFLY > Skills Lab P01 > Crear escena de pruebas aislada**.
4. El instalador duplica la escena y SOLO abre/modifica:
   `Assets/Highfly/SkillsLab/Scenes/HIGHFLY_MOBA_P01.unity`.
   Si ya existe, no la sobreescribe.
5. Play: usar joystick/cámara heredados, botones AEREA / ABISAL / GUARDIAN,
   S1/S2/S3 y CANCELAR. Los botones S1/S2/S3 antiguos quedan bloqueados
   en esta escena copia, pero se conserva ATK básico.
6. Probar WebGL horizontal y Samsung S23 Ultra antes de asignar estado «estable».

## Límites actuales (no ocultar)
- **Sin build Unity verificado** en esta entrega: hay controles de coherencia estática,
  pero falta importar y compilar en Unity 6 / ejecutar los Play Mode tests.
- S2 «Ascenso» usa `Jump + TryDash` existentes: la trayectoria no es todavía una
  parábola física programable como la referencia; FURY no fue alterado.
- Los FX son originales para Unity y NO una importación directa de las 100
  recetas Three.js (NixieFX y Wawa son referencias separadas).
- Los FX pueden requerir incluir shaders de partículas en el proyecto si se
  recortan durante la exportación WebGL; comparar calidad 0/1/2.
- La atracción y empuje solo afectan a NPC con `CharacterController`, y se
  debe comprobar en vivo su convivencia con la IA, colisiones y poise de jefes.
- No hay clips KayKit propietarios nuevos aquí; permanece el avatar legal del
  laboratorio Unity (Quaternius CC0 según la preparación existente).
- Sin iconos, audio, texturas ni animaciones copiadas de Mobile Legends.
- El diseño de ataque aéreo + cancel windows/física de animaciones necesita
  la validación del FURY general antes de considerarse «full production».

## Checklist de QA (para marcar MANUALMENTE)
- [ ] Unity 6 compila sin errores.
- [ ] Escena fuente y main no cambian; la escena duplicada abre.
- [ ] Joystick y cámara funcionan simultáneamente en S23 horizontal.
- [ ] ATK 1-2-3-4 funciona junto a las 9 skills.
- [ ] Arrastrar/soltar/cancelar no consume recurso al cancelar.
- [ ] Recast S2 en los tres kits; no recastea tras expirar.
- [ ] Guinevere-like: S2 levanta -> S3 causa exactamente 11 ticks -> S2 recast.
- [ ] Dyrroth-like: vulnerability 20% y finalizador de vida faltante.
- [ ] Tigreal-like: empuje, segundo levantamiento y atracción con colliders.
- [ ] No atraviesa obstáculos, no duplica daño por colliders de un mismo enemigo.
- [ ] FPS y overdraw medidos en calidad 0/1/2 en Samsung S23 Ultra.
- [ ] WebGL sin errores de shader, inputs ni bloqueo de carga.

## Licencias
Implementación del laboratorio: código HIGHFLY propio. VFX: materiales
y partículas procedimentales, sin assets externos añadidos.
Si se importan posteriormente ejemplos de NixieFX, Wawa VFX o Three.js VFX,
auditar cada textura/FBX/audio individualmente antes de distribuir.
Referencias: no copiar arte ni animaciones protegidas de personajes de MLBB.

## Archivos P01
- `Assets/Highfly/SkillsLab/HighflyMobaSkillsLab.cs`: hitboxes, costes, daños, CC, cooldown/recast.
- `Assets/Highfly/SkillsLab/HighflyMobaSkillFx.cs`: visuales procedimentales solamente.
- `Assets/Highfly/SkillsLab/HighflyMobaAimButton.cs`: drag/release/cancel.
- `Assets/Highfly/SkillsLab/HighflyMobaSkillHud.cs`: estado visible de skills.
- `Assets/Highfly/SkillsLab/Editor/HighflyMobaLabInstaller.cs`: instalador copy-on-write.

Se excluye cualquier modificación de `Assets/Highfly/Core/HighflyThirdPersonMotor.cs`,
la escena CITY01 original y los scripts históricos de Guinevere.

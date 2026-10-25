# HIGHFLY NEXUS — Proyecto Violeta R05 → Unity

## Fuente y alcance
Port del archivo aportado por el usuario: HIGHFLY_PROYECTO_VIOLETA_R05_VFX_3D(1).html.
El HTML contiene una versión R02 anterior, el motor efectivo UniqueGuinevereLab R04 y la
capa visual tridimensional R05. Adoptamos los valores efectivos R04/R05, no los R02.
No se incluyen archivos, personajes, código ni VFX de Mobile Legends.

## Estado real (25 de octubre de 2026)
- Implementado en C#: combate Prisma U1, Ruptura U2 (salto y recast),
  Tempestad U3 (11 impactos), Super Magic y ATK 1-2-3-4.
- VFX propios nativos de Unity (partículas, anillos, estelas y eco)
  con escalas FLUIDO, EQUILIBRADO y ULTRA.
- S1/S2 aceptan apuntado por arrastrar/soltar; cancelar al soltar
  arriba a la derecha; tap usa apuntado asistido.
- Comportamiento de enemigos: slow, control aéreo y poise de jefes.
- Se conecta todo mediante HighflyCombatLabBuilder al generar la escena.
- La versión de Pages existente NO se modifica.
- Compilación Unity, funcionamiento en Android, WebGL y rendimiento SIN VERIFICAR.

## Parámetros fieles al laboratorio
| Skill | Mecánica |
| --- | --- |
| U1 Prisma | MP14; CD4s; velocidad25; rango15; daño78; slow al 75% durante1.2s; hasta3 marcas por5s; refund1s cooldown al acertar |
| U2 Ruptura | MP23; CD17s; salto hasta8m en0.42s; AoE2.1m; daños96+48; levanta1s; recast durante5s con blink4.7m y eco1.5s/AoE2.05m/45daño |
| U3 Tempestad | MP40; CD52s; radio4.2m; 11 impactos/2s; total352; continuidad de combo aéreo y consumo de3 marcas |
| Pasiva | Super Magic hasta100; +34 por ataques/habilidades acertados y +10 por tick U3; ATK cargado96 de daño, aplica marca y cura; 25% extra daño a enemigo en aire |

### Adaptaciones propias y pendientes
- ATK evoluciona a combo 1-2-3-4 (+35% daño del cuarto); el controlador de
  animaciones actual dispone de índices 1-3, por lo que falta una variante
  animada específica del cuarto golpe.
- La curación plana original (80 sobre un personaje de 1000 HP) se escala
  a 8% de HP máximo actual, más 8% de HP faltante, para no romper el balance.
- El control aéreo afecta IA y VFX, pero todavía NO desplaza físicamente
  ni reproduce una animación dedicada de lanzamiento.
- Aún faltan HUD de cooldown, apuntado nearest/lowest-HP configurable,
  tercera/cuartas habilidades, locomoción multitouch final, el doble salto
  con mortal FURY 360 2.0 y la separación de animación torso/piernas.
- La nueva lógica se instala en el Combat Lab de Unity; NO se ha integrado
  al pueblo RG POLY City01 porque su Pages usa un artefacto WebGL antiguo
  fijado por el workflow de deploy, no compila automáticamente este código.

## Lista de verificación antes de fusionar
1. Abrir el proyecto en Unity 2021.3.45f1 y validar compilación.
2. Generar HIGHFLY > Build Combat Lab Scene sin errores.
3. Verificar control izquierdo, cámara derecha, HP/MP/energía y NPC.
4. Probar U1 tap, drag, cancel, daño, marcas, ralentización y refund.
5. Probar U2 salto, choque con paredes, daño, levantamiento, recast, eco.
6. Probar U3 solo y después de U2: 11 pulsos reales, combos y boss poise.
7. Probar carga Super Magic, curación escalada y cuarto ataque.
8. Probar varios monstruos, muerte, colisiones, Android horizontal y FPS.
9. Compilar APK + WebGL preliminar; SOLO después integrar a RG POLY
   y publicar una preview nueva sin cambiar el Pages público estable.

## Origen y licencias
Código nuevo HIGHFLY derivado del prototipo del usuario. No se importan
assets de ClaudeCraft ni sus iconos premium, modelos restringidos, sonidos NC
o material propio de Mobile Legends. Asset externo futuro: auditar el recurso
individual y documentar licencia antes de incorporarlo.

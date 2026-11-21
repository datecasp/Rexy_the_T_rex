# Rexy 2D

**Rexy 2D** es un juego de plataformas 2D desarrollado en Unity, protagonizado por Rexy, un T-Rex que debe recorrer niveles, esquivar enemigos, saltar obstáculos y recoger objetos para conseguir la máxima puntuación.

## Características

- Movimiento lateral, carrera y salto.
- Animaciones de caminar, correr, saltar y atacar.
- Enemigos con patrulla propia y persecución del jugador.
- Combate por pisotón: elimina dinosaurios al caer sobre ellos desde arriba.
- Coleccionables que otorgan puntos.
- Sistema de vidas y marcador de puntuación.
- Cámara que sigue al personaje horizontalmente y acompaña los saltos.
- Carga dinámica de escenas/chunks para construir niveles extensos.
- Controles de teclado y botones táctiles en pantalla.

## Controles

| Acción | Teclado |
|---|---|
| Moverse a la izquierda/derecha | `A` / `D` o flechas izquierda/derecha |
| Correr | `Shift izquierdo` |
| Saltar | `Espacio` |
| Atacar | `F` |

En dispositivos táctiles están disponibles botones para movimiento izquierdo y derecho, salto y ataque.

## Jugabilidad

- El juego comienza con 3 vidas.
- Los coleccionables añaden 50 puntos por defecto.
- Pisotear a un dinosaurio concede 250 puntos por defecto.
- Una colisión lateral o con partes peligrosas del enemigo hace perder una vida.
- Si el personaje cae por debajo de la zona de muerte, también pierde una vida.
- Cuando se agotan las vidas, el juego carga la escena `GameOver`.

## Requisitos

- Unity **6000.6.8f1**.
- Proyecto orientado a **.NET Standard 2.1**.
- Unity Universal Render Pipeline 2D.
- Microsoft Visual Studio 2022/2026 recomendado para trabajar con la solución C#.

## Abrir el proyecto

1. Instala Unity **6000.6.8f1** desde Unity Hub.
2. Clona el repositorio:

   ```bash
   git clone https://github.com/datecasp/Rexy_the_T_rex.git
   ```

3. Abre la carpeta del proyecto desde Unity Hub usando la versión indicada.
4. Espera a que Unity importe los paquetes y genere los archivos de solución.
5. Abre `Assets/Scenes/Level1_Bootstrap.unity` o `Assets/Scenes/Level1_Scene.unity` y pulsa **Play**.

## Escenas principales

- `Level1_Bootstrap`: escena base encargada de iniciar y coordinar la carga del nivel.
- `Level1_Scene`: escena principal del nivel.
- `Level1_Chunk_*`: fragmentos aditivos que pueden cargarse o descargarse según la posición del jugador.
- `GameOver`: escena mostrada al agotar todas las vidas.

## Organización del proyecto

```text
Assets/
├── Scenes/              Escenas principales y chunks aditivos del nivel
├── Scripts/            Control del jugador, HUD, cámara y coleccionables
├── TrexSprite/         Sprite, animaciones y controlador de animación de Rexy
├── TrexEnemies/        IA y herramientas de enemigos dinosaurio
├── Editor/             Utilidades personalizadas del editor de Unity
├── Terrain/            Elementos del terreno y plataformas
├── Sprites/            Recursos gráficos generales
├── UI/                 Interfaces y elementos de interfaz
├── DinoFood_Unity/      Recursos de coleccionables
└── DinoScoreDigits_Unity/ Dígitos visuales del marcador
```

## Arquitectura técnica

### Personaje

`TrexController` gestiona movimiento, salto, ataque, detección de suelo, recogida de objetos y reaparición. Usa `Rigidbody2D` y `TrexAnimationDriver` para sincronizar la física con las animaciones.

### Enemigos

`DinosaurEnemyAI` permite patrullar dentro de un área alrededor de la posición inicial y perseguir al jugador dentro de un rango configurable. También comprueba si el jugador elimina al enemigo con un pisotón o si recibe daño por contacto.

`EnemyPatrolBetweenPoints` ofrece una patrulla basada en límites relativos al punto de aparición y desactiva temporalmente el control externo del movimiento de la IA cuando está activo.

### HUD y puntuación

`HUDManager` administra vidas y puntuación. `CollectiblePickup` añade puntos al recoger objetos y `DinosaurEnemyAI` añade puntos al derrotar enemigos.

### Carga de nivel

`LevelChunkLoader` carga y descarga escenas aditivas según el avance horizontal del jugador. Puede trabajar con posiciones centrales o rangos de mundo y permite ajustar los radios de carga y descarga.

### Herramientas de editor

El proyecto incluye asistentes y utilidades personalizadas para dividir escenas en chunks, preparar sprites y configurar enemigos.

## Rama actual

El desarrollo activo se realiza en la rama `develop`.

## Contribuciones

Las contribuciones son bienvenidas. Antes de enviar cambios:

1. Crea una rama descriptiva desde `develop`.
2. Mantén el estilo y la organización existentes.
3. Prueba las escenas modificadas en el editor antes de hacer el commit.
4. Evita subir carpetas generadas como `Library`, `Temp`, `Logs` o archivos locales de Unity.

## Autor

Desarrollado por [datecasp](https://github.com/datecasp).

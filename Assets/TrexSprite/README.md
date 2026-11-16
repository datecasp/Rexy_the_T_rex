# TrexSprite — Unity asset

Asset 2D/2.5D del T-Rex con 5 animaciones y 50 frames PNG individuales.

## Contenido
- `Frames/Idle`, `Frames/Walk`, `Frames/Run`, `Frames/Jump`, `Frames/Attack`: 10 PNG por animación.
- `Editor/TrexSpriteInstaller.cs`: genera los clips, el Animator Controller y el prefab al importar.
- `Runtime/TrexAnimationDriver.cs`: API sencilla para controlar el Animator desde código.
- `Source/TrexSpriteSheet.png`: atlas original.

## Instalación
1. Descomprime la carpeta `Assets` de este ZIP en la raíz de tu proyecto Unity (o copia `Assets/TrexSprite` dentro de la carpeta `Assets`).
2. Espera a que Unity termine de importar y compilar.
3. El instalador crea automáticamente:
   - `Animations/Idle.anim`, `Walk.anim`, `Run.anim`, `Jump.anim`, `Attack.anim`
   - `Animator/Trex.controller`
   - `Prefabs/Trex.prefab`
4. Arrastra `Assets/TrexSprite/Prefabs/Trex.prefab` a la escena.

Si no se generan automáticamente, ejecuta `Tools > Trex Sprite > Create or Rebuild Unity Assets`.

## Control desde código
Añade `TrexAnimationDriver` al objeto (ya viene en el prefab):

```csharp
using UnityEngine;
using TrexSprite;

public class PlayerExample : MonoBehaviour
{
    [SerializeField] private TrexAnimationDriver trex;

    void Update()
    {
        float speed = Mathf.Abs(Input.GetAxisRaw("Horizontal"));
        trex.SetMoveSpeed(speed);
        trex.SetRunning(Input.GetKey(KeyCode.LeftShift));

        if (Input.GetKeyDown(KeyCode.Space)) trex.Jump();
        if (Input.GetKeyDown(KeyCode.F)) trex.Attack();
    }
}
```

Parámetros del Animator:
- `Speed` (float): 0 = Idle, > 0.1 = Walk.
- `Running` (bool): activa Run desde Walk.
- `Jump` (trigger): reproduce Jump y vuelve a Idle al terminar.
- `Attack` (trigger): reproduce Attack y vuelve a Idle al terminar.

## Ajustes de sprites
- Cada frame es un PNG RGBA de 224 × 192 px.
- Los 10 frames de cada ciclo están numerados `01` a `10`.
- Pivot centrado, 100 Pixels Per Unit, filtro Bilinear, sin mipmaps.
- Las celdas conservan el mismo lienzo para mantener un pivote estable.
- El personaje mira hacia la derecha.

## Notas
El prefab usa `SpriteRenderer` y `Animator`; no incluye física, collider, input de jugador ni lógica de movimiento. Añade `Rigidbody2D`/colliders y tu controlador de movimiento según el juego. Jump y Attack son animaciones visuales: la altura/desplazamiento real del personaje debe gestionarse con el controlador de juego si necesitas movimiento físico.

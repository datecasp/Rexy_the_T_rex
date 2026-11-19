# DinoFood - Unity-ready 2D sprites

## Contents
- Food/Meat: drumsticks and meat chunk
- Food/Bones: individual bones
- Food/Remains: skulls, skeletons and dinosaur remains
- Food/Eggs: whole and hatched eggs
- Food/Plants: apple, berries and leaves

Every PNG is a **512 x 512 RGBA** image with transparent background, centered horizontally and aligned to a common bottom baseline. Objects retain their original proportions; transparent padding is intentional for consistent pivots and sizing.

## Unity import settings (select all PNGs)
- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Pixels Per Unit: 100 (or match your project scale)
- Mesh Type: Tight
- Extrude Edges: 2
- Filter Mode: Bilinear (Point only if using pixel art)
- Compression: None or Low
- Generate Mip Maps: Off
- Pivot: Bottom Center (0.5, 0)

## Suggested use
Create a prefab per food object with:
- SpriteRenderer (sprite assigned)
- Collider2D (Circle/Polygon; trigger if collected by player)
- Optional Rigidbody2D (Kinematic if it should not fall)
- Your pickup/consumable script

The PNGs are individual static sprites, not animation frames. The original sheet did not contain animation sequences.

## Note on cutouts
The source sheet had a dark illustrated background. Background removal is automated, so inspect edge details in Unity; tiny dark details/shadows may need minor manual cleanup for production.

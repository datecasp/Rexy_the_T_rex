Dino Score Digits - Unity import pack

CONTENIDO
- Digit_0.png ... Digit_9.png
- 10 PNG independientes, uno por dígito.

ESPECIFICACIONES
- PNG RGBA con transparencia real.
- Lienzo normalizado de 256 x 256 px.
- Dígitos centrados horizontalmente y alineados sobre una línea base común.
- Estética de piedra/beige con musgo, coherente con el estilo de los dinosaurios y el escenario.

IMPORTACIÓN EN UNITY
Selecciona los 10 PNG y configura:
- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Pixels Per Unit: 100 (ajústalo si tu proyecto usa otra escala)
- Mesh Type: Tight
- Filter Mode: Bilinear
- Compression: None
- Generate Mip Maps: Off
- Pivot: Bottom Center (0.5, 0)

USO EN LA UI
Para mostrar 225 puntos, instancia/coloca tres Image (u objetos con SpriteRenderer) y asigna:
Digit_2, Digit_2, Digit_5.
Puedes reutilizar los mismos sprites tantas veces como necesites; no hace falta generar imágenes para cada puntuación.

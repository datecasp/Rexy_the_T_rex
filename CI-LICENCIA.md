# Activación de Unity para GitHub Actions

## Método recomendado: licencia Unity CI

1. En GitHub, abre `Settings > Secrets and variables > Actions`.
2. Crea el secreto `UNITY_LICENSE`.
3. Usa el contenido base64 de un archivo `.ulf` obtenido mediante una licencia válida para CI.

Ejemplo local para generar el valor:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\ruta\Unity.ulf"))
```

Copia toda la salida y pégala como valor de `UNITY_LICENSE`.

## Método alternativo: licencia Plus/Pro

Crea estos secretos:

- `UNITY_EMAIL`
- `UNITY_PASSWORD`
- `UNITY_SERIAL`

No uses `UNITY_LICENSE` al mismo tiempo que las credenciales y el serial.

## Unity Personal

Unity Personal no permite reutilizar directamente la licencia vinculada a un ordenador local en otro equipo. Para automatizar las compilaciones necesitas:

- una licencia válida compatible con CI; o
- Unity Plus/Pro con serial; o
- activar Unity Cloud Gaming/License Server si tu cuenta dispone de él.

Si ninguna de estas opciones está disponible, el workflow debe ejecutarse en una máquina Windows con Unity y generar los archivos manualmente.

Los archivos `.ulf`, `.alf` y `.keystore` no deben subirse al repositorio.

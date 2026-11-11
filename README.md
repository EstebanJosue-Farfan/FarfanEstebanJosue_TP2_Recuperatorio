# TP1_Prg1 - Supervivencia sobre plataformas

Trabajo Práctico N°1 de **Programación de Videojuegos 1** (TUDIVJ, Facultad de Ingeniería, UNJu).

## Descripción del juego

Prototipo 3D de supervivencia. El jugador empieza sobre un grupo de plataformas que desaparecen y reaparecen cada pocos segundos, mientras caen esferas desde el cielo. Hay que sobrevivir **2:00** saltando de una plataforma a otra. Al terminar el tiempo aparece una plataforma meta, y para ganar hay que **llevar la caja hasta la zona de entrega**. Si el jugador se cae al vacío o lo golpea una esfera, pierde.

## Versión de Unity

**Unity 6.5 (6000.5.8f1)**, render pipeline URP.

## Controles

| Tecla | Acción |
|---|---|
| **W A S D** | Mover al personaje |
| **Espacio** | Saltar |
| **E** | Recoger la caja (estando cerca) |
| **G** | Soltar la caja |
| **R** | Reiniciar tras ganar o perder |

## Mecánicas implementadas

- **Plataformas intermitentes (`Invoke`):** tres plataformas se ponen rojas como aviso, desaparecen y reaparecen con `Invoke()`.
- **Plataforma móvil (`Invoke`):** una de ellas se desplaza entre dos posiciones y usa `Invoke()` para pausar y cambiar de dirección en cada extremo.
- **Spawner de obstáculos (`InvokeRepeating`):** un generador instancia esferas periódicamente con `InvokeRepeating()`. Cada una se destruye a los 6 segundos para evitar acumulación, y si toca al jugador este pierde.
- **Recolección y transporte (`SetParent`):** la caja se recoge con **E** y se asocia al punto de transporte del jugador con `SetParent()`. Se suelta con **G** restaurando su independencia y su física.
- **Power-Up con corrutina:** una esfera amarilla otorga **aumento de velocidad** (x1.8) durante 4 segundos y el jugador se pone amarillo. La esfera tarda 10 segundos en reaparecer (cooldown), ambos controlados con corrutinas.
- **Entrega y victoria (`Trigger`):** la zona de entrega es un Collider con Trigger. Solo se gana si **la caja** es depositada en ella; que entre solo el jugador no activa la victoria. Se muestra el mensaje "¡VICTORIA!" y la zona cambia de color.
- **Derrota:** caer al vacío o ser golpeado por una esfera muestra "¡PERDISTE!".

**Capacidad del Power-Up elegida:** velocidad de movimiento.

## Captura del escenario

Se encuntra (Docs/captura.png)

## Cómo abrir y ejecutar el proyecto

1. Clonar el repositorio:
```bash
   git clone https://github.com/EstebanJosue-Farfan/TP1_Prg1
```
2. Abrir **Unity Hub** > **Add** > **Add project from disk** y elegir la carpeta clonada.
3. Abrir con **Unity 6.5 (6000.5.8f1)**.
4. Abrir la escena `Assets/Scenes/SampleScene`.
5. Apretar **Play**.

## Estructura del proyecto

```
Assets/
  Scenes/     Escena principal (SampleScene)
  Scripts/    Scripts de las mecánicas
  Prefabs/    Prefab del obstáculo
  Materials/  Materiales
```

## Autor

Esteban Josue Farfan - TP1, 2026

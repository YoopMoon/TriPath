# TriPath — Juego de plataformas 2D con dificultad adaptativa

**Autor:** Javier Mañas García-Valcárcel

## Descripción

TriPath es un videojuego de plataformas 2D desarrollado en Unity 6 como Trabajo Fin de Grado. El proyecto combina una base jugable clásica del género —desplazamiento lateral, salto, enemigos y coleccionables— con un sistema de dificultad adaptativa que observa el rendimiento del jugador al completar cada nivel y ajusta automáticamente ciertos parámetros del juego en los niveles siguientes.

El prototipo incluye:
- 2 mundos con 10 niveles jugables
- 3 personajes seleccionables con atributos diferenciados (Virtual Guy, Ninja Frog, Mask Dude)
- Sistema de vida progresivo con daño parcial, curación e invulnerabilidad temporal
- Enemigos con comportamientos distintos: terrestres, aéreos y con proyectiles
- Sistema de dificultad adaptativa basado en tiempo, daño recibido y porcentaje de monedas recogidas
- HUD, menú de pausa, pantalla de Game Over y ranking final de jugadores
- Capa de audio con música por mundos y efectos de sonido

---

## Requisitos

- **Unity 6** (versión 6000.0 o superior)
- Sistema operativo: Windows 10/11 (recomendado), macOS o Linux
- **Visual Studio 2022** o Visual Studio Code con extensión de C#
- Git

---

## Cómo abrir el proyecto

1. Clona el repositorio:

   git clone https://github.com/YoopMoon/TriPath.git

2. Abre **Unity Hub** y haz clic en **Add project from disk**.

3. Selecciona la carpeta raíz del repositorio clonado.

4. Asegúrate de tener instalada la versión **Unity 6**. Unity Hub te avisará si no la tienes y te ofrecerá instalarla.

5. Una vez abierto el proyecto, ve a `File > Build Settings` y comprueba que la plataforma seleccionada es **PC, Mac & Linux Standalone**.

6. Para ejecutar el juego desde el editor, abre la escena `MainMenu` desde `Assets/Scenes` y pulsa **Play**.

---

## Nota sobre los assets

Los recursos gráficos y de audio provienen de paquetes gratuitos de la Unity Asset Store y no están incluidos en este repositorio por restricciones de licencia. Es necesario importarlos manualmente antes de ejecutar el proyecto:

- [Pixel Adventure 1](https://assetstore.unity.com/packages/2d/characters/pixel-adventure-1-155360)
- [Pixel Adventure 2](https://assetstore.unity.com/packages/2d/characters/pixel-adventure-2-155418)
- [Free Casual Platformer & Puzzle Music Pack](https://assetstore.unity.com/packages/audio/music/electronic/free-casual-platformer-puzzle-music-pack-356114)
- [Free Sci-Fi and Cyberpunk Music Pack](https://assetstore.unity.com/packages/audio/ambient/sci-fi/free-sci-fi-and-cyberpunk-music-pack-264590)
- [Halftone Sound Effects Pack Lite](https://assetstore.unity.com/packages/audio/sound-fx/halftone-sound-effects-pack-lite-178439)
- [Keyboard Keys for UI](https://dreammixgames.itch.io/keyboard-keys-for-ui)

---

## Estructura del proyecto

## Estructura del proyecto

```text
Assets/
  Scenes/           - Escenas del juego (menu, niveles, ranking)
  Scripts/          - Toda la logica en C#
    Player/         - Control del personaje, salud, animaciones
    Enemies/        - Comportamiento de enemigos
    Collectibles/   - Monedas y frutas
    Adaptive/       - Sistema de dificultad adaptativa
    Audio/          - Gestores de musica y efectos
    UI/             - HUD, menus, ranking
    Persistence/    - Persistencia entre escenas
  Prefabs/          - Objetos reutilizables
  Tilemaps/         - Paletas y tiles del escenario
  Settings/         - Input Actions y configuracion del proyecto
```
---

## Licencia

Este proyecto se desarrolla con fines académicos. El código fuente es de libre consulta. Los assets de terceros están sujetos a sus respectivas licencias.

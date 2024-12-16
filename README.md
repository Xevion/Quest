# Rebirth

A simple puzzle game about growth.

## Control Design

- Camera
  - WASD, Arrow keys to move.
  - Right click, drag to move.
  - Scroll wheel to zoom. Mouse position dictates zoom center.
- Units
  - Left click on/nearby a planet.
  - Scroll to lower/raise the total percentage of units.
  - Drag to another planet, release to send units.
  - Until units arrive, they won't be selectable or part of the usable population on a planet.

## Planet Selection

1. Click on a Planet
2. Planet emits signal to Network
3. Network pulls Selection GameObject into view, overhead planet
4. Network takes all scroll key inputs, re-renders Selection gameobject to fit percentage
5. Click on another planet (to send units), or same one (to cancel)
6. Network sends signal to Planet to send units
7. Or if cancelled, Selection gameObject is hidden
8. If sent, Planet tells units to start moving to another planet

## Unit Combat

All units are placed inside datastructure

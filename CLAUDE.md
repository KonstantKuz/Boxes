# Boxes - Unity Game Project

## Project Overview
Unity игра с квестовой системой, взаимодействием с ящиками и физикой.

## Key Scripts
- `Assets/Scripts/Gameplay/Interactable/BoxesInteraction/` - система взаимодействия с ящиками
- `Assets/Scripts/Infrastructure/QuestService/` - квестовая система
- `Assets/Scripts/Gameplay/RoadSystem/CarAgent.cs` - AI для машин

## Coding Standards
- БЕЗ комментариев в коде (используй память проекта вместо этого)
- НЕ ИСПОЛЬЗОВАТЬ тупые короткие сокращения: rb, vel, x.value, pos, rot и т.д. - использовать полные читаемые имена: rigidbody, velocity, position, rotation

## Architecture
- Mirror networking для мультиплеера
- Reflex для dependency injection
- Addressables для загрузки ассетов

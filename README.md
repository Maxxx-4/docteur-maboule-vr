# Docteur Maboule VR

Adaptation en réalité virtuelle du jeu d'adresse *Docteur Maboule* (Operation), développée de zéro sous **Unity 6000.3.24** pour le **Meta Quest 3**. Projet de groupe du cours de VR (équipe de 3 : Maxence, Jeanne et Octave).

## Principe du jeu

Le joueur est un chirurgien. Avec une pince tenue à la main, il retire de petites pièces (les organes) du patient. Si la pince touche le bord d'une cavité : buzzer, nez rouge, vibration de la manette et une erreur de plus. Trop d'erreurs ou temps écoulé = défaite ; toutes les pièces retirées = victoire.

## Fonctionnalités

Périmètre de base :

- [ ] Pince attrapée avec la manette
- [ ] Patient avec au moins 5 organes à retirer
- [ ] Buzzer, nez rouge et vibration au contact du bord
- [ ] Score et chrono
- [ ] Écran de fin avec bouton pour recommencer
- [ ] Jeu fluide sur le Quest 3

Bonus :

- [ ] Réalité mixte (patient posé sur une vraie table)
- [ ] Plusieurs niveaux de difficulté

## Concepts de VR utilisés

Saisie d'objets et interactions avec les manettes, retour haptique, suivi de la tête et de la caméra, interactions par collisions, interface en espace 3D. Le projet s'appuie sur le template VR d'Unity et le parcours Unity Learn *VR Development*.

## Lancer le projet

1. Installer **Unity 6000.3.24** (Unity Hub) avec le module **Android Build Support** pour le Quest 3.
2. Cloner le dépôt : `git clone https://github.com/Maxxx-4/docteur-maboule-vr.git`
3. Ouvrir le dossier dans Unity Hub (*Add project from disk*), puis ouvrir la scène principale.
4. Sans casque : utiliser le **XR Device Simulator** (Window > Package Manager > XR Interaction Toolkit > Samples).
5. Sur casque : *File > Build Profiles > Android*, brancher le Quest 3 en mode développeur, puis *Build and Run*.

## Organisation du projet

| Dossier | Contenu |
|---|---|
| `Assets/` | Scènes, scripts, prefabs et assets du jeu |
| `Packages/` et `ProjectSettings/` | Configuration Unity |
| `docs/` | Documentation : assets utilisés, configuration Unity, plan d'apprentissage |

Documentation détaillée :

- [Assets 3D utilisés (source et licence)](docs/ASSETS.md)
- [Configuration Unity](docs/CONFIGURATION_UNITY.md)
- [Plan du parcours Unity Learn](docs/PLAN_PARCOURS_UNITY_LEARN.md)

## Gestion de projet

- Planning Notion : https://app.notion.com/p/Docteur-Maboule-en-VR-40d15aa0a059806e8acbf9464e5dc1ce
- Rendu final : **1er décembre 2026** (démo devant la classe)
- Assets 3D : uniquement gratuits, avec source et licence indiquées dans `docs/ASSETS.md`

## Équipe

Maxence Périnel, Jeanne, Octave

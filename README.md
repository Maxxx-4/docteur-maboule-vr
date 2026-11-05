# Docteur Maboule VR

Adaptation en réalité virtuelle du jeu *Docteur Maboule* (Operation), développée sous **Unity** pour le **Meta Quest 3**. Projet du cours de VR, équipe de 3.

- **Planning / gestion de projet :** Notion → `[COLLER LE LIEN NOTION ICI]`
- **Rendu final :** 1er décembre 2026 (démo devant la classe en décembre)
- **Assets 3D :** uniquement gratuits (indiquer la source et la licence dans `docs/ASSETS.md`)

## Principe du jeu

Le joueur est un chirurgien. Avec une pince tenue à la main, il retire de petites pièces du patient. Si la pince touche le bord d'une cavité : buzzer, nez rouge, vibration de la manette, une erreur de plus. Trop d'erreurs ou temps écoulé = défaite ; toutes les pièces retirées = victoire.

## Démarrage (chaque membre, une seule fois)

1. Installer **Unity Hub** et l'éditeur **Unity 6.2** (même version pour les 3 !), avec le module **Android Build Support** (SDK, NDK, OpenJDK) pour le Quest 3.
2. Cloner le dépôt (avec Git LFS installé : `git lfs install`).
3. Unity Hub → **Add → Add project from disk** → choisir le dossier cloné → sélectionner Unity 6.2.
4. Au premier lancement Unity télécharge les packages du `Packages/manifest.json` (XR Interaction Toolkit, OpenXR, XR Hands, XR Management, Input System, URP). Si un numéro de version est refusé, ouvrir *Window → Package Manager* et prendre la version proposée.
5. Suivre `docs/CONFIGURATION_UNITY.md` (réglages XR, Android, simulateur). La première personne qui le fait **commit les fichiers générés** (`ProjectSettings/`, les `.meta`).

## Travailler sans casque : le simulateur XR

On travaillera surtout avec le **XR Device Simulator** (fourni avec XR Interaction Toolkit). Voir `docs/CONFIGURATION_UNITY.md` pour l'activer et pour les touches (clavier + souris qui simulent casque et manettes). Le casque réel sert aux tests de validation (build Android sur Quest 3, voir le même document).

Limites du simulateur : pas de vraies vibrations ni de vrai ressenti de profondeur. Tout ce qui touche au haptique et à la précision de la pince doit être validé sur le Quest 3.

## Structure

```
Assets/
  Scripts/    GameManager, CavityEdge, BodyPart, OperationTool, HapticsHelper
  Scenes/     une scène par fonctionnalité de test + la scène finale
  Prefabs/  Materials/  Models/  Audio/  Settings/
docs/         configuration, plan de travail (parcours Unity Learn), assets
```

## Règles d'équipe Git

- Branche `main` = toujours jouable. Travail dans `feature/<nom>-<sujet>`, puis Pull Request relue par un autre membre.
- **Une seule personne modifie une même scène à la fois** (les scènes se fusionnent mal) : on découpe en prefabs et en petites scènes de test, et on se prévient sur Notion/le groupe.
- Toujours commiter les `.meta` avec leurs fichiers. Ne jamais commiter `Library/`, `Temp/`, `Builds/` (déjà dans `.gitignore`).
- Fichiers lourds (modèles, textures, sons) : gérés par Git LFS (`.gitattributes`).
- Réglages Unity à garder : *Project Settings → Editor → Asset Serialization = Force Text* et *Version Control → Visible Meta Files*.

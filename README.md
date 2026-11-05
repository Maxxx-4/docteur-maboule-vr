# Docteur Maboule VR

Adaptation en réalité virtuelle du jeu *Docteur Maboule* (Operation), développée sous **Unity** pour le **Meta Quest 3**. Projet du cours de VR, équipe de 3.

- **Planning / gestion de projet :** Notion → `[COLLER LE LIEN NOTION ICI]`
- **Rendu final :** 1er décembre 2026 (démo devant la classe en décembre)
- **Assets 3D :** uniquement gratuits (indiquer la source et la licence dans `docs/ASSETS.md`)

## Principe du jeu

Le joueur est un chirurgien. Avec une pince tenue à la main, il retire de petites pièces du patient. Si la pince touche le bord d'une cavité : buzzer, nez rouge, vibration de la manette, une erreur de plus. Trop d'erreurs ou temps écoulé = défaite ; toutes les pièces retirées = victoire.

## Démarrage (chaque membre, une seule fois)

1. Installer **Unity Hub**, **GitHub Desktop** et l'éditeur **Unity 6.3 LTS (6000.3.24f1)**. Tout le monde doit avoir **exactement la même version**. Ajouter le module **Android Build Support** (SDK, NDK, OpenJDK) pour le Quest 3.
2. Dans GitHub Desktop : *File → Clone Repository*, choisir `docteur-maboule-vr`, puis un dossier **hors OneDrive**.
3. Dans Unity Hub : *Add → Add project from disk*, choisir le dossier cloné, puis l'éditeur 6000.3.24f1.
4. Au premier lancement, Unity télécharge les packages de `Packages/manifest.json` (XR Interaction Toolkit, OpenXR, XR Hands, XR Management, Input System, URP). Si une version est refusée, ouvrir *Window → Package Manager* et prendre la version « Verified » proposée.
5. Suivre `docs/CONFIGURATION_UNITY.md` (réglages XR, Android, simulateur).

## Travailler sans casque : le simulateur XR

On travaillera surtout avec le **XR Device Simulator** (fourni avec XR Interaction Toolkit). Voir `docs/CONFIGURATION_UNITY.md` pour l'activer et pour les touches (clavier + souris qui simulent casque et manettes). Le casque réel sert aux tests de validation (build Android sur Quest 3).

Limites du simulateur : pas de vraies vibrations ni de vrai ressenti de profondeur. Tout ce qui touche au haptique et à la précision de la pince doit être validé sur le Quest 3.

## Structure

```
Assets/
  Scripts/    GameManager, CavityEdge, BodyPart, OperationTool, HapticsHelper
  Scenes/     une scène par fonctionnalité de test + la scène finale
  Prefabs/  Materials/  Models/  Audio/  Settings/
docs/         configuration, plan de travail (parcours Unity Learn), assets
```

## Travail en équipe avec Git (GitHub Desktop)

Le `.gitignore` est le modèle officiel Unity : on envoie `Assets`, `Packages`, `ProjectSettings` et les fichiers `.meta` ; on n'envoie jamais `Library`, `Temp`, `Logs` ni les builds (Unity les régénère).

**Avant de travailler**
1. **Fetch** pour voir si quelqu'un a poussé des changements.
2. **Pull** pour récupérer la dernière version.
3. Travailler sur sa propre branche : *Branch → New branch*, nommée `feature/<sujet>` (ex. `feature/pince`, `feature/score`, `feature/ui`).

**Quand une étape est terminée**
1. **Commit** avec un message clair.
2. **Push**.
3. **Merge** de la branche vers `main` quand la fonctionnalité est terminée et fonctionne.

**Règles**
- `main` doit toujours rester jouable : on ne travaille pas directement dessus.
- **Une seule personne modifie une même scène ou un même prefab à la fois** : on se répartit les fonctionnalités et on découpe en prefabs et en petites scènes de test.
- Toujours committer les `.meta` avec leurs fichiers.
- En cas de conflit de fusion, on ne force rien : on se prévient et on le résout ensemble.
- Si GitHub refuse un fichier trop gros, on revoit le `.gitignore` avant toute autre solution.

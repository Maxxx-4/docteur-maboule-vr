# Configuration Unity (XR + simulateur + Quest 3)

À faire une fois, par une seule personne, puis à commiter. Les noms de menus peuvent légèrement varier selon la version exacte de Unity 6.2 et des packages : en cas de doute, la documentation du package fait foi.

## 1. Rendu et plateforme

1. *File → Build Profiles* : ajouter la plateforme **Android** et la définir comme active (Quest 3 = Android).
2. *Project Settings → Player → Android* : Minimum API Level **Android 10 (API 29)** ou plus, Scripting Backend **IL2CPP**, Target Architecture **ARM64**.
3. Rendu : URP. Pour le Quest, éviter les effets lourds (post-traitement, ombres temps réel multiples).

## 2. XR Plug-in Management

1. *Project Settings → XR Plug-in Management* : onglet **PC** → cocher **OpenXR** (nécessaire pour tester dans l'éditeur) ; onglet **Android** → cocher **OpenXR**.
2. *Project Settings → XR Plug-in Management → OpenXR* (onglet Android) : dans *Interaction Profiles*, ajouter **Oculus Touch Controller Profile** ; dans *OpenXR Feature Groups*, activer **Meta Quest Support**. Si *Project Validation* signale des problèmes, cliquer sur **Fix All**.
3. Pour le suivi des mains : activer **Hand Tracking Subsystem** (package XR Hands) dans les features OpenXR.

## 3. XR Interaction Toolkit : exemples de départ

*Window → Package Manager → XR Interaction Toolkit → Samples* : importer **Starter Assets** (préréglages d'actions, prefabs) et **XR Device Simulator**.

## 4. Le simulateur XR (travail sans casque)

1. Dans les Samples importés, repérer le prefab **XR Device Simulator** et le glisser dans la scène de test. Selon la version du toolkit, il existe aussi une option dans *Project Settings → XR Plug-in Management → XR Interaction Toolkit* pour ajouter automatiquement le simulateur aux scènes en éditeur.
2. Le simulateur contrôle le casque et les deux manettes avec le clavier et la souris. Les raccourcis exacts s'affichent dans l'interface du simulateur ou dans la documentation du package (menu *Samples → XR Device Simulator*), notamment pour : passer d'une manette à l'autre, bouger la tête, appuyer sur grip et gâchette.
3. Le prefab **XR Origin** de votre scène doit porter les bons *Input Action Assets* (ceux des Starter Assets) pour que le simulateur et les vrais contrôleurs partagent les mêmes actions.
4. Désactiver ou retirer le simulateur dans la scène finale (ou le garder uniquement dans les scènes de test).

## 5. Test sur Quest 3 (validation régulière)

1. Quest 3 : activer le **mode développeur** (application Meta Horizon sur téléphone).
2. Brancher en USB-C, accepter le débogage USB dans le casque.
3. *Build Profiles → Android → Run Device* : choisir le Quest, puis **Build And Run**.
4. Le jeu final doit être validé sur casque avant chaque jalon (haptique, taille de la pince, confort).

## 6. Premier montage de la scène (ordre conseillé)

1. Scène `01_SimulateurTest` : XR Origin + XR Device Simulator + sol + cube saisissable (vérifier qu'on peut le prendre avec le simulateur).
2. Scène `02_Table` : table d'opération, patient (modèle gratuit), éclairage.
3. Pince : modèle simple + Rigidbody + Collider + **XR Grab Interactable** + script `OperationTool`.
4. Cavités : Collider **Is Trigger** sur les bords + script `CavityEdge` (buzzer = AudioSource, nez = Renderer).
5. Pièces : Rigidbody + XR Grab Interactable + `BodyPart`.
6. Un objet vide `GameManager` avec le script du même nom ; brancher les événements sur un Canvas **World Space** (score, erreurs, temps).

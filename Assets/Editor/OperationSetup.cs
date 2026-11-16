#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Outils d'éditeur pour Docteur Maboule : crée les prefabs des 8 pièces et de la pince
/// à partir des FBX de Assets/Models (colliders natifs, Rigidbody, XR Grab Interactable).
/// Menu : Docteur Maboule > ...
/// </summary>
public static class OperationSetup
{
    const string ModelsDir = "Assets/Models/";
    const string PrefabDir = "Assets/Prefabs/";

    struct PieceDef
    {
        public string file, handle, shape;
        public Vector3 size, center;
        public float radius, height;
    }

    // Données issues de Operation_Objects.json (tailles en mètres, repère Unity).
    static readonly PieceDef[] Pieces =
    {
        new PieceDef { file="Piece_01_BrokenHeart", handle="Handle_Piece_01", shape="box",    size=new Vector3(0.0784f,0.0244f,0.0721f) },
        new PieceDef { file="Piece_02_Apple",       handle="Handle_Piece_02", shape="sphere", radius=0.0268f, center=new Vector3(0f,-0.00298f,0f) },
        new PieceDef { file="Piece_03_Fish",        handle="Handle_Piece_03", shape="box",    size=new Vector3(0.0774f,0.0219f,0.0507f) },
        new PieceDef { file="Piece_04_Croissant",   handle="Handle_Piece_04", shape="box",    size=new Vector3(0.0797f,0.0367f,0.0408f) },
        new PieceDef { file="Piece_05_FunnyBone",   handle="Handle_Piece_05", shape="capsule",radius=0.0114f, height=0.0689f },
        new PieceDef { file="Piece_06_Donut",       handle="Handle_Piece_06", shape="box",    size=new Vector3(0.0427f,0.0176f,0.0425f) },
        new PieceDef { file="Piece_07_Wrench",      handle="Handle_Piece_07", shape="box",    size=new Vector3(0.0714f,0.0087f,0.0288f) },
        new PieceDef { file="Piece_08_Starfish",    handle="Handle_Piece_08", shape="box",    size=new Vector3(0.0671f,0.0253f,0.0636f) },
    };

    [MenuItem("Docteur Maboule/1 - Créer prefabs pièces et pince")]
    public static void CreatePrefabs()
    {
        Directory.CreateDirectory(PrefabDir.TrimEnd('/'));
        int ok = 0;

        foreach (var def in Pieces)
        {
            var go = Instantiate(def.file);
            if (go == null) continue;
            RemoveProxies(go);

            switch (def.shape)
            {
                case "sphere":
                    var sc = go.AddComponent<SphereCollider>(); sc.radius = def.radius; sc.center = def.center; break;
                case "capsule":
                    var cc = go.AddComponent<CapsuleCollider>(); cc.radius = def.radius; cc.height = def.height; cc.direction = 0; break;
                default:
                    var bc = go.AddComponent<BoxCollider>(); bc.size = def.size; bc.center = def.center; break;
            }

            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;       // reste dans la cavité tant qu'on ne la saisit pas
            rb.useGravity = false;

            var grab = go.AddComponent<XRGrabInteractable>();
            grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
            grab.throwOnDetach = false;
            var h = FindDeep(go.transform, def.handle);
            if (h != null) grab.attachTransform = h;

            go.AddComponent<BodyPart>();

            Save(go, def.file);
            ok++;
        }

        var pince = Instantiate("Pince");
        if (pince != null)
        {
            RemoveProxies(pince);

            var tip = pince.AddComponent<BoxCollider>();                  // pointes
            tip.center = new Vector3(0f, 0f, 0.1315f);
            tip.size = new Vector3(0.02f, 0.0052f, 0.03f);
            var handle = pince.AddComponent<BoxCollider>();               // manche
            handle.center = new Vector3(0f, 0f, -0.0025f);
            handle.size = new Vector3(0.023f, 0.015f, 0.085f);

            var rb = pince.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var grab = pince.AddComponent<XRGrabInteractable>();
            grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
            grab.throwOnDetach = false;
            var attach = FindDeep(pince.transform, "Attach");
            if (attach != null) grab.attachTransform = attach;

            pince.AddComponent<OperationTool>();
            Save(pince, "Pince");
            ok++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Docteur Maboule] Prefabs créés : " + ok + " / 9 dans " + PrefabDir);
    }

    [MenuItem("Docteur Maboule/Dump hiérarchie de Gérard (docs/gerard_hierarchy.txt)")]
    public static void DumpGerard()
    {
        var root = GameObject.Find("Patient_VR_Final");
        if (root == null) { Debug.LogError("Patient_VR_Final introuvable dans la scène."); return; }
        var sb = new StringBuilder();
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            string path = t.name;
            for (var p = t.parent; p != null && p != root.transform.parent; p = p.parent) path = p.name + "/" + path;
            var comps = new StringBuilder();
            foreach (var c in t.GetComponents<Component>())
                if (c != null && !(c is Transform)) comps.Append(c.GetType().Name).Append(' ');
            sb.AppendLine(path + " | pos " + t.position.ToString("F4") + " | rot " + t.rotation.eulerAngles.ToString("F1") + " | " + comps);
        }
        Directory.CreateDirectory("docs");
        File.WriteAllText("docs/gerard_hierarchy.txt", sb.ToString());
        Debug.Log("[Docteur Maboule] Dump écrit dans docs/gerard_hierarchy.txt");
    }

    // Positions de départ (repère Unity, relatives à Patient_VR_Final) et rotation autour de Y.
    static readonly Vector3[] HomePos =
    {
        new Vector3(-0.10601f, 0.21808f, -0.27141f),
        new Vector3( 0.10601f, 0.23639f, -0.26998f),
        new Vector3(-0.00272f, 0.23597f, -0.11935f),
        new Vector3( 0.00003f, 0.25189f,  0.05373f),
        new Vector3(-0.43904f, 0.07896f,  0.00810f),
        new Vector3( 0.30996f, 0.11862f, -0.21620f),
        new Vector3(-0.16162f, 0.13439f,  0.43406f),
        new Vector3( 0.16172f, 0.14434f,  0.43170f),
    };
    static readonly float[] HomeYaw = { 0, 0, 0, 0, 63.43f, 0, 82.62f, 0 };

    [MenuItem("Docteur Maboule/2 - Placer pièces, pince et bords dans la scène")]
    public static void SetupScene()
    {
        var patient = GameObject.Find("Patient_VR_Final");
        if (patient == null) { Debug.LogError("Patient_VR_Final introuvable dans la scène."); return; }
        Vector3 origin = patient.transform.position;

        // 1) Pièces dans leurs cavités
        for (int i = 0; i < Pieces.Length; i++)
        {
            string n = Pieces[i].file;
            var old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + n + ".prefab");
            if (prefab == null) { Debug.LogError("Prefab manquant : " + n + " (lance d'abord le menu 1)"); continue; }
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.name = n;
            inst.transform.position = origin + HomePos[i];
            inst.transform.rotation = Quaternion.Euler(0f, HomeYaw[i], 0f);
        }

        // 2) Pince posée sur la table, côté joueur, pointe vers Gérard
        var oldPince = GameObject.Find("Pince");
        if (oldPince != null) Object.DestroyImmediate(oldPince);
        var pincePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "Pince.prefab");
        if (pincePrefab != null)
        {
            var p = (GameObject)PrefabUtility.InstantiatePrefab(pincePrefab);
            p.name = "Pince";
            p.transform.position = new Vector3(0.62f, 0.9f + 0.0085f, 0.15f);
            p.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }

        // 3) Bords des cavités : triggers + CavityEdge, proxys invisibles
        int edges = 0;
        foreach (var t in patient.GetComponentsInChildren<Transform>(true))
        {
            if (!t.name.StartsWith("COL_")) continue;
            var mr = t.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;                       // plus de vert de debug

            if (t.name.Contains("_RimSeg_"))
            {
                var mf = t.GetComponent<MeshFilter>();
                var mc = t.GetComponent<MeshCollider>();
                if (mc == null) mc = t.gameObject.AddComponent<MeshCollider>();
                if (mf != null) mc.sharedMesh = mf.sharedMesh;
                mc.convex = true;
                mc.isTrigger = true;
                if (t.GetComponent<CavityEdge>() == null) t.gameObject.AddComponent<CavityEdge>();
                edges++;
            }
        }

        // 4) GameManager
        if (Object.FindFirstObjectByType<GameManager>() == null)
        {
            var gm = new GameObject("GameManager").AddComponent<GameManager>();
            var so = new SerializedObject(gm);
            so.FindProperty("totalPieces").intValue = Pieces.Length;
            so.FindProperty("timeLimitSeconds").floatValue = 240f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Docteur Maboule] Scène prête : 8 pièces, pince, " + edges + " bords de cavité, GameManager.");
    }

    [MenuItem("Docteur Maboule/3 - Remplacer XR Origin par le rig avec manettes")]
    public static void ReplaceXROrigin()
    {
        const string rigPath = "Assets/Samples/XR Interaction Toolkit/3.3.2/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(rigPath);
        if (prefab == null) { Debug.LogError("Prefab introuvable : " + rigPath); return; }

        var old = GameObject.Find("XR Origin (VR)");
        Vector3 pos = new Vector3(0.9f, 0f, 0f);
        Quaternion rot = Quaternion.Euler(0f, -90f, 0f);
        if (old != null)
        {
            pos = old.transform.position;
            rot = old.transform.rotation;
            Object.DestroyImmediate(old);
        }
        var oldRig = GameObject.Find("XR Origin (XR Rig)");
        if (oldRig != null) Object.DestroyImmediate(oldRig);

        var rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        rig.transform.SetPositionAndRotation(pos, rot);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Docteur Maboule] XR Origin (XR Rig) avec manettes en place à " + pos);
    }

    [MenuItem("Docteur Maboule/4 - Ajouter un sol")]
    public static void AddFloor()
    {
        var old = GameObject.Find("Sol");
        if (old != null) Object.DestroyImmediate(old);

        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);   // avec BoxCollider
        floor.name = "Sol";
        floor.transform.position = new Vector3(0f, -0.05f, 0f);       // le dessus est à y = 0
        floor.transform.localScale = new Vector3(12f, 0.1f, 12f);
        floor.isStatic = true;

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Docteur Maboule] Sol ajouté (12 m x 12 m).");
    }

    // ------------------------------------------------------------------

    static GameObject Instantiate(string file)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelsDir + file + ".fbx");
        if (asset == null) { Debug.LogError("FBX introuvable : " + ModelsDir + file + ".fbx"); return null; }
        var go = Object.Instantiate(asset);
        go.name = file;
        return go;
    }

    static void RemoveProxies(GameObject go)
    {
        var toKill = new System.Collections.Generic.List<GameObject>();
        foreach (var t in go.GetComponentsInChildren<Transform>(true))
            if (t.name.StartsWith("COL_")) toKill.Add(t.gameObject);
        foreach (var o in toKill) Object.DestroyImmediate(o);
    }

    static Transform FindDeep(Transform parent, string name)
    {
        foreach (var t in parent.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    static void Save(GameObject go, string name)
    {
        PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + name + ".prefab");
        Object.DestroyImmediate(go);
    }
}
#endif

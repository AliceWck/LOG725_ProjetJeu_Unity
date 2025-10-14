using UnityEngine;
using UnityEditor;

public class WallWithDoorMaker : MonoBehaviour
{
    [Header("Dimensions du Mur")]
    public float murLargeur = 5f;
    public float murHauteur = 3f;
    public float murEpaisseur = 0.2f;

    [Header("Porte")]
    public GameObject doorPrefab;          // Glisse ton prefab de porte ici
    public float porteX = 2f;              // Position horizontale (depuis la gauche)
    public float porteLargeur = 1f;        // Largeur de l'ouverture
    public float porteHauteur = 2.2f;      // Hauteur de l'ouverture
    public bool inverserPorte = false;     // Inverser l'orientation de la porte

    [Header("Matériau")]
    public Material murMaterial;

    [ContextMenu("Créer Mur avec Porte")]
    public void CreerMurAvecPorte()
    {
        // Nettoyer les enfants existants (sauf la porte si elle existe déjà)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (!child.name.Contains("door") && !child.name.Contains("Door"))
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // Calculer les dimensions des parties du mur
        float gaucheWidth = porteX;
        float droiteWidth = murLargeur - porteX - porteLargeur;
        float hautHeight = murHauteur - porteHauteur;

        // 1. Partie GAUCHE (si elle existe)
        if (gaucheWidth > 0.01f)
        {
            CreerPartie("MurGauche",
                new Vector3(gaucheWidth / 2, murHauteur / 2, 0),
                new Vector3(gaucheWidth, murHauteur, murEpaisseur));
        }

        // 2. Partie DROITE (si elle existe)
        if (droiteWidth > 0.01f)
        {
            CreerPartie("MurDroite",
                new Vector3(porteX + porteLargeur + droiteWidth / 2, murHauteur / 2, 0),
                new Vector3(droiteWidth, murHauteur, murEpaisseur));
        }

        // 3. Partie HAUT (au-dessus de la porte)
        if (hautHeight > 0.01f)
        {
            CreerPartie("MurHaut",
                new Vector3(porteX + porteLargeur / 2, porteHauteur + hautHeight / 2, 0),
                new Vector3(porteLargeur, hautHeight, murEpaisseur));
        }

        // 4. Placer la porte (si un prefab est assigné)
        if (doorPrefab != null)
        {
            PlacerPorte();
        }
        else
        {
            Debug.LogWarning("Aucun prefab de porte assigné ! Glisse ton prefab dans 'Door Prefab'.");
        }

        Debug.Log("Mur avec porte créé !");
    }

    private void CreerPartie(string nom, Vector3 position, Vector3 taille)
    {
        GameObject partie = GameObject.CreatePrimitive(PrimitiveType.Cube);
        partie.name = nom;
        partie.transform.parent = transform;
        partie.transform.localPosition = position;
        partie.transform.localScale = taille;

        if (murMaterial != null)
        {
            partie.GetComponent<Renderer>().material = murMaterial;
        }
    }

    private void PlacerPorte()
    {
        // Chercher si une porte existe déjà
        Transform existingDoor = transform.Find("Door");
        if (existingDoor != null)
        {
            DestroyImmediate(existingDoor.gameObject);
        }

        // Instancier le prefab de porte
        GameObject door = Instantiate(doorPrefab, transform);
        door.name = "Door";

        // Positionner la porte
        Vector3 doorPosition = new Vector3(
            porteX + porteLargeur / 2,
            0,
            0
        );
        door.transform.localPosition = doorPosition;

        // Rotation (inverser si nécessaire)
        if (inverserPorte)
        {
            door.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            door.transform.localRotation = Quaternion.identity;
        }

        Debug.Log("Porte placée à " + doorPosition);
    }

    // Visualisation dans l'éditeur
    void OnDrawGizmos()
    {
        // Cadre du mur complet
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            transform.position + new Vector3(murLargeur / 2, murHauteur / 2, 0),
            new Vector3(murLargeur, murHauteur, murEpaisseur));

        // Zone de la porte
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position + new Vector3(porteX + porteLargeur / 2, porteHauteur / 2, 0),
            new Vector3(porteLargeur, porteHauteur, murEpaisseur + 0.5f));

        // Position exacte de la porte
        Gizmos.color = Color.green;
        Vector3 doorPos = transform.position + new Vector3(porteX + porteLargeur / 2, 0, 0);
        Gizmos.DrawSphere(doorPos, 0.1f);

        // Direction de la porte
        Vector3 doorForward = inverserPorte ? -transform.forward : transform.forward;
        Gizmos.DrawRay(doorPos, doorForward * 0.5f);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WallWithDoorMaker))]
public class WallWithDoorMakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WallWithDoorMaker maker = (WallWithDoorMaker)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "1. Ajuste les dimensions du mur\n" +
            "2. Glisse ton prefab de porte\n" +
            "3. Positionne l'ouverture\n" +
            "4. Clique sur le bouton !",
            MessageType.Info);

        EditorGUILayout.Space();

        // Vérifier si un prefab est assigné
        if (maker.doorPrefab == null)
        {
            EditorGUILayout.HelpBox("⚠️ Glisse ton prefab de porte dans 'Door Prefab' !", MessageType.Warning);
        }

        if (GUILayout.Button("CRÉER MUR AVEC PORTE", GUILayout.Height(50)))
        {
            maker.CreerMurAvecPorte();
        }
    }
}
#endif
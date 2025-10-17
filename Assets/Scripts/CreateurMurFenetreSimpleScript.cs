using UnityEngine;
using UnityEditor;

public class CreateurMurFenetreSimple : MonoBehaviour
{
    [Header("Dimensions du Mur")]
    public float murLargeur = 5f;
    public float murHauteur = 3f;
    public float murEpaisseur = 0.2f;

    [Header("Dimensions de la Fenêtre")]
    public float fenetreX = 2f; // Position horizontale (depuis la gauche)
    public float fenetreY = 1f; // Position verticale (depuis le bas)
    public float fenetreLargeur = 1.5f;
    public float fenetreHauteur = 2f;

    [Header("Matériau")]
    public Material murMaterial;

    [ContextMenu("Créer Mur avec Fenêtre")]
    public void CreerMurAvecFenetre()
    {
        // Détruire les enfants existants
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Calculer les dimensions des 4 parties du mur
        float gaucheWidth = fenetreX;
        float droiteWidth = murLargeur - fenetreX - fenetreLargeur;
        float basHeight = fenetreY;
        float hautHeight = murHauteur - fenetreY - fenetreHauteur;

        // 1. Partie GAUCHE (si existe)
        if (gaucheWidth > 0.01f)
        {
            CreerBoutMur("Gauche",
                new Vector3(gaucheWidth / 2, murHauteur / 2, 0),
                new Vector3(gaucheWidth, murHauteur, murEpaisseur));
        }

        // 2. Partie DROITE (si existe)
        if (droiteWidth > 0.01f)
        {
            CreerBoutMur("Droite",
                new Vector3(fenetreX + fenetreLargeur + droiteWidth / 2, murHauteur / 2, 0),
                new Vector3(droiteWidth, murHauteur, murEpaisseur));
        }

        // 3. Partie BAS (si existe)
        if (basHeight > 0.01f)
        {
            CreerBoutMur("Bas",
                new Vector3(fenetreX + fenetreLargeur / 2, basHeight / 2, 0),
                new Vector3(fenetreLargeur, basHeight, murEpaisseur));
        }

        // 4. Partie HAUT (si existe)
        if (hautHeight > 0.01f)
        {
            CreerBoutMur("Haut",
                new Vector3(fenetreX + fenetreLargeur / 2, fenetreY + fenetreHauteur + hautHeight / 2, 0),
                new Vector3(fenetreLargeur, hautHeight, murEpaisseur));
        }

        Debug.Log("Mur avec fenêtre créé !");
    }

    private void CreerBoutMur(string nom, Vector3 position, Vector3 taille)
    {
        GameObject bout = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bout.name = nom;
        bout.transform.parent = transform;
        bout.transform.localPosition = position;
        bout.transform.localScale = taille;

        if (murMaterial != null)
        {
            bout.GetComponent<Renderer>().material = murMaterial;
        }
    }

    // Visualiser dans l'éditeur - traits 3d persp
    private void OnDrawGizmos()
    {
        // Cadre du mur complet
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            transform.position + new Vector3(murLargeur / 2, murHauteur / 2, 0),
            new Vector3(murLargeur, murHauteur, murEpaisseur));

        // Zone fenêtre
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position + new Vector3(fenetreX + fenetreLargeur / 2, fenetreY + fenetreHauteur / 2, 0),
            new Vector3(fenetreLargeur, fenetreHauteur, murEpaisseur + 0.1f));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CreateurMurFenetreSimple))]
public class SimpleWindowMakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CreateurMurFenetreSimple maker = (CreateurMurFenetreSimple)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Ajuster les valeurs ci-dessus, puis cliquer sur le bouton pour générer le mur avec fenêtre.", MessageType.Info);

        EditorGUILayout.Space();
        if (GUILayout.Button("CRÉER MUR AVEC FENÊTRE", GUILayout.Height(50)))
        {
            maker.CreerMurAvecFenetre();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Réinitialiser Valeurs par Défaut", GUILayout.Height(30)))
        {
            Undo.RecordObject(maker, "Reset Window Maker");
            maker.murLargeur = 10f;
            maker.murHauteur = 5f;
            maker.murEpaisseur = 0.2f;
            maker.fenetreX = 4f;
            maker.fenetreY = 1.5f;
            maker.fenetreLargeur = 2f;
            maker.fenetreHauteur = 2f;
        }
    }
}
#endif
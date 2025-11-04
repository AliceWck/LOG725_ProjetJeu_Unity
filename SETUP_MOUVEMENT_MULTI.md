# Guide rapide : Créer un objet avec mouvements synchronisés en multijoueur (Mirror)

## 1. Préparer le prefab

- Créez un GameObject (ex : Joueur) dans la scène.
- Ajoutez le composant **NetworkIdentity**.
- Ajoutez un script qui hérite de **NetworkBehaviour** (ex : GamePlayer).

## Exemple de script minimal

```csharp
using Mirror;
using UnityEngine;

public class SimpleNetworkPlayer : NetworkBehaviour
{
    public float speed = 5f;

    void Update()
    {
        if (!isLocalPlayer) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0, v) * speed * Time.deltaTime;
        transform.Translate(move);
    }
}
```

## 2. Synchroniser la position

- Dans le script, utilisez `[SyncVar]` pour les variables à synchroniser (ex : position, état).
- Pour le mouvement, utilisez `transform.position` côté serveur et laissez Mirror synchroniser.
- Pour un mouvement fluide, ajoutez le composant **NetworkTransform** (Mirror) sur le prefab.

## 3. Spawner en réseau

- Ajoutez le prefab dans la liste **Spawnable Prefabs** du NetworkManager.
- Utilisez `NetworkServer.Spawn(objet)` pour instancier côté serveur.

## 4. Contrôler le joueur local

- Dans le script, vérifiez `isLocalPlayer` pour activer les contrôles uniquement sur le client local.

```csharp
public override void OnStartLocalPlayer()
{
    // Activer caméra, contrôles, etc. uniquement pour le joueur local
}
```

## 5. Résumé

- **NetworkIdentity** obligatoire
- **NetworkBehaviour** pour la logique
- **NetworkTransform** pour synchroniser la position
- Spawner via `NetworkServer.Spawn`
- Contrôles locaux avec `isLocalPlayer`

Pour plus d'options : voir la doc Mirror officielle !

using UnityEngine;
using UnityEngine.UIElements;

namespace UI.MainMenu
{
    public class RulesPanelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIDocument uiDocument;

        private VisualElement root;
        private VisualElement rulesOverlay;
        private Button backButton;

        // Labels pour le texte
        private Label goalText;
        private Label gardienText;
        private Label ombresText;
        private Label fantomeText;

        // Textes des règles
        private const string GOAL_TEXT = @"Le jeu se déroule dans un manoir hanté plongé dans l'obscurité. Les joueurs sont divisés en deux équipes : Gardien et Ombres.

Les Ombres doivent trouver toutes les clés dispersées dans le manoir et s'échapper avant l'aube, tout en évitant le Gardien qui rôde dans le manoir.

Le Gardien doit empêcher les Ombres de s'échapper en les trouvant et en éliminant grâce à sa lampe torche, avant qu'elles ne trouvent toutes les clés.
        
Le temps est limité : si l'aube se lève sans que les Ombres ne se soient échappées, le Gardien gagne automatiquement. 

Attention aux bruits, et aux autres résidents du manoir, qui pourront limiter votre discrétion !";

        private const string GARDIEN_TEXT = @"Objectif : Éliminer toutes les Ombres avant qu'elles ne s'échappent.

Caractéristiques :
        • Lampe torche avec batterie limitée (rechargeable aux stations)
        • Peut éliminer les ombres en les flashant avec la lampe
        • Peut interagir avec les lampes et fenêtres du Manoir
   
Stratégie :
        • Explorez méthodiquement le manoir
        • Économisez votre batterie de lampe, car pour la recharger, vous 
        devrez retourner à la station de chargement
        • Allumez les lampes, ouvrez les fenêtres, pour limiter les cachettes 
        des Ombres 
        • Ecoutez les bruits, ils vous donneront des indices sur les cachettes
        • Tendez des embuscades aux Ombres isolées";

        private const string OMBRES_TEXT = @"Objectif : Trouver toutes les clés et s'échapper du manoir à l'aube, et survivre à la nuit.

Caractéristiques :
        • Vulnérable à la lumière de la lampe du Gardien
        • Peuvent interagir avec les lampes et fenêtres du Manoir

Stratégie :
        • Fusionnez avec les ombres pour être invisible
        • Évitez la lumière directe des lampes : elles vous sortent des 
        ombres
        • Évitez la lumière directe de la lampe du Gardien: elle vous blesse
        • Eteignez les lampes, fermez les fenêtres pour limiter la lumière
        • Surveillez les zones des clés
        • Ramassez les clés dispersées dans le manoir
        • Ouvrez les portes de sortie une fois les clés récupérées";

        private const string FANTOME_TEXT = @"État : Une Ombre éliminée devient un Fantôme.

Caractéristiques :
        • Peut traverser les murs
        • Vision complète du manoir
        • Peut faire du bruit et intéragir avec les lampes

Restrictions :
        • Ne peut plus interagir avec les fenêtres
        • Ne peut plus communiquer avec les vivants via le chat
        • Reste spectateur jusqu'à la fin de la partie

        Vous pouvez observer la partie, embêter les autres joueurs et 
        explorer librement le manoir en attendant la fin de la partie.";

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            root = uiDocument.rootVisualElement;
            InitializeElements();
            SetupEventHandlers();
            SetRulesText();

            // Masquer par défaut
            Hide();
        }

        private void InitializeElements()
        {
            rulesOverlay = root.Q<VisualElement>("rules-overlay");
            backButton = root.Q<Button>("back-button");

            // Récupérer les labels de texte
            goalText = root.Q<Label>("goal-text");
            gardienText = root.Q<Label>("gardien-text");
            ombresText = root.Q<Label>("ombres-text");
            fantomeText = root.Q<Label>("fantome-text");
        }

        private void SetupEventHandlers()
        {
            backButton?.RegisterCallback<ClickEvent>(evt => Hide());
        }

        private void SetRulesText()
        {
            if (goalText != null)
                goalText.text = GOAL_TEXT;

            if (gardienText != null)
                gardienText.text = GARDIEN_TEXT;

            if (ombresText != null)
                ombresText.text = OMBRES_TEXT;

            if (fantomeText != null)
                fantomeText.text = FANTOME_TEXT;
        }

        #region Public Methods

        public void Show()
        {
            if (rulesOverlay != null)
            {
                rulesOverlay.RemoveFromClassList("hidden");
                rulesOverlay.style.display = DisplayStyle.Flex;
            }
        }

        public void Hide()
        {
            if (rulesOverlay != null)
            {
                rulesOverlay.AddToClassList("hidden");
                rulesOverlay.style.display = DisplayStyle.None;
            }
        }

        #endregion
    }
}
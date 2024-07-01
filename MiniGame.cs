using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;




public class MiniGame
{
    private Random random = new Random();

    private int initialABar;
    private int initialBBar;
    private int initialCBar;

    private int maximumABar = 10;
    private int maximumBBar = 100;
    private int maximumCBar = 1000;

    private int currentABar;
    private int currentBBar;
    private int currentCBar;

    private int goodABar;
    private int goodBBar;
    private int goodCBar;

    private Stopwatch stopwatch = new Stopwatch();
    private int timeLimit = 999; // Temps limite en secondes

    private bool gameWon = false;
    private bool gameLost = false;
    private object lockObject = new object();

    public MiniGame()
    {
        goodABar = random.Next(0, 11);
        goodBBar = random.Next(0, 11) * 10; // Multiples de 10
        goodCBar = random.Next(0, 11) * 100; // Multiples de 100

        do
        {
            initialABar = random.Next(0, 11);
        } while (initialABar == goodABar);

        do
        {
            initialBBar = random.Next(0, 11) * 10;
        } while (initialBBar == goodBBar);

        do
        {
            initialCBar = random.Next(0, 11) * 100;
        } while (initialCBar == goodCBar);

        currentABar = initialABar;
        currentBBar = initialBBar;
        currentCBar = initialCBar;
    }

    public void Start()
    {
        stopwatch.Start();
        Thread timerThread = new Thread(Timer);
        timerThread.Start();

        Console.CursorVisible = false; // Masquer le curseur

        while (true)
        {
            lock (lockObject)
            {
                if (gameLost || gameWon)
                {
                    break;
                }

                DisplayBar(currentABar, maximumABar, goodABar, "Alpha   ");
                DisplayBar(currentBBar, maximumBBar, goodBBar, "Beta    ");
                DisplayBar(currentCBar, maximumCBar, goodCBar, "Epsilon ");

                int timeRemaining = timeLimit - stopwatch.Elapsed.Seconds;
                Console.SetCursorPosition(0, 4); // Placez le curseur à la position où le temps restant est affiché

                if (timeRemaining <= 10)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.WriteLine($"Temps restant : {timeRemaining} secondes".PadRight(Console.WindowWidth - 1));
                Console.ResetColor();
                Console.WriteLine("");
                Console.WriteLine("1. Diminuer la barre Alpha (-1)");
                Console.WriteLine("2. Augmenter la barre Alpha (+1)");
                Console.WriteLine("4. Diminuer la barre Beta (-10)");
                Console.WriteLine("5. Augmenter la barre Beta (+10)");
                Console.WriteLine("7. Diminuer la barre Epsilon (-100)");
                Console.WriteLine("8. Augmenter la barre Epsilon (+100)");
                
                Console.WriteLine("9. Quitter");

                Console.SetCursorPosition(0, 13); // Placez le curseur à la position suivante
                // Test
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("");
                Console.WriteLine($"//Test Alpha:{goodABar} Beta:{goodBBar} Epsilon:{goodCBar}");
                Console.ResetColor();

                if (Console.KeyAvailable)
                {
                    char choix = Console.ReadKey(true).KeyChar;

                    switch (choix)
                    {
                        case '2':
                            currentABar = Math.Min(currentABar + 1, maximumABar);
                            break;
                        case '1':
                            currentABar = Math.Max(currentABar - 1, 0);
                            break;
                        case '5':
                            currentBBar = Math.Min(currentBBar + 10, maximumBBar);
                            break;
                        case '4':
                            currentBBar = Math.Max(currentBBar - 10, 0);
                            break;
                        case '8':
                            currentCBar = Math.Min(currentCBar + 100, maximumCBar);
                            break;
                        case '7':
                            currentCBar = Math.Max(currentCBar - 100, 0);
                            break;
                        case '9':
                            gameLost = true;
                            break;
                        default:
                            Console.WriteLine("Choix invalide. Essayez encore.");
                            break;
                    }

                    if (currentABar == goodABar && currentBBar == goodBBar && currentCBar == goodCBar)
                    {
                        gameWon = true;

                        // Afficher les barres une dernière fois pour les mettre à jour en vert
                        DisplayBar(currentABar, maximumABar, goodABar, "Alpha   ");
                        DisplayBar(currentBBar, maximumBBar, goodBBar, "Beta    ");
                        DisplayBar(currentCBar, maximumCBar, goodCBar, "Epsilon ");

                        // Ajouter un court délai pour que l'utilisateur puisse voir les barres en vert
                        Thread.Sleep(500);
                    }
                }

                Thread.Sleep(100); // Ajoute une légère pause pour réduire la charge du CPU
            }
        }

        stopwatch.Stop();
        Console.CursorVisible = true; // Réafficher le curseur

        if (gameWon)
        {
            Console.SetCursorPosition(0, 5); // Placez le curseur à la position suivante
            Console.WriteLine("Félicitations ! Vous avez déverrouillé le coffre !");
        }
        else if (gameLost)
        {
            Console.SetCursorPosition(0, 5); // Placez le curseur à la position suivante
            Console.WriteLine("Temps écoulé ! Vous avez perdu le mini-jeu.");
        }
    }

    private void Timer()
    {
        while (true)
        {
            lock (lockObject)
            {
                if (stopwatch.Elapsed.Seconds >= timeLimit)
                {
                    gameLost = true;
                    break;
                }

                if (gameWon)
                {
                    break;
                }
            }
            Thread.Sleep(1000);
        }
    }

    /*private void DisplayBar(int currentBar, int maximumBar, int goodBar, string barLabel)
    {
        // Calcule le pourcentage de la barre actuelle par rapport à la valeur maximale, multiplié par 20 pour une barre de longueur 20
        int percentage = (currentBar * 20) / maximumBar;

        // Crée une représentation de la barre en utilisant des caractères '|' pour le pourcentage calculé et complète à droite jusqu'à 20 caractères
        string bar = new string('|', percentage).PadRight(20);

        // Calcule la distance absolue entre la valeur actuelle et la valeur cible
        int distance = Math.Abs(currentBar - goodBar);

        // Définit un seuil pour déterminer les couleurs : si goodBar est 0, utilise maximumBar / 2, sinon utilise goodBar / 2
        // Ajustement pour s'assurer que le seuil est au moins 1 pour éviter des valeurs trop restrictives
        int threshold = Math.Max(1, goodBar == 0 ? maximumBar / 2 : goodBar / 2);

        // Positionne le curseur en fonction de l'étiquette de la barre pour afficher chaque barre sur une ligne spécifique
        Console.SetCursorPosition(0, barLabel == "Alpha   " ? 0 : barLabel == "Beta    " ? 1 : 2);

        // Affecte une couleur à la barre en fonction de la distance par rapport à la valeur cible
        if (currentBar == goodBar)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        else if (distance <= threshold)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        // Affiche la barre avec son étiquette, sa représentation graphique, et ses valeurs actuelle et maximale
        Console.WriteLine($"{barLabel}[{bar}] {currentBar} / {maximumBar}".PadRight(Console.WindowWidth - 1));

        // Réinitialise les couleurs de la console à leurs valeurs par défaut
        Console.ResetColor();
    }*/

    private void DisplayBar(int currentBar, int maximumBar, int goodBar, string barLabel)
    {
        // Calcul du pourcentage de la barre actuelle par rapport à la valeur maximale
        int percentage = (currentBar * 100) / maximumBar;

        // Calcul du seuil de 30% de la valeur maximale pour déterminer la couleur jaune
        int threshold = (20 * maximumBar) / 100;

        // Détermine la couleur en fonction de la distance par rapport à la valeur cible
        int distance = Math.Abs(currentBar - goodBar);
        ConsoleColor barColor;

        if (currentBar == goodBar)
        {
            barColor = ConsoleColor.Green;
        }
        else if (distance <= threshold)
        {
            barColor = ConsoleColor.Yellow;
        }
        else
        {
            barColor = ConsoleColor.Red;
        }

        StringBuilder barBuilder = new StringBuilder();

        // Ajoute les points avant le marqueur |
        for (int i = 0; i < percentage / 10; i++)
        {
            barBuilder.Append('.');
        }

        // Ajoute le marqueur |
        barBuilder.Append('|');

        // Ajoute les points après le marqueur |
        for (int i = percentage / 10 + 1; i < 11; i++)
        {
            barBuilder.Append('.');
        }

        // Positionne le curseur en fonction de l'étiquette de la barre pour l'affichage sur une ligne spécifique
        Console.SetCursorPosition(0, barLabel == "Alpha   " ? 0 : barLabel == "Beta    " ? 1 : 2);

        // Affecte une couleur à la barre en fonction de la distance par rapport à la valeur cible
        Console.ForegroundColor = barColor;

        // Affiche la barre avec son étiquette, sa représentation graphique, et ses valeurs actuelle et maximale
        Console.WriteLine($"{barLabel}[{barBuilder}] {currentBar} / {maximumBar}".PadRight(Console.WindowWidth - 1));

        // Réinitialise les couleurs de la console à leurs valeurs par défaut
        Console.ResetColor();
    }



}

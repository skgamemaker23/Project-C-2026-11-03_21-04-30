
public static class CardCompatibility
{
    // Player 1 cards
    public const int Witch = 1;
    public const int Vamp = 2;
    public const int Wolf = 3;
    public const int Ghost = 4;

    // Player 2 cards
    public const int Fairy = 5;
    public const int Zombie = 6;
    public const int Skeleton = 7;
    public const int Demon = 8;

    public static bool IsCompatible(int player1Card, int player2Card)
    {
        return
            (player1Card == Ghost && player2Card == Zombie) ||
            (player1Card == Wolf && player2Card == Fairy) ||
            (player1Card == Vamp && player2Card == Demon) ||
            (player1Card == Witch && player2Card == Fairy) ||
            (player1Card == Ghost && player2Card == Fairy) ||
            (player1Card == Witch && player2Card == Skeleton) ||
            (player1Card == Ghost && player2Card == Skeleton);
    }
}

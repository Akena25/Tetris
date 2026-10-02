using System;
using System.Collections.Generic;
using System.Threading;
class GameField
{
    public const int Width = 10;
    public const int Height = 20;
    public int[,] Field { get; private set; }
    public GameField()
    {
        Field = new int[Height, Width];
    }
    public void Clear()
    {
        Field = new int[Height, Width];
    }
}
class Program
{
    static GameField gameField = new GameField();
    static Random random = new Random();
    static int score = 0;
    static int[,] currentPiece = new int[1, 1];
    static int pieceX, pieceY;
    static bool gameOver = false;
    static int[][,] pieces =
    {
    new int[,] { { 1, 1, 1, 1 } }, // I
    new int[,] { { 1, 1 }, { 1, 1 } }, // O
    new int[,] { { 0, 1, 0 }, { 1, 1, 1 } }, // T
    new int[,] { { 1, 0 }, { 1, 0 }, { 1, 1 } }, // L
    new int[,] { { 0, 1 }, { 0, 1 }, { 1, 1 } }, // J
    new int[,] { { 0, 1, 1 }, { 1, 1, 0 } }, // S
    new int[,] { { 1, 1, 0 }, { 0, 1, 1 } } // Z
};
    static void Main() // Главный цикл игры
    {
        Console.CursorVisible = false;
        while (true)
        {
            RestartGame(); // Перезапуск игры
            DateTime lastFall = DateTime.Now;
            int fallDelay = 500;
            while (!gameOver)
            {
                if (Console.KeyAvailable) // Обработка ввода пользователя
                {
                    ConsoleKey key = Console.ReadKey(true).Key;

                    switch (key)
                    {
                        case ConsoleKey.LeftArrow:
                            Move(-1, 0);
                            break;

                        case ConsoleKey.RightArrow:
                            Move(1, 0);
                            break;

                        case ConsoleKey.DownArrow:
                            Move(0, 1);
                            break;

                        case ConsoleKey.UpArrow:
                        case ConsoleKey.Spacebar:
                            Rotate();
                            break;

                        case ConsoleKey.Escape:
                            return;
                    }
                }

                if ((DateTime.Now - lastFall).TotalMilliseconds >= fallDelay) // Автоматическое падение фигуры
                {
                    if (!Move(0, 1))
                    {
                        LockPiece();
                        ClearLines();
                        SpawnPiece();
                        fallDelay = Math.Max(
                            100,
                            500 - score / 50 * 10
                        );
                    }

                    lastFall = DateTime.Now;
                }

                Draw();

                Thread.Sleep(30);
            }
            Draw();
            Console.SetCursorPosition(0, GameField.Height + 3);
            Console.Clear();
            Console.WriteLine("╔════════════════════════════╗");
            Console.WriteLine("║       ИГРА ОКОНЧЕНА!       ║");
            Console.WriteLine("╚════════════════════════════╝");
            Console.WriteLine($"Ваш счёт: {score}");
            Console.WriteLine();
            Console.WriteLine("Нажмите ENTER, чтобы начать заново.");
            Console.WriteLine("Нажмите ESC, чтобы выйти.");

            while (true)// Ожидание ввода пользователя для перезапуска или выхода
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter)
                {
                    break;
                }

                if (key == ConsoleKey.Escape)
                {
                    return;
                }
            }
        }
    }
    static void SpawnPiece() // Создание новой фигуры
    {
        currentPiece = pieces[random.Next(pieces.Length)];
        pieceX = GameField.Width / 2 - currentPiece.GetLength(1) / 2;
        pieceY = 0;

        if (Collision(pieceX, pieceY, currentPiece))
            gameOver = true;
    }
    static void RestartGame() // Перезапуск игры
    {
        Console.Clear();
        gameField.Clear();
        score = 0;
        gameOver = false;
        SpawnPiece();
    }
    static bool Collision(int x, int y, int[,] piece) // Проверка столкновения фигуры с границами поля или другими фигурами
    {
        for (int r = 0; r < piece.GetLength(0); r++)
        {
            for (int c = 0; c < piece.GetLength(1); c++)
            {
                if (piece[r, c] == 0)
                    continue;
                int nx = x + c;
                int ny = y + r;
                if (nx < 0 || nx >= GameField.Width || ny >= GameField.Height)
                    return true;
                if (ny >= 0 && gameField.Field[ny, nx] != 0)
                    return true;
            }
        }

        return false;
    }
    static bool Move(int dx, int dy) // Перемещение фигуры
    {
        if (!Collision(pieceX + dx, pieceY + dy, currentPiece))
        {
            pieceX += dx;
            pieceY += dy;
            return true;
        }

        return false;
    }
    static void Rotate() // Поворот фигуры
    {
        int rows = currentPiece.GetLength(0);
        int cols = currentPiece.GetLength(1);

        int[,] rotated = new int[cols, rows];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                rotated[c, rows - 1 - r] = currentPiece[r, c];
            }
        }

        if (!Collision(pieceX, pieceY, rotated))
            currentPiece = rotated;
    }
    static void LockPiece() // Закрепление фигуры на игровом поле
    {
        for (int r = 0; r < currentPiece.GetLength(0); r++)
        {
            for (int c = 0; c < currentPiece.GetLength(1); c++)
            {
                if (currentPiece[r, c] == 1)
                {
                    int x = pieceX + c;
                    int y = pieceY + r;

                    if (y >= 0)
                        gameField.Field[y, x] = 1;
                }
            }
        }
    }
    static void ClearLines() // Очистка заполненных линий и начисление очков
    {
        int linesCleared = 0;

        for (int row = GameField.Height - 1; row >= 0; row--)
        {
            bool isFull = true;

            for (int col = 0; col < GameField.Width; col++)
            {
                if (gameField.Field[row, col] == 0)
                {
                    isFull = false;
                    break;
                }
            }

            if (isFull)
            {
                linesCleared++;

                for (int r = row; r > 0; r--) // Сдвигаем все строки вниз
                {
                    for (int col = 0; col < GameField.Width; col++)
                    {
                        gameField.Field[r, col] =
                            gameField.Field[r - 1, col];
                    }
                }
                for (int col = 0; col < GameField.Width; col++) // Очищаем верхнюю строку
                {
                    gameField.Field[0, col] = 0;
                }
                row++;
            }
        }

        score += linesCleared switch // Начисление очков в зависимости от количества очищенных линий
        {
            1 => 100,
            2 => 300,
            3 => 500,
            4 => 800,
            _ => 0
        };
    }
    static void Draw() // Отрисовка игрового поля и текущей фигуры
    {
        Console.SetCursorPosition(0, 0);

        Console.WriteLine("╔════════════════════╗");

        for (int r = 0; r < GameField.Height; r++)
        {
            Console.Write("║");

            for (int c = 0; c < GameField.Width; c++)
            {
                bool isPiece = false;

                //проверка фигуры в клетке
                for (int pr = 0; pr < currentPiece.GetLength(0); pr++)
                {
                    for (int pc = 0; pc < currentPiece.GetLength(1); pc++)
                    {
                        if (currentPiece[pr, pc] == 1 &&
                            pieceX + pc == c &&
                            pieceY + pr == r)
                        {
                            isPiece = true;
                        }
                    }
                }

                //если клетка занята
                if (gameField.Field[r, c] == 1)
                {
                    Console.Write("██");
                }
                //если здесь находится фигура
                else if (isPiece)
                {
                    Console.Write("██");
                }
                //пустая клетка
                else
                {
                    Console.Write("  ");
                }
            }

            Console.WriteLine("║");
        }

        Console.WriteLine("╚════════════════════╝");
        Console.WriteLine($"Счёт: {score}");
        Console.WriteLine();
        Console.WriteLine("вправо / влево — движение");
        Console.WriteLine("вниз — ускорить падение");
        Console.WriteLine("вверх / пробел — поворот фигуры");
        Console.WriteLine("Esc — выход");
    }
}
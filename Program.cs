
using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    const int Width = 10;
    const int Height = 20;

    static int[,] field = new int[Height, Width];
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

    static void Main()
    {
        Console.CursorVisible = false;

        while (true)
        {
            // Начинаем новую игру
            RestartGame();

            DateTime lastFall = DateTime.Now;
            int fallDelay = 500;

            // Игровой цикл
            while (!gameOver)
            {
                // Управление
                if (Console.KeyAvailable)
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
                            if (Move(0, 1)) ;
                            break;

                        case ConsoleKey.UpArrow:
                        case ConsoleKey.Spacebar:
                            Rotate();
                            break;

                        case ConsoleKey.Escape:
                            Console.CursorVisible = true;
                            return;
                    }
                }

                // Автоматическое падение фигуры
                if ((DateTime.Now - lastFall).TotalMilliseconds >= fallDelay)
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

            Console.SetCursorPosition(0, Height + 3);

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
                    // Начинаем новую игру
                    break;
                }

                if (key == ConsoleKey.Escape)
                {
                    Console.CursorVisible = true;
                    return;
                }
            }
        }
    }

    static void SpawnPiece() // Создание новой фигуры
    {
        currentPiece = pieces[random.Next(pieces.Length)];
        pieceX = Width / 2 - currentPiece.GetLength(1) / 2;
        pieceY = 0;

        if (Collision(pieceX, pieceY, currentPiece))
            gameOver = true;
    }

    static void RestartGame() // Перезапуск игры
    {
        Console.Clear();
        field = new int[Height, Width];
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

                if (nx < 0 || nx >= Width || ny >= Height)
                    return true;

                if (ny >= 0 && field[ny, nx] != 0)
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
                        field[y, x] = 1;
                }
            }
        }
    }

    static void ClearLines() // Очистка заполненных линий и начисление очков
    {
        int linesCleared = 0;

        for (int r = Height - 1; r >= 0; r--)
        {
            bool full = true;

            for (int c = 0; c < Width; c++)
            {
                if (field[r, c] == 0)
                {
                    full = false;
                    break;
                }
            }

            if (full)
            {
                linesCleared++;

                for (int row = r; row > 0; row--)
                {
                    for (int c = 0; c < Width; c++)
                        field[row, c] = field[row - 1, c];
                }

                for (int c = 0; c < Width; c++)
                    field[0, c] = 0;

                r++;
            }
        }

        score += linesCleared switch
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

        for (int r = 0; r < Height; r++)
        {
            Console.Write("║");

            for (int c = 0; c < Width; c++)
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
                if (field[r, c] == 1)
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
        Console.WriteLine("← → — движение");
        Console.WriteLine("↓ — ускорить падение");
        Console.WriteLine("↑ / Пробел — поворот");
        Console.WriteLine("Esc — выход");
    }
}
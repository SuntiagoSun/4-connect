using Godot;
using System;
using System.Collections.Generic;
//КОД ГЛАВНОЙ СЦЕНЫ
public partial class main_level : Node2D
{
	private Node Globals;         //Ссылка на глобальный скрипт Globals
	private bool cant = false;    //Запрещает компьютеру делать преждевременный ход
	private PackedScene boomSc;   //Ссылка на предхагруженную сцену с эффектом взрыва 
	private PackedScene diskSc;   //Ссылка на предзагруженную сцену с диском 
	private int[][] field = new int[][]   //Двумерный массив, представляющий игровое поле
	{
		new int[] {0,0,0,0,0,0,0},
		new int[] {0,0,0,0,0,0,0},
		new int[] {0,0,0,0,0,0,0},
		new int[] {0,0,0,0,0,0,0},
		new int[] {0,0,0,0,0,0,0},
		new int[] {0,0,0,0,0,0,0}
	};

	private const int startX = 415;    //Отступ пикселей от края экрана по X
	private const int startY = 815;    //Отступ пикселей от края экрана по Y
	private Node disksNode;            //Ссылка на сцену с диском 
	private Node spawnersNode;         //Ссылка на сцену с маркером
	private Node particlesNode;        //Ссылка на сцену с частицами

	/*Функция загрузки необходимых сцен. Внутри загружает все необходимые сцены для работы игры.
	 Вызывается единожды, когда эта сцены впервые появляется на главной сцене. */
	public override void _Ready()       
	{
		//Загружаем ссылки на некоторые сцены
		Globals = GetNode<Node>("/root/Globals");
		disksNode = GetNode<Node>("Disks");
		spawnersNode = GetNode<Node>("Field/Spawners");
		particlesNode = GetNode<Node>("Particles");

		//Загружаем предзагруженные сцены
		boomSc = ResourceLoader.Load<PackedScene>("res://Objects/win_particle.tscn");
		diskSc = ResourceLoader.Load<PackedScene>("res://Objects/disk.tscn");
	}

	/* Функция логики бота (компьютера). Внутри проверяет, настал ли момент хода компьютера, после
	 чего загружает предзагруженную сцену диска на сцену в самую выгодную позицию и передаёт ход
	обратно. Вызывается каждый кадр.*/
	public override void _Process(double delta)
	{
		//Проверяем, если сейчас стоит режим игры с компьютером, передался ход компьютеру и можно ли ходить
		if ((int)Globals.Get("turn") == -1 && !(bool)Globals.Get("pvp_mode") && !cant)
		{
			//Подгружаем диск
			var disk = (RigidBody2D)diskSc.Instantiate();
			disksNode.AddChild(disk);
			
			//Рассчитываем лучший столбец для диска
			int posX = ComputerMove(field);
			var spawners = spawnersNode.GetChildren();
			var truePos = ((Node2D)spawners[posX]).GlobalPosition;

			//Задаём диску необходимые свойства и пермещаем на нужную позицию
			disk.Set("player", -1);
			disk.Modulate = new Color(0, 0, 1, 1);
			disk.GlobalPosition = truePos;
			disk.Set("in_field", true);
			disk.LinearVelocity = new Vector2(0, 1);
			disk.Set("toogled", false);

			PhysicsMaterial material = new PhysicsMaterial();
			material.Bounce = 0.01f;
			disk.PhysicsMaterialOverride = material;
			disk.GravityScale = 1;

			//Не можем ходить, пока не походит игрок
			cant = true;
		}
		//Проверяем, походил ли игрок
		if ((int)Globals.Get("turn") == 1)
		{
			cant = false;
		}
	}

	/*Функция обновления состояния поля. Внутри идёт запись в двумерный массив поля нового хода,
	 также идёт проверка на ничью и проверка, победил ли кто-то из игроков. Вызывается в конце
	каждого хода игрока или компьютера.*/
	public void UpdateField(int y, int x, int what)
	{
		//Обновляем поле, меняя один из нулей на ход игрока
		field[y][x] = what;
		Globals.Set("win", CheckWinner(field));
		//Если все 42 места на поле заняты, объявляем конец игры
		if (GetNode<Node>("StaticDisks").GetChildCount() == 42)
		{
			Globals.Set("game_over",true);
		}
		else
		{
			//Если кто-то уже выиграл, объявляем конец игры
			if ((int)Globals.Get("win") != 0)
			{
				Globals.Set("game_over", true);
			}
		}
	}
	/*Функция проверки победителя. Внутри идёт проверка, есть ли на поле последовательность из
	 4 подряд идущих одинаковых дисков, после чего определяется, кто победил или объявляется ничья.
	Вызывается после обновления поля.*/
	private int CheckWinner(int[][] grid)
	{
		//Сохраняем размеры поля
		int n = grid.Length;
		if (n == 0) return 0;
		int m = grid[0].Length;

		bool foundFirst = false;
		bool foundSecond = false;
		//Пробегаемся по всем клеткам поля
		for (int i = 0; i < n; i++)
		{
			for (int j = 0; j < m; j++)
			{
				int cell = grid[i][j];
				//Если клетка равна 0, скипаем цикл
				if (cell == 0) continue;

				// Проверяем, есть ли 4 в ряд по горизонтали
				if (j <= m - 4)
				{
					if (cell == grid[i][j + 1] &&
						cell == grid[i][j + 2] &&
						cell == grid[i][j + 3])
					{
						//Вызываем эффект победы
						Boom(new List<int[]> { new int[] { i, j }, new int[] { i, j + 1 }, new int[] { i, j + 2 }, new int[] { i, j + 3 } });
						if (cell == 1) foundFirst = true;
						else foundSecond = true;
					}
				}

				// Проверяем, есть ли 4 в ряд по вертикали
				if (i <= n - 4)
				{
					if (cell == grid[i + 1][j] &&
						cell == grid[i + 2][j] &&
						cell == grid[i + 3][j])
					{
						//Вызываем эффект победы
						Boom(new List<int[]> { new int[] { i, j }, new int[] { i + 1, j }, new int[] { i + 2, j }, new int[] { i + 3, j } });
						if (cell == 1) foundFirst = true;
						else foundSecond = true;
					}
				}

				// Проверяем, есть ли 4 в ряд по диагонали вниз-вправо
				if (i <= n - 4 && j <= m - 4)
				{
					if (cell == grid[i + 1][j + 1] &&
						cell == grid[i + 2][j + 2] &&
						cell == grid[i + 3][j + 3])
					{
						//Вызываем эффект победы
						Boom(new List<int[]> { new int[] { i, j }, new int[] { i + 1, j + 1 }, new int[] { i + 2, j + 2 }, new int[] { i + 3, j + 3 } });
						if (cell == 1) foundFirst = true;
						else foundSecond = true;
					}
				}

				// Проверяем, есть ли 4 в ряд по диагонали вниз-влево
				if (i <= n - 4 && j >= 3)
				{
					if (cell == grid[i + 1][j - 1] &&
						cell == grid[i + 2][j - 2] &&
						cell == grid[i + 3][j - 3])
					{
						//Вызываем эффект победы
						Boom(new List<int[]> { new int[] { i, j }, new int[] { i + 1, j - 1 }, new int[] { i + 2, j - 2 }, new int[] { i + 3, j - 3 } });
						if (cell == 1) foundFirst = true;
						else foundSecond = true;
					}
				}
			}
		}
		//Определяем, кто победил и победил ли вообще
		if (foundFirst) return 1;
		if (foundSecond) return -1;
		return 0;
	}
	/*Функция анимации эффекта победы. Внутри функция пробегается по координатам дисков,
	 которые встали 4 в ряд и загружает на их месте эффект взрыва частиц.
	Вызывается при победе одного из игрков или компьютера.*/
	private void Boom(List<int[]> indexes)
	{
		//Пробегаемся по координатам
		foreach (int[] index in indexes)
		{
			//Загружаем сцену взрыва
			var boom = boomSc.Instantiate();
			particlesNode.AddChild(boom);

			Node2D boomNode = (Node2D)boom;
			//Ставим каждый взрыв на нужные координаты
			boomNode.GlobalPosition = new Vector2(
				startX + 125 * index[1],
				startY - 120 * (5 - index[0])
			);
			
			GpuParticles2D particles = boom as GpuParticles2D;
			if (particles != null)
			{
				//Начинаем взрыв
				particles.Emitting = true;
			}
		}
	}

	// КОД КОМПЬЮТЕРА
	private const int WIN_SCORE = 1000000;     //Обозначает эвристическую оценку выигрыша компьютера
	private const int DEPTH = 5;               //Обозначает глубину оценки возмодных ходов компьютера

	/*Функция определения хода компьютера. Внутри выбирается столбец, в который компьютеру выгоднее
	 всего кинуть диск. Вызывается после каждого хода игрока*/
	private int ComputerMove(int[][] grid)
	{
		//Формируем массив столбцов, в которые можно кидать диски
		List<int> validColumns = GetValidColumns(grid);
		if (validColumns.Count == 0) return 0;

		
		foreach (int col in validColumns)
		{
			int[][] testGrid = DropPiece(grid, col, 1);
			//Проверяем, может ли выиграть игрок следующим ходом
			if (CheckWin(testGrid, 1))
			{
				//Блокируем ход игроку
				return col;
			}
		}

		//Находим лучший ход с помощью мин-максинга
		return FindBestMove(grid, DEPTH);
	}
	/*Функция определения столбцов, в которые можно ходить. Внутри функция проверяет, есть ли
	 у каждого столбца в самом верху пустое место, после чего сохраняет эти столбцы.
	Вызывается каждый раз перед ходом компьютера.*/
	private List<int> GetValidColumns(int[][] grid)
	{
		List<int> validColumns = new List<int>();
		if (grid.Length == 0) return validColumns;

		int cols = grid[0].Length;
		//Пробегаемся по каждому столбцу
		for (int j = 0; j < cols; j++)
		{
			//Проверяем, свободна ли первая клетка
			if (grid[0][j] == 0)
			{
				validColumns.Add(j);
			}
		}
		//Возвращаем столбцы
		return validColumns;
	}
	/*Функция создания поля для нового хода. Внутри функция создаёт копию игрового поля,
	 после чего по заданному столбцу симулирует ход игрока и обновляет копию поля. Вызывается
	каждый раз, при оценке ходов компьюетром*/
	private int[][] DropPiece(int[][] grid, int column, int player)
	{
		int rows = grid.Length;
		int cols = grid[0].Length;

		// Копируем игровое поле
		int[][] newGrid = new int[rows][];
		for (int i = 0; i < rows; i++)
		{
			newGrid[i] = new int[cols];
			Array.Copy(grid[i], newGrid[i], cols);
		}

		// Находим самую нижнюю свободную позицию в каждом столбце
		for (int i = rows - 1; i >= 0; i--)
		{
			if (newGrid[i][column] == 0)
			{
				//Симулируем ход в эту позицию и завершаем цикл
				newGrid[i][column] = player;
				break;
			}
		}
		//Возвращаем новое поле
		return newGrid;
	}
	/*Функция проверки победы для компьюетра. Внутри идёт проверка, есть ли на поле последовательность из
	 4 подряд идущих одинаковых дисков, после чего определяется, победил ли компьютер.
	Вызывается каждый раз при оценке ходов компьюетром. */
	private bool CheckWin(int[][] grid, int player)
	{
		int rows = grid.Length;
		if (rows == 0) return false;
		int cols = grid[0].Length;

		// Проверяем, есть ли 4  в ряд по горизонтали
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols - 3; j++)
			{
				if (grid[i][j] == player &&
					grid[i][j + 1] == player &&
					grid[i][j + 2] == player &&
					grid[i][j + 3] == player)
				{
					return true;
				}
			}
		}

		// Проверяем, есть ли 4  в ряд по вертикали
		for (int i = 0; i < rows - 3; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				if (grid[i][j] == player &&
					grid[i + 1][j] == player &&
					grid[i + 2][j] == player &&
					grid[i + 3][j] == player)
				{
					return true;
				}
			}
		}

		// Проверяем, есть ли 4  в ряд по диагонали вниз-вправо
		for (int i = 0; i < rows - 3; i++)
		{
			for (int j = 0; j < cols - 3; j++)
			{
				if (grid[i][j] == player &&
					grid[i + 1][j + 1] == player &&
					grid[i + 2][j + 2] == player &&
					grid[i + 3][j + 3] == player)
				{
					return true;
				}
			}
		}

		// Проверяем, есть ли 4  в ряд по диагонали вниз-влево
		for (int i = 0; i < rows - 3; i++)
		{
			for (int j = 3; j < cols; j++)
			{
				if (grid[i][j] == player &&
					grid[i + 1][j - 1] == player &&
					grid[i + 2][j - 2] == player &&
					grid[i + 3][j - 3] == player)
				{
					return true;
				}
			}
		}

		return false;
	}
	/*Функция оценки ходов при помощи мин-максинга. Рекурсивно оценивает возможные ходы игрока
	 и компьютера, максимизируя ценность ходов компьютера и минимизируя ценность ходов игрока.
	Вызывается каждый раз при рассчёте хода компьюетром*/
	private int Minimax(int[][] grid, int depth, int alpha, int beta, bool maximizing)
	{
		List<int> validColumns = GetValidColumns(grid);
		bool terminal = CheckWin(grid, -1) || CheckWin(grid, 1) || validColumns.Count == 0;
		//Достигнута максимальная глубина рекурсии
		if (depth == 0 || terminal)
		{
			if (terminal)
			{
				//Проверяем, можно ли победить одним ходом
				if (CheckWin(grid, -1)) return WIN_SCORE;
				if (CheckWin(grid, 1)) return -WIN_SCORE;
				return 0;
			}
			//Произвожим эвристическую оценку позиции
			return EvaluateBoard(grid);
		}
		//Если ходит компьюетр, максимизируем оценку позиции
		if (maximizing)
		{
			int value = int.MinValue;
			foreach (int col in validColumns)
			{
				//Производим оценку позиции для каждого возможного случая хода компьютера
				int[][] newGrid = DropPiece(grid, col, -1);
				value = Math.Max(value, Minimax(newGrid, depth - 1, alpha, beta, false));
				alpha = Math.Max(alpha, value);
				if (value >= beta) break;
			}
			return value;
		}
		else     //Если ходит игрок, минимизируем оценку позиции
		{
			int value = int.MaxValue;
			foreach (int col in validColumns)
			{
				//Производим оценку позиции для каждого возможного случая хода игрока
				int[][] newGrid = DropPiece(grid, col, 1);
				value = Math.Min(value, Minimax(newGrid, depth - 1, alpha, beta, true));
				beta = Math.Min(beta, value);
				if (value <= alpha) break;
			}
			return value;
		}
	}
	/*Функция определения лучшего хода компьютера. Пробегается по всем возможным вариантам
	 хода, при этом фиксируя самый лучшую оценку позиции для каждого из них. Вызывается 
	перед ходом компьютера*/
	private int FindBestMove(int[][] grid, int depth)
	{
		List<int> validColumns = GetValidColumns(grid);
		int bestScore = int.MinValue;
		int bestCol = validColumns[0];
		int alpha = int.MinValue;
		int beta = int.MaxValue;
		//Пробегается по каждому свободному столбцу
		foreach (int col in validColumns)
		{
			//Симулирует бросок диска в выбранный столбец
			int[][] newGrid = DropPiece(grid, col, -1);
			//Рассчитывает оценку позиции после этого хода
			int score = Minimax(newGrid, depth - 1, alpha, beta, false);
			//Фиксирует лучшую оценку и лучший столбец
			if (score > bestScore)
			{
				bestScore = score;
				bestCol = col;
			}

			alpha = Math.Max(alpha, bestScore);
		}
		//Возвращает лучший столбец для хода
		return bestCol;
	}
	/*Функция оценки позиции поля. Производит оценку конкретной позиции для компьютера,
	 чтобы произвести лучший ход. Вызывается каждый раз перед симуляцией хода компьюетром*/
	private int EvaluateBoard(int[][] grid)
	{
		int score = 0;
		int rows = grid.Length;
		int cols = grid[0].Length;

		// Даёт дополнительный вес ходам в центр поля
		int centerCol = cols / 2;
		for (int i = 0; i < rows; i++)
		{
			if (grid[i][centerCol] == -1)
			{
				score += 3;
			}
		}

		// Производит оценку всех позиций, где можно собрать 4 в ряд компьютером и игроком
		score += EvaluateLines(grid, -1) * 10;
		score -= EvaluateLines(grid, 1) * 10;

		return score;
	}
	/*Функция оценки всевозможных позиций, где можно собрать 4 в ряд. Внутри функция пробегается
	 по всему игровому полю и в каждой точке рассчитывает оценку возможности сделать 4 в ряд
	в любую сторону. Вызывается каждый раз при оценке позиции поля.*/
	private int EvaluateLines(int[][] grid, int player)
	{
		int count = 0;
		int rows = grid.Length;
		int cols = grid[0].Length;

		// Пробегаемся всем клеткам поля и оцениваем горизонтальные позиции возможных 4 в ряд
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols - 3; j++)
			{
				count += EvaluateWindow(new int[] {
					grid[i][j], grid[i][j+1],
					grid[i][j+2], grid[i][j+3]
				}, player);
			}
		}

		//  Пробегаемся всем клеткам поля и оцениваем вертикальные позиции возможных 4 в ряд
		for (int i = 0; i < rows - 3; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				count += EvaluateWindow(new int[] {
					grid[i][j], grid[i+1][j],
					grid[i+2][j], grid[i+3][j]
				}, player);
			}
		}

		// Пробегаемся всем клеткам поля и оцениваем диагональные позиции вниз-вправо возможных 4 в ряд
		for (int i = 0; i < rows - 3; i++)
		{
			for (int j = 0; j < cols - 3; j++)
			{
				count += EvaluateWindow(new int[] {
					grid[i][j], grid[i+1][j+1],
					grid[i+2][j+2], grid[i+3][j+3]
				}, player);
			}
		}

		// Пробегаемся всем клеткам поля и оцениваем диагональные позиции вниз-влево возможных 4 в ряд
		for (int i = 0; i < rows - 3; i++)
		{
			for (int j = 3; j < cols; j++)
			{
				count += EvaluateWindow(new int[] {
					grid[i][j], grid[i+1][j-1],
					grid[i+2][j-2], grid[i+3][j-3]
				}, player);
			}
		}
		//Возвращаем оценку позиции
		return count;
	}
	/*Функция оценки окна на потенциальные 4 в ряд. Пробегается по 4-ым клеткам поля,
	 прибавляя вес за каждый идущий подряд диск. Вызывается каждый раз при расчёте
	всевозможных позиций, где можно собрать 4 в ряд*/
	private int EvaluateWindow(int[] window, int player)
	{
		int opp = -player;
		int score = 0;
		int empty = 0;
		int pieces = 0;
		//Пробегаемся по каждой клетке в заданном диапозоне
		foreach (int cell in window)
		{
			if (cell == player) pieces++;      //Если клетка принадлежит компьютеру, учитвыем её
			else if (cell == opp) return 0;    //Если клетка принадлежит противнику, прекращаем отсчёт
			else empty++;                      //Если клетка пустая, учитываем её
		}
		//Выдаём оценку потенциальной 4 в ряд в зависимости от количества клеток компьютера и свободных клеток
		if (pieces == 3 && empty == 1) score += 100;
		else if (pieces == 2 && empty == 2) score += 10;
		else if (pieces == 1 && empty == 3) score += 1;

		return score;
	}
}

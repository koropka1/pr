using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace FlowerShopCSV
{
    struct Flower
    {
        public int Id;
        public string Name;
        public string Category;
        public double Price;
        public int Quantity;

        public Flower(int id, string name, string category, double price, int quantity)
        {
            Id = id;
            Name = name;
            Category = category;
            Price = price;
            Quantity = quantity;
        }

        public void Print()
        {
            Console.WriteLine($"{Id,-4}{Name,-12}{Category,-12}{Price,8:F2}{Quantity,6}");
        }
    }

    class Program
    {
        static string flowerFile = "data/flowers.csv";
        static string userFile = "data/users.csv";

        static void Main()
        {
            Directory.CreateDirectory("data");
            InitFiles();

            bool authorized = AuthMenu();
            if (!authorized)
                return;

            MainMenu();
        }

        static void InitFiles()
        {
            if (!File.Exists(flowerFile))
            {
                File.WriteAllText(flowerFile, "Id,Name,Category,Price,Quantity\n");

                // Додаємо 3 стартові квітки з потрібними категоріями
                File.AppendAllText(flowerFile, $"{GenerateId(flowerFile)},Орхідея,Декоративна,350,10\n");
                File.AppendAllText(flowerFile, $"{GenerateId(flowerFile)},Тюльпан,Сезонна,45,20\n");
                File.AppendAllText(flowerFile, $"{GenerateId(flowerFile)},Троянда,Святкова,80,15\n");
            }

            if (!File.Exists(userFile))
                File.WriteAllText(userFile, "Id,Email,PasswordHash\n");
        }

        // ===== АВТОРИЗАЦІЯ =====
        static bool AuthMenu()
        {
            Console.WriteLine("1 - Вхід");
            Console.WriteLine("2 - Реєстрація");
            Console.Write("Ваш вибір: ");

            if (!int.TryParse(Console.ReadLine(), out int choice))
                return false;

            if (choice == 1) return Login();
            if (choice == 2) return Register();

            return false;
        }

        static bool Register()
        {
            Console.Write("Email: ");
            string email = Console.ReadLine();

            if (UserExists(email))
            {
                Console.WriteLine("Email вже існує");
                return false;
            }
            Console.Write("Пароль: ");
            string password = Console.ReadLine();

            int id = GenerateId(userFile);
            string hash = Hash(password);

            File.AppendAllText(userFile, $"{id},{email},{hash}\n");
            Console.WriteLine("✔ Реєстрація успішна");
            return true;
        }

        static bool Login()
        {
            Console.Write("Email: ");
            string email = Console.ReadLine();
            Console.Write("Пароль: ");
            string password = Console.ReadLine();

            string hash = Hash(password);
            var lines = File.ReadAllLines(userFile);

            for (int i = 1; i < lines.Length; i++)
            {
                var p = lines[i].Split(',');
                if (p.Length == 3 && p[1] == email && p[2] == hash)
                {
                    Console.WriteLine("✔ Вхід виконано");
                    return true;
                }
            }

            Console.WriteLine("Невірний логін або пароль");
            return false;
        }

        static bool UserExists(string email)
        {
            var lines = File.ReadAllLines(userFile);
            for (int i = 1; i < lines.Length; i++)
            {
                var p = lines[i].Split(',');
                if (p.Length == 3 && p[1] == email)
                    return true;
            }
            return false;
        }

        // ===== ГОЛОВНЕ МЕНЮ =====
        static void MainMenu()
        {
            while (true)
            {
                Console.WriteLine("\n1-Додати квітку");
                Console.WriteLine("2-Показати всі");
                Console.WriteLine("3-Пошук");
                Console.WriteLine("4-Видалити");
                Console.WriteLine("5-Статистика");
                Console.WriteLine("0-Вихід");
                Console.Write("Вибір: ");

                if (!int.TryParse(Console.ReadLine(), out int c))
                    continue;

                if (c == 0) break;
                if (c == 1) AddFlower();
                if (c == 2) ShowFlowers();
                if (c == 3) SearchFlower();
                if (c == 4) DeleteFlower();
                if (c == 5) Statistics();
            }
        }

        static void AddFlower()
        {
            Console.Write("Назва: ");
            string name = Console.ReadLine();

            Console.Write("Категорія: ");
            string cat = Console.ReadLine();

            Console.Write("Ціна: ");
            double price = double.Parse(Console.ReadLine());

            Console.Write("Кількість: ");
            int qty = int.Parse(Console.ReadLine());

            int id = GenerateId(flowerFile);
            File.AppendAllText(flowerFile, $"{id},{name},{cat},{price},{qty}\n");
        }

        static void ShowFlowers()
        {
            Console.WriteLine("ID  Назва        Категорія     Ціна   К-сть");
            Console.WriteLine("------------------------------------------");

            var lines = File.ReadAllLines(flowerFile);
            for (int i = 1; i < lines.Length; i++)
            {
                var p = lines[i].Split(',');
                if (p.Length != 5) continue;

                Flower f = new Flower(
                    int.Parse(p[0]),
                    p[1],
                    p[2],
                    double.Parse(p[3]),
                    int.Parse(p[4])
                );
                f.Print();
            }
        }

        static void SearchFlower()
        {
            Console.Write("Назва: ");
            string s = Console.ReadLine();

            var lines = File.ReadAllLines(flowerFile);
            for (int i = 1; i < lines.Length; i++)
            {
                var p = lines[i].Split(',');
                if (p.Length == 5 && p[1].Equals(s, StringComparison.OrdinalIgnoreCase))
                    Console.WriteLine(lines[i]);
            }
        }

        static void DeleteFlower()
        {
            Console.Write("ID: ");
            int id = int.Parse(Console.ReadLine());

            var lines = new List<string>(File.ReadAllLines(flowerFile));
            for (int i = 1; i < lines.Count; i++)
            {
                var p = lines[i].Split(',');
                if (p.Length == 5 && int.Parse(p[0]) == id)
                {
                    lines.RemoveAt(i);
                    break;
                }
            }
            File.WriteAllLines(flowerFile, lines);
        }

        static void Statistics()
        {
            int count = 0;
            double sum = 0, min = double.MaxValue, max = 0;

            var lines = File.ReadAllLines(flowerFile);
            for (int i = 1; i < lines.Length; i++)
            {
                var p = lines[i].Split(',');
                double price = double.Parse(p[3]);
                int qty = int.Parse(p[4]);

                sum += price * qty;
                min = Math.Min(min, price);
                max = Math.Max(max, price);
                count++;
            }

            Console.WriteLine($"Кількість позицій: {count}");
            Console.WriteLine($"Загальна сума: {sum}");
            Console.WriteLine($"Мін. ціна: {min}");
            Console.WriteLine($"Макс. ціна: {max}");
            Console.WriteLine($"Середня: {sum / count}");
        }

        static int GenerateId(string path)
        {
            int max = 0;
            var lines = File.ReadAllLines(path);

            for (int i = 1; i < lines.Length; i++)
            {
                var p = lines[i].Split(',');
                if (int.TryParse(p[0], out int id) && id > max)
                    max = id;
            }
            return max + 1;
        }

        static string Hash(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToHexString(bytes);
            }
        }
    }
}

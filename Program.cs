using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

class BusServer
{
    private const string DbFile = "bus_station.db";

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        InitializeDatabase();

        int port = 8888;
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        TcpListener server = new TcpListener(localAddr, port);

        server.Start();
        Console.WriteLine($"[SERVER] Сервер запущено на порті {port}. База даних SQLite активна.");

        try
        {
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }
        finally
        {
            server.Stop();
        }
    }

    private static void InitializeDatabase()
    {
        using (var connection = new SqliteConnection($"Data Source={DbFile}"))
        {
            connection.Open();
            string sql = @"
                CREATE TABLE IF NOT EXISTS Routes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DepartureCity TEXT,
                    ArrivalCity TEXT,
                    DepartureTime TEXT
                );
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE,
                    Password TEXT,
                    Role TEXT
                );
                CREATE TABLE IF NOT EXISTS Bookings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT,
                    RouteInfo TEXT,
                    SeatNumber INTEGER
                );";
            using (var command = new SqliteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }

            // Додаємо адміна та юзера
            string checkUsers = "SELECT COUNT(*) FROM Users";
            using (var cmd = new SqliteCommand(checkUsers, connection))
            {
                long count = (long)(cmd.ExecuteScalar() ?? 0);
                if (count == 0)
                {
                    string seedUsers = "INSERT INTO Users (Username, Password, Role) VALUES ('admin', 'admin', 'Admin');" +
                                       "INSERT INTO Users (Username, Password, Role) VALUES ('user', '123', 'User');";
                    using (var insertCmd = new SqliteCommand(seedUsers, connection))
                        insertCmd.ExecuteNonQuery();
                }
            }

            // Генеруємо 5-7 рейсів на день
            string checkRoutes = "SELECT COUNT(*) FROM Routes";
            using (var cmd = new SqliteCommand(checkRoutes, connection))
            {
                long count = (long)(cmd.ExecuteScalar() ?? 0);
                if (count == 0)
                {
                    var cities = new[] {
                        ("Львів", "Київ"), ("Львів", "Івано-Франківськ"), ("Львів", "Тернопіль"),
                        ("Київ", "Одеса"), ("Харків", "Київ"), ("Львів", "Ужгород")
                    };
                    var times = new[] { "08:00", "10:30", "14:00", "17:15", "19:30", "22:00" };
                    var random = new Random();
                    DateTime startDate = DateTime.Today;

                    using (var transaction = connection.BeginTransaction())
                    {
                        for (int day = 0; day < 7; day++)
                        {
                            DateTime currentDay = startDate.AddDays(day);
                            int routesPerDay = random.Next(5, 8); // Від 5 до 7 рейсів

                            for (int i = 0; i < routesPerDay; i++)
                            {
                                var route = cities[random.Next(cities.Length)];
                                var time = times[random.Next(times.Length)];
                                string departureTime = $"{currentDay:yyyy-MM-dd} {time}";

                                string insertRoute = "INSERT INTO Routes (DepartureCity, ArrivalCity, DepartureTime) VALUES (@dep, @arr, @time);";
                                using (var insertCmd = new SqliteCommand(insertRoute, connection, transaction))
                                {
                                    insertCmd.Parameters.AddWithValue("@dep", route.Item1);
                                    insertCmd.Parameters.AddWithValue("@arr", route.Item2);
                                    insertCmd.Parameters.AddWithValue("@time", departureTime);
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
        }
        Console.WriteLine("[SERVER] База даних готова, маршрути згенеровано.");
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        Console.WriteLine($"\n[SERVER] --- Нове підключення клієнта ---");
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[2048];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine($"[SERVER] Отримано запит: {request}");

            string response = "UNKNOWN_COMMAND";

            if (request.StartsWith("LOGIN"))
            {
                var p = request.Split(';');
                if (p.Length >= 3) response = CheckLogin(p[1], p[2]);
            }
            else if (request.StartsWith("REGISTER"))
            {
                var p = request.Split(';');
                if (p.Length >= 3) response = RegisterUser(p[1], p[2]);
            }
            else if (request.StartsWith("GET_ROUTES"))
            {
                response = ReadRoutesFromDatabase();
                Console.WriteLine($"[SERVER] Відправлено список доступних маршрутів.");
            }
            else if (request.StartsWith("BOOK_SEAT"))
            {
                var p = request.Split(';');
                if (p.Length >= 4) response = BookSeatInDatabase(p[1], p[2], p[3]);
            }
            else if (request.StartsWith("GET_BOOKED_SEATS"))
            {
                string routeInfo = request.Substring("GET_BOOKED_SEATS;".Length);
                response = GetBookedSeats(routeInfo);
                Console.WriteLine($"[SERVER] Відправлено зайняті місця: {(response == "EMPTY" ? "немає" : response)}");
            }
            else if (request.StartsWith("ADD_ROUTE"))
            {
                var p = request.Split(';');
                if (p.Length >= 4) response = AddRouteToDatabase(p[1], p[2], p[3]);
            }
            else if (request.StartsWith("DELETE_ROUTE"))
            {
                var p = request.Split(';');
                if (p.Length >= 4) response = DeleteRouteFromDatabase(p[1], p[2], p[3]);
            }
            else if (request.StartsWith("DELETE_USER"))
            {
                var p = request.Split(';');
                if (p.Length >= 2) response = DeleteUserFromDatabase(p[1]);
            }

            // Виводимо в консоль відповідь сервера (крім довгого списку маршрутів)
            if (!request.StartsWith("GET_ROUTES") && !request.StartsWith("GET_BOOKED_SEATS"))
            {
                Console.WriteLine($"[SERVER] Результат/Відповідь: {response}");
            }

            byte[] responseData = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(responseData, 0, responseData.Length);
        }

        client.Close();
        Console.WriteLine("[SERVER] --- Клієнт відключився ---");
    }
    // (Тут залишаються ваші старі методи CheckLogin, RegisterUser, BookSeatInDatabase, ReadRoutesFromDatabase)
    // Додаю їх нижче для повноти коду:

    private static string CheckLogin(string username, string password)
    {
        using (var connection = new SqliteConnection($"Data Source={DbFile}"))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("SELECT Role FROM Users WHERE Username=@u AND Password=@p", connection))
            {
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);
                var role = cmd.ExecuteScalar()?.ToString();
                return role != null ? $"SUCCESS:{role}" : "ERROR: Невірний логін або пароль!";
            }
        }
    }

    private static string RegisterUser(string username, string password)
    {
        try
        {
            using (var connection = new SqliteConnection($"Data Source={DbFile}"))
            {
                connection.Open();
                using (var cmd = new SqliteCommand("INSERT INTO Users (Username, Password, Role) VALUES (@u, @p, 'User')", connection))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", password);
                    cmd.ExecuteNonQuery();
                }
            }
            return "SUCCESS";
        }
        catch { return "ERROR: Користувач існує!"; }
    }

    private static string ReadRoutesFromDatabase()
    {
        StringBuilder sb = new StringBuilder();
        using (var connection = new SqliteConnection($"Data Source={DbFile}"))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("SELECT DepartureCity, ArrivalCity, DepartureTime FROM Routes", connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    sb.Append($"{reader["DepartureCity"]}-{reader["ArrivalCity"]}-{reader["DepartureTime"]};");
            }
        }
        return sb.Length > 0 ? sb.ToString().TrimEnd(';') : "EMPTY";
    }

    private static string BookSeatInDatabase(string username, string route, string seat)
    {
        using (var connection = new SqliteConnection($"Data Source={DbFile}"))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("INSERT INTO Bookings (Username, RouteInfo, SeatNumber) VALUES (@u, @r, @s)", connection))
            {
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@r", route);
                cmd.Parameters.AddWithValue("@s", seat);
                cmd.ExecuteNonQuery();
            }
        }
        return "SUCCESS";
    }

    // НОВІ МЕТОДИ ДЛЯ АДМІНА ТА БЛОКУВАННЯ МІСЦЬ:
    private static string GetBookedSeats(string routeInfo)
    {
        List<string> booked = new List<string>();
        using (var connection = new SqliteConnection($"Data Source={DbFile}"))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("SELECT SeatNumber FROM Bookings WHERE RouteInfo=@r", connection))
            {
                cmd.Parameters.AddWithValue("@r", routeInfo);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) booked.Add(reader["SeatNumber"].ToString());
                }
            }
        }
        return booked.Count > 0 ? string.Join(",", booked) : "EMPTY";
    }

    private static string AddRouteToDatabase(string dep, string arr, string time)
    {
        using (var connection = new SqliteConnection($"Data Source={DbFile}"))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("INSERT INTO Routes (DepartureCity, ArrivalCity, DepartureTime) VALUES (@dep, @arr, @time)", connection))
            {
                cmd.Parameters.AddWithValue("@dep", dep);
                cmd.Parameters.AddWithValue("@arr", arr);
                cmd.Parameters.AddWithValue("@time", time);
                cmd.ExecuteNonQuery();
            }
        }
        return "SUCCESS";
    }

    private static string DeleteRouteFromDatabase(string dep, string arr, string time)
    {
        using (var connection = new SqliteConnection($"Data Source={DbFile}"))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("DELETE FROM Routes WHERE DepartureCity=@d AND ArrivalCity=@a AND DepartureTime=@t", connection))
            {
                cmd.Parameters.AddWithValue("@d", dep);
                cmd.Parameters.AddWithValue("@a", arr);
                cmd.Parameters.AddWithValue("@t", time);
                cmd.ExecuteNonQuery();
            }
        }
        return "SUCCESS";
    }

    private static string DeleteUserFromDatabase(string username)
    {
        using (var connection = new SqliteConnection($"Data Source={DbFile}"))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("DELETE FROM Users WHERE Username=@u", connection))
            {
                cmd.Parameters.AddWithValue("@u", username);
                cmd.ExecuteNonQuery();
            }
        }
        return "SUCCESS";
    }
}
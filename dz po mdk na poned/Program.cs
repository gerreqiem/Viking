using System;
using System.Data.SQLite;
using System.Threading;
using FitnessCenter_Viking;
namespace FitnessCenter_VikingApp
{
    class Viking
    {
        static void Main(string[] args)
        {
            IClientService clientService = new SQLiteClientServiceAdapter("Data Source=fitness_center.db;Version=3;");
            string connectionString = "Data Source=fitness_center.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS Clients (" +
                                          "Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                          "LastName TEXT NOT NULL," +
                                          "FirstName TEXT NOT NULL," +
                                          "MiddleName TEXT," +
                                          "DateOfBirth DATE NOT NULL," +
                                          "PhoneNumber TEXT UNIQUE," +
                                          "ClientStatus TEXT NOT NULL," +
                                          "SubscriptionExpiration DATE" +
                                          ")";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                while (true)
                {
                    Console.WriteLine("Выберите действие:");
                    Console.WriteLine("1. Просмотреть список клиентов");
                    Console.WriteLine("2. Добавить нового клиента");
                    Console.WriteLine("3. Редактировать информацию о клиенте");
                    Console.WriteLine("4. Удалить клиента(ов)");
                    Console.WriteLine("5. Добавить случайных клиентов");
                    Console.WriteLine("6. Выйти из программы");
                    string choice = Console.ReadLine();
                    switch (choice)
                    {
                        case "1":
                            ShowClients(connection);
                            break;
                        case "2":
                            AddClient(connection);
                            break;
                        case "3":
                            EditClient(connection);
                            break;
                        case "4":
                            Console.Write("\t1 - удаление всех клиентов\n \t2 - удаление клиента по идентификатору\n");
                            int doing = int.Parse(Console.ReadLine());
                            if (doing == 1)
                            {
                                DeleteAllClients(connection);
                                Thread.Sleep(1000);
                            }
                            else if (doing == 2)
                            {
                                Console.Write("Введите идентификатор клиента для удаления: ");
                                int deleteClientId = int.Parse(Console.ReadLine());
                                DeleteOneClient(connection, deleteClientId);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Пожалуйста введите правильное число");
                                Console.ResetColor();
                            }
                            break;
                        case "5":
                            Console.Write("Введите количество случайных клиентов, которых нужно добавить: ");
                            int kolvo = int.Parse(Console.ReadLine());
                            GenerateClients(connection, kolvo);
                            Thread.Sleep(1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Клиенты были успешно добавлены");
                            Console.ResetColor();
                            break;
                        case "6":
                            return;
                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Некорректный выбор. Попробуйте снова.");
                            Console.ResetColor();
                            break;
                    }

                    Console.Write("\n");
                    Thread.Sleep(1000);
                }
            }
        }
        static void ShowClients(SQLiteConnection connection)
        {
            string query = "SELECT * FROM Clients";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("Список клиентов:");
                    while (reader.Read())
                    {
                        DateTime subscriptionExpiration = Convert.ToDateTime(reader["SubscriptionExpiration"]);
                        string expirationStatus = (subscriptionExpiration < DateTime.Now) ? "истёк" : subscriptionExpiration.ToString("yyyy-MM-dd");
                        string middleName = reader["MiddleName"] != DBNull.Value ? reader["MiddleName"].ToString() : "Нет данных";
                        string phoneNumber = reader["PhoneNumber"] != DBNull.Value ? reader["PhoneNumber"].ToString() : "Нет данных";
                        Console.WriteLine($"Идентификатор: {reader["Id"]}, Фамилия: {reader["LastName"]}, Имя: {reader["FirstName"]}, Отчество: {middleName}, Дата рождения: {((DateTime)reader["DateOfBirth"]).ToString("yyyy-MM-dd")}, Номер телефона: {phoneNumber}, Статус: {reader["ClientStatus"]}, Дата окончания абонемента: {expirationStatus}");
                    }
                }
            }
        }
        static void AddClient(SQLiteConnection connection)
        {
            Console.WriteLine("Введите фамилию:");
            string lastName = Console.ReadLine();
            Console.WriteLine("Введите имя:");
            string firstName = Console.ReadLine();
            Console.WriteLine("Введите отчество:");
            string middleName = Console.ReadLine();
            Console.WriteLine("Введите дату рождения (в формате ГГГГ-ММ-ДД):");
            DateTime dateOfBirth = DateTime.Parse(Console.ReadLine());
            Console.WriteLine("Введите номер телефона:");
            string phoneNumber = Console.ReadLine();
            if (IsPhoneNumberExists(connection, phoneNumber))
            {
                Console.WriteLine($"Номер телефона {phoneNumber} уже принадлежит другому клиенту.");
                return;
            }
            Console.WriteLine("Введите статус клиента:");
            string clientStatus = Console.ReadLine();
            if (clientStatus != "клиент" && clientStatus != "бизнес-клиент" && clientStatus != "вип-клиент")
            {
                Console.WriteLine("Некорректный статус клиента. Допустимые значения: клиент, бизнес-клиент, вип-клиент.");
                return;
            }
            Console.WriteLine("Введите дату окончания абонемента (в формате ГГГГ-ММ-ДД):");
            DateTime subscriptionExpiration = DateTime.Parse(Console.ReadLine());
            string insertQuery = "INSERT INTO Clients (LastName, FirstName, MiddleName, DateOfBirth, PhoneNumber, ClientStatus, SubscriptionExpiration) VALUES (@LastName, @FirstName, @MiddleName, @DateOfBirth, @PhoneNumber, @ClientStatus, @SubscriptionExpiration)";
            using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@LastName", lastName);
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@MiddleName", middleName);
                command.Parameters.AddWithValue("@DateOfBirth", dateOfBirth);
                command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                command.Parameters.AddWithValue("@ClientStatus", clientStatus);
                command.Parameters.AddWithValue("@SubscriptionExpiration", subscriptionExpiration);
                command.ExecuteNonQuery();
                Console.WriteLine("Новый клиент успешно добавлен.");
            }
        }
        static bool IsPhoneNumberExists(SQLiteConnection connection, string phoneNumber)
        {
            string query = "SELECT COUNT(*) FROM Clients WHERE PhoneNumber = @PhoneNumber";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
        static void EditClient(SQLiteConnection connection)
        {
            Console.WriteLine("Введите идентификатор клиента для редактирования:");
            int clientId = int.Parse(Console.ReadLine());
            Console.WriteLine("Введите новую фамилию:");
            string lastName = Console.ReadLine();
            Console.WriteLine("Введите новое имя:");
            string firstName = Console.ReadLine();
            Console.WriteLine("Введите новое отчество:");
            string middleName = Console.ReadLine();
            Console.WriteLine("Введите новую дату рождения (в формате ГГГГ-ММ-ДД):");
            DateTime dateOfBirth = DateTime.Parse(Console.ReadLine());
            Console.WriteLine("Введите новый номер телефона:");
            string newPhoneNumber = Console.ReadLine();
            if (IsPhoneNumberExists(connection, newPhoneNumber))
            {
                Console.WriteLine($"Номер телефона {newPhoneNumber} уже принадлежит другому клиенту.");
                return;
            }
            Console.WriteLine("Введите новый статус клиента (клиент, бизнес-клиент, вип-клиент):");
            string clientStatus = Console.ReadLine();
            if (clientStatus != "клиент" && clientStatus != "бизнес-клиент" && clientStatus != "вип-клиент")
            {
                Console.WriteLine("Некорректный статус клиента. Допустимые значения: клиент, бизнес-клиент, вип-клиент.");
                return;
            }
            Console.WriteLine("Введите новую дату окончания абонемента (в формате ГГГГ-ММ-ДД):");
            DateTime subscriptionExpiration = DateTime.Parse(Console.ReadLine());
            string updateQuery = "UPDATE Clients SET LastName = @LastName, FirstName = @FirstName, MiddleName = @MiddleName, DateOfBirth = @DateOfBirth, PhoneNumber = @PhoneNumber, ClientStatus = @ClientStatus, SubscriptionExpiration = @SubscriptionExpiration WHERE Id = @ClientId";
            using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@ClientId", clientId);
                command.Parameters.AddWithValue("@LastName", lastName);
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@MiddleName", middleName);
                command.Parameters.AddWithValue("@DateOfBirth", dateOfBirth);
                command.Parameters.AddWithValue("@PhoneNumber", newPhoneNumber);
                command.Parameters.AddWithValue("@ClientStatus", clientStatus);
                command.Parameters.AddWithValue("@SubscriptionExpiration", subscriptionExpiration);
                command.ExecuteNonQuery();
                Console.WriteLine("Информация о клиенте успешно обновлена.");
            }
        }
        static void DeleteOneClient(SQLiteConnection connection, int clientId)
        {
            if (!IsClientExists(connection, clientId))
            {
                Console.WriteLine($"Клиент с идентификатором {clientId} не найден.");
                return;
            }
            string deleteQuery = "DELETE FROM Clients WHERE Id = @ClientId";
            using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@ClientId", clientId);
                command.ExecuteNonQuery();
                Console.WriteLine("Клиент успешно удален.");
                string resetSequenceQuery = "DELETE FROM sqlite_sequence WHERE name = 'Clients'";
                using (SQLiteCommand resetCommand = new SQLiteCommand(resetSequenceQuery, connection))
                {
                    resetCommand.ExecuteNonQuery();
                }
            }
        }
        static void DeleteAllClients(SQLiteConnection connection)
        {
            if (!AreClientsExist(connection))
            {
                Console.WriteLine("Нет клиентов для удаления.");
                return;
            }
            string deleteQuery = "DELETE FROM Clients";
            using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
            {
                command.ExecuteNonQuery();
                Console.WriteLine("Все клиенты успешно удалены.");
                string resetSequenceQuery = "DELETE FROM sqlite_sequence WHERE name = 'Clients'";
                using (SQLiteCommand resetCommand = new SQLiteCommand(resetSequenceQuery, connection))
                {
                    resetCommand.ExecuteNonQuery();
                }
            }
        }
        static bool IsClientExists(SQLiteConnection connection, int clientId)
        {
            string query = "SELECT COUNT(*) FROM Clients WHERE Id = @ClientId";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ClientId", clientId);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
        static bool AreClientsExist(SQLiteConnection connection)
        {
            string query = "SELECT COUNT(*) FROM Clients";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
        static DateTime GenerateRandomDateOfBirth()
        {
            Random rnd = new Random();
            DateTime start = new DateTime(1950, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(rnd.Next(range));
        }
        static string GenerateRandomPhoneNumber()
        {
            Random rnd = new Random();
            return "+7" + rnd.Next(100000000, 999999999).ToString();
        }
        static string GenerateRandomClientStatus()
        {
            Random rnd = new Random();
            string[] statuses = { "клиент", "бизнес-клиент", "вип-клиент" };
            return statuses[rnd.Next(statuses.Length)];
        }
        static void AddRandomClient(SQLiteConnection connection)
        {
            string[] firstNames = { "Иван", "Петр", "Александр", "Михаил", "Сергей", "Андрей", "Алексей", "Дмитрий", "Николай", "Евгений" };
            string[] lastNames = { "Иванов", "Петров", "Сидоров", "Кузнецов", "Смирнов", "Михайлов", "Федоров", "Соловьев", "Лебедев", "Павлов" };
            string firstName = firstNames[new Random().Next(firstNames.Length)];
            string lastName = lastNames[new Random().Next(lastNames.Length)];
            string middleName = firstNames[new Random().Next(firstNames.Length)] + "ович";
            DateTime dateOfBirth = GenerateRandomDateOfBirth();
            string phoneNumber = GenerateRandomPhoneNumber();
            string clientStatus = GenerateRandomClientStatus();
            DateTime subscriptionExpiration = DateTime.Now.AddDays(new Random().Next(30, 365));
            string insertQuery = "INSERT INTO Clients (LastName, FirstName, MiddleName, DateOfBirth, PhoneNumber, ClientStatus, SubscriptionExpiration) VALUES (@LastName, @FirstName, @MiddleName, @DateOfBirth, @PhoneNumber, @ClientStatus, @SubscriptionExpiration)";
            using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@LastName", lastName);
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@MiddleName", middleName);
                command.Parameters.AddWithValue("@DateOfBirth", dateOfBirth);
                command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                command.Parameters.AddWithValue("@ClientStatus", clientStatus);
                command.Parameters.AddWithValue("@SubscriptionExpiration", subscriptionExpiration);
                command.ExecuteNonQuery();
            }
        }
        static void GenerateClients(SQLiteConnection connection, int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddRandomClient(connection);
            }
        }
    }
}

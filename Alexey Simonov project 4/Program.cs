/*

Симонов Алексей Дмитриевич БПИ-248-2 5 вариант B-side

Управление в меню по стрелкам или по нажатию кнопок 1-9 на клавиатуре, enter - выбрать, esc - выйти

В целом я в консоли везде подписал на какие кнопки нажимать

Если таблица кажется сломанной, нужно сделать консоль шире

Общая информация о реализации:

Для хранения данных я использовал формат JSON, поэтому импорт из JSON это по сути пункт "изменить путь к файлу"

*/

// Объявление пространства имён для проекта, связанного с управлением библиотекой книг
namespace Alexey_Simonov_project_4
{
    /// <summary>
    /// Внутренний класс Program, содержащий точку входа в приложение для управления библиотекой книг.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Асинхронный метод Main, являющийся точкой входа в приложение.
        /// Обрабатывает запуск программы, настройку пути к файлу и взаимодействие с пользователем через меню.
        /// </summary>
        private static async Task Main()
        {
            string? path; // Переменная для хранения пути к файлу библиотеки, может быть null
            while (true)
            {
                // Выводим приглашение пользователю для ввода пути к файлу библиотеки
                Console.WriteLine("Введите путь к файлу библиотеки:");
                
                // Закомментированный пример пути для тестирования (оставлен для отладки)
                //path = "C:\\Users\\user\\Desktop\\проект 4.txt";
                path = Console.ReadLine() ?? string.Empty; // Читаем путь из консоли, присваиваем пустую строку, если ввод null

                // Если путь не содержит расширение, добавляем .txt автоматически
                if (!path.Contains("."))
                {
                    path += ".txt";
                }

                // Пытаемся обработать путь к файлу, создавая файл, если его нет, или загружая существующий
                try
                {
                    // Проверяем, существует ли файл по указанному пути
                    if (!File.Exists(path))
                    {
                        // Сообщаем пользователю, что файл не найден, и создаём новый
                        Console.WriteLine("Такого файла не было, был создан новый с этим именем");
                        Metods.Library = new LibraryManager(path, false); // Создаём новый менеджер библиотеки без загрузки
                        break; // Выход из цикла после успешного создания
                    }
                    // Если файл существует, пытаемся загрузить или создать его через вспомогательный метод
                    else if (Metods.TryLoadOrCreateFile(path))
                    {
                        Metods.Library = new LibraryManager(path); // Создаём менеджер библиотеки с загрузкой данных
                        break; // Выход из цикла после успешной загрузки
                    }
                }
                catch (Exception ex)
                {
                    // Обрабатываем любые исключения при работе с файлом и просим пользователя повторить ввод
                    Console.WriteLine($"Ошибка: {ex.Message}. Попробуйте снова.");
                }
            }
            
            // Приглашаем пользователя нажать любую клавишу для продолжения работы с меню
            Console.WriteLine("Нажмите любую кнопку для начала");
            Console.ReadKey(); // Ждём нажатия клавиши

            int selectedIndex = 0; // Инициализируем индекс выбранного пункта меню (0–9)

            // Главный цикл меню, продолжающийся до выхода пользователя
            while (true)
            {
                // Очищаем консоль перед выводом нового состояния меню
                Console.Clear();
                // Выводим информацию о текущем файле и количестве книг в библиотеке
                Console.WriteLine($"Файл открыт: {Metods.LibraryPath}");
                Console.WriteLine($"Книг в библиотеке: {Metods.LibCount}");
                Console.WriteLine("\nМеню:");

                // Создаём массив пунктов меню для отображения
                string[] menuItems = new[]
                {
                    "1. Просмотреть книги", "2. Добавить книгу вручную", "3. Добавить книгу по ISBN (OpenLibrary)",
                    "4. Редактировать книгу", "5. Удалить книгу", "6. Показать рекомендации", "7. Импорт из CSV",
                    "8. Экспорт в JSON/CSV", "9. Изменить путь к файлу (JSON или TXT)",
                };

                // Цикл для вывода каждого пункта меню с визуальным выделением выбранного
                for (int i = 0; i < menuItems.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        // Выделяем выбранный пункт: добавляем отступы и белый цвет для акцента
                        if (i > 0)
                        {
                            Console.WriteLine(); // Добавляем отступ сверху, кроме первого пункта
                        }

                        Console.ForegroundColor = ConsoleColor.White; // Устанавливаем белый цвет для выделения
                        Console.WriteLine($"  =>  {menuItems[i]}  <=  "); // Форматируем с большими стрелками для выделения
                        Console.WriteLine(); // Добавляем отступ снизу, кроме последнего пункта
                        Console.ResetColor(); // Возвращаем стандартный цвет текста
                    }
                    else
                    {
                        // Не выбранные пункты отображаем в тусклом цвете для контраста
                        Console.ForegroundColor = ConsoleColor.DarkGray; // Устанавливаем тусклый серый цвет
                        Console.WriteLine($"     {menuItems[i]}");
                        Console.ResetColor(); // Возвращаем стандартный цвет текста
                    }
                }

                // Подсказка пользователю о управлении меню
                Console.Write("\nИспользуйте стрелки или 1-9 для навигации, Enter для выбора, Esc для выхода: ");

                // Читаем нажатие клавиши без отображения её в консоли
                ConsoleKeyInfo key = Console.ReadKey(true); // Читаем клавишу без вывода
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = Math.Max(0, selectedIndex - 1); // Перемещаемся вверх, не допуская значения меньше 0
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex =
                            Math.Min(menuItems.Length - 1,
                                selectedIndex + 1); // Перемещаемся вниз, не выходя за пределы 8 (длина массива - 1)
                        break;
                    case ConsoleKey.D1: // Обработка нажатия клавиши '1' или NumPad1
                    case ConsoleKey.NumPad1:
                        selectedIndex = 0; // Выделяем первый пункт меню
                        break;
                    case ConsoleKey.D2: // Обработка нажатия клавиши '2' или NumPad2
                    case ConsoleKey.NumPad2:
                        selectedIndex = 1; // Выделяем второй пункт меню
                        break;
                    case ConsoleKey.D3: // Обработка нажатия клавиши '3' или NumPad3
                    case ConsoleKey.NumPad3:
                        selectedIndex = 2; // Выделяем третий пункт меню
                        break;
                    case ConsoleKey.D4: // Обработка нажатия клавиши '4' или NumPad4
                    case ConsoleKey.NumPad4:
                        selectedIndex = 3; // Выделяем четвёртый пункт меню
                        break;
                    case ConsoleKey.D5: // Обработка нажатия клавиши '5' или NumPad5
                    case ConsoleKey.NumPad5:
                        selectedIndex = 4; // Выделяем пятый пункт меню
                        break;
                    case ConsoleKey.D6: // Обработка нажатия клавиши '6' или NumPad6
                    case ConsoleKey.NumPad6:
                        selectedIndex = 5; // Выделяем шестой пункт меню
                        break;
                    case ConsoleKey.D7: // Обработка нажатия клавиши '7' или NumPad7
                    case ConsoleKey.NumPad7:
                        selectedIndex = 6; // Выделяем седьмой пункт меню
                        break;
                    case ConsoleKey.D8: // Обработка нажатия клавиши '8' или NumPad8
                    case ConsoleKey.NumPad8:
                        selectedIndex = 7; // Выделяем восьмой пункт меню
                        break;
                    case ConsoleKey.D9: // Обработка нажатия клавиши '9' или NumPad9
                    case ConsoleKey.NumPad9:
                        selectedIndex = 8; // Выделяем девятый пункт меню
                        break;
                    case ConsoleKey.Enter:
                        // Обрабатываем выбор пункта меню, добавляя 1 к индексу, так как меню начинается с 1
                        switch (selectedIndex + 1) // Сдвигаем индекс на 1, так как меню начинается с 1
                        {
                            case 1:
                                Console.Clear(); // Очищаем консоль перед отображением
                                Metods.Library.DisplayBooks(Metods.DisplayBooksWithCoversInPanels,
                                    "\n=== Список книг в библиотеке ==="); // Вызываем метод для отображения списка книг с обложками
                                Console.Clear(); // Очищаем консоль после отображения
                                break;
                            case 2:
                                Metods.AddBook(); // Вызываем метод для добавления книги вручную
                                break;
                            case 3:
                                string? isbn; // Переменная для хранения ISBN, может быть null
                                Console.Clear(); // Очищаем консоль перед вводом
                                Console.WriteLine("Введите пустую строку для выхода"); // Подсказка пользователю
                                Console.Write("Введите корректный IBSN: "); // Запрашиваем ввод ISBN
                                isbn = Console.ReadLine(); // Читаем введённый ISBN
                                
                                // Цикл для проверки валидности ISBN
                                while (!Metods.IsValidIsbn(isbn))
                                {
                                    Console.Clear(); // Очищаем консоль для повторного ввода
                                    Console.WriteLine("Введите пустую строку для выхода"); // Подсказка пользователю
                                    Console.ForegroundColor = ConsoleColor.Red; // Устанавливаем красный цвет для ошибки
                                    Console.Write("Введите корректный IBSN: "); // Запрашиваем повторный ввод с выделением ошибки
                                    Console.ResetColor(); // Возвращаем стандартный цвет
                                    isbn = Console.ReadLine(); // Читаем новый ввод
                                    if (isbn == "") // Проверяем, пустая ли строка для выхода
                                    {
                                        break;
                                    }
                                }

                                // Если ISBN валиден, добавляем книгу через OpenLibrary
                                if (Metods.IsValidIsbn(isbn))
                                {
                                    await Metods.Library.AddBookFromOpenLibrary(isbn); // Асинхронно добавляем книгу по ISBN
                                }

                                Console.WriteLine("Нажмите любую кнопку для выхода"); // Подсказка пользователю
                                Console.ReadKey(); // Ждём нажатия клавиши для возврата
                                break;
                            case 4:
                                Metods.EditBook(); // Вызываем метод для редактирования книги
                                break;
                            case 5:
                                Metods.DeleteBook(); // Вызываем метод для удаления книги
                                break;
                            case 6:
                                Metods.ShowRecs(); // Вызываем метод для отображения рекомендаций
                                break;
                            case 7:
                                Metods.ImportFromCsv(); // Вызываем метод для импорта из CSV
                                break;
                            case 8:
                                Console.Clear(); // Очищаем консоль перед выбором формата экспорта
                                int exportPointer = 0; // Индекс выбранного формата экспорта (0 для JSON, 1 для CSV)
                                while (true)
                                {
                                    Console.Clear(); // Очищаем консоль для перерисовки меню экспорта
                                    Console.WriteLine("Выберите, куда экспортировать:"); // Заголовок меню экспорта
                                    Console.WriteLine("В JSON" + (exportPointer == 0 ? "  =>" : "")); // Отображаем JSON с выделением, если выбрано
                                    Console.WriteLine("В CSV" + (exportPointer == 1 ? "  =>" : "")); // Отображаем CSV с выделением, если выбрано
                                    ConsoleKeyInfo exportKey = Console.ReadKey(true); // Читаем клавишу без отображения
                                    if (exportKey.Key == ConsoleKey.DownArrow)
                                    {
                                        exportPointer = (exportPointer + 1) % 2; // Перемещаемся вниз по вариантам (0 или 1)
                                    }

                                    if (exportKey.Key == ConsoleKey.UpArrow)
                                    {
                                        exportPointer = (exportPointer - 1 + 2) % 2; // Перемещаемся вверх по вариантам (0 или 1)
                                    }

                                    if (exportKey.Key == ConsoleKey.Enter)
                                    {
                                        break; // Выход из цикла при выборе формата
                                    }
                                }

                                // Выполняем экспорт в выбранный формат
                                if (exportPointer == 0)
                                {
                                    Metods.ExportToJson(); // Экспортируем в JSON
                                }

                                if (exportPointer == 1)
                                {
                                    Metods.ExportToCsv(); // Экспортируем в CSV
                                }

                                break;
                            case 9:
                                Metods.ChangeFilePath(); // Вызываем метод для изменения пути к файлу
                                break;
                        }

                        break; // Выход из обработки выбора пункта меню
                    case ConsoleKey.Escape:
                        Metods.Library.SaveBooks(); // Сохраняем все изменения в файле перед выходом
                        Console.WriteLine("Изменения сохранены. До свидания!"); // Сообщаем пользователю о сохранении
                        return; // Завершаем программу по нажатию Esc
                }
            }
        }

    }
}
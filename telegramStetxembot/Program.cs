using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private const string BotToken = "TOKEN";

    private static readonly Random Rnd = new Random();

    private static readonly string[] Quotes = new string[]
    {
        "Лучше быть последним — первым, чем первым — последним. (с) Джейсон Стетхем",
        "Если волк молчит, лучше его не перебивай. (с) Джейсон Стетхем",
        "Работа не волк. Волк — это ходить. Работа — это ворк. (с) Джейсон Стетхем",
        "Не важно, кто напротив. Важно, кто брат. (с) Джейсон Стетхем",
        "Одна ошибка — и ты ошибся. (с) Джейсон Стетхем"
    };

    private static readonly string[] Photos = new string[]
    {
        "https://foni.papik.pro/uploads/posts/2024-10/foni-papik-pro-whyb-p-kartinki-stetkhem-na-prozrachnom-fone-2.png",
    };

    private static readonly ReplyKeyboardMarkup MainKeyboard = new ReplyKeyboardMarkup(new KeyboardButton[][]
    {
        new KeyboardButton[]
        {
            new KeyboardButton("Выдать базу"),
            new KeyboardButton("Мудрость дня")
        }
    })
    {
        ResizeKeyboard = true
    };

    static async Task Main(string[] args)
    {
        var botClient = new TelegramBotClient(BotToken);
        using var cts = new CancellationTokenSource();

        try
        {
            var me = await botClient.GetMe(cts.Token);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[OK] Бот @{me.Username} успешно запущен!");
            Console.ResetColor();
            Console.WriteLine("Нажмите Enter в этом окне, чтобы остановить бота.\n");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message }
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cts.Token
            );

            Console.ReadLine();
            cts.Cancel();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ОШИБКА ПРИ ЗАПУСКЕ]: " + ex.Message);
            Console.ResetColor();
            Console.WriteLine("\nНажмите Enter для выхода...");
            Console.ReadLine();
        }
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message == null || string.IsNullOrEmpty(update.Message.Text))
            return;

        long chatId = update.Message.Chat.Id;
        string text = update.Message.Text;
        string userName = update.Message.From != null ? update.Message.From.FirstName : "Пользователь";

        Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + userName + ": " + text);

        switch (text)
        {
            case "/start":
                await bot.SendMessage(
                    chatId: chatId,
                    text: "Здорово, " + userName + ". Жми на кнопки снизу и впитывай мудрость.",
                    replyMarkup: MainKeyboard,
                    cancellationToken: cancellationToken);
                break;

            case "Выдать базу":
                string randomQuote = Quotes[Rnd.Next(Quotes.Length)];
                string randomPhoto = Photos[Rnd.Next(Photos.Length)];

                await bot.SendPhoto(
                    chatId: chatId,
                    photo: InputFile.FromUri(randomPhoto),
                    caption: randomQuote,
                    cancellationToken: cancellationToken);
                break;

            case "Мудрость дня":
                string dayQuote = Quotes[DateTime.Now.DayOfYear % Quotes.Length];

                await bot.SendMessage(
                    chatId: chatId,
                    text: "Мудрость на сегодня:\n\n" + dayQuote,
                    replyMarkup: MainKeyboard,
                    cancellationToken: cancellationToken);
                break;

            default:
                await bot.SendMessage(
                    chatId: chatId,
                    text: "Не понял тебя. Выбирай вариант на кнопках ниже.",
                    replyMarkup: MainKeyboard,
                    cancellationToken: cancellationToken);
                break;
        }
    }

    private static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        string errorMessage = exception is ApiRequestException apiEx
            ? "[Telegram API Error]: " + apiEx.ErrorCode + " — " + apiEx.Message
            : exception.ToString();

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(errorMessage);
        Console.ResetColor();

        return Task.CompletedTask;
    }
}
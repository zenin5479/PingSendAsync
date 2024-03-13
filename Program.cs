using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace PingSendAsync
{
   public class PingExample
   {
      public static void Main(string[] args)
      {
         // Требуется хост или IP-адрес
         string who = "www.google.ru";
         // Представляет событие синхронизации потока,
         // которое при получении сигнала освобождает один поток ожидания, и событие сбрасывается автоматически.
         // Если поток не ожидается, следующий поток, которому задано состояние ожидания,
         // немедленно освобождается, и событие сбрасывается автоматически.
         AutoResetEvent waiter = new AutoResetEvent(false);
         Ping pingSender = new Ping();
         // При возникновении события PingCompleted вызывается метод PingCompletedCallback
         pingSender.PingCompleted += PingCompletedCallback;
         // Буфер из 32 байт данных для передачи
         string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
         byte[] buffer = Encoding.ASCII.GetBytes(data);
         // Ожидание ответа 12 секунд
         int timeout = 12000;
         // Параметры для передачи:
         // ttl Значение типа Int32 больше нуля, указывающее, сколько раз могут быть переадресованы пакеты данных Ping.
         // dontFragment Boolean Значение true, чтобы данные, отправляемые на удаленный узел, не фрагментировались; в противном случае — false.
         PingOptions options = new PingOptions(64, true);
         Console.WriteLine("Переадресовывать пакеты данных: {0}", options.Ttl);
         Console.WriteLine("Не фрагментировать данные: {0}", options.DontFragment);
         // Отправляем ping асинхронно.
         // Используем waiter в качестве токена пользователя.
         // Когда обратный вызов завершится, он может повторитьь этот поток.
         pingSender.SendAsync(who, timeout, buffer, options, waiter);
         // Предотвращаем завершение работы приложения.
         // Блокирует текущий поток до получения сигнала объектом WaitHandle.
         waiter.WaitOne();
         Console.WriteLine("Проверка связи завершена");
      }

      private static void PingCompletedCallback(object sender, PingCompletedEventArgs e)
      {
         // Если операция была отменена, отображается сообщение пользователю
         if (e.Cancelled)
         {
            Console.WriteLine("Пинг отменен");
            // Основной поток возобновиться
            // Токен пользователя - это объект AutoResetEvent, который ожидает основной поток
            ((AutoResetEvent)e.UserState).Set();
         }

         // Если произошла ошибка, отображается исключение для пользователя
         if (e.Error != null)
         {
            Console.WriteLine("Не удалось выполнить пинг:");
            Console.WriteLine(e.Error.ToString());
            // Основной поток возобновиться
            ((AutoResetEvent)e.UserState).Set();
         }

         PingReply reply = e.Reply;
         DisplayReply(reply);
         // Основной поток возобновиться
         ((AutoResetEvent)e.UserState).Set();
      }

      public static void DisplayReply(PingReply reply)
      {
         if (reply == null)
            return;

         Console.WriteLine("Статус пинга: {0}", reply.Status);
         if (reply.Status == IPStatus.Success)
         {
            Console.WriteLine("Адрес: {0}", reply.Address);
            // Возвращает число миллисекунд, затраченное на отправку запроса проверки связи по протоколу ICMP
            // и получение соответствующего сообщения с ответом проверки связи по протоколу ICMP.
            Console.WriteLine("Время проверки связи: {0} {1}", reply.RoundtripTime, "миллисекунд");
            Console.WriteLine("Пакеты данных переадресованы: {0}", reply.Options.Ttl);
            Console.WriteLine("Данные не фрагментированы: {0}", reply.Options.DontFragment);
            Console.WriteLine("Размер буфера: {0}", reply.Buffer.Length);
         }
      }
   }
}
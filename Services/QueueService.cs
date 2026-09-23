using Microsoft.EntityFrameworkCore;
using CustomHome.Data;
using CustomHome.Models;

namespace CustomHome.Services
{
    public class QueueService
    {
        private readonly ServiceStationContext _context;

        public QueueService(ServiceStationContext context)
        {
            _context = context;
        }

        public void GetToken()
        {
            ExecuteWithQueueLock(() =>
            {
                var settings = _context.QueueSettings.First(); // though not using FOR UPDATE here, part of same transaction locked by FOR UPDATE queary in ExecuteWithQueueLock

                var waitingCount = _context.ServiceTokens
                    .Count(t => t.Status == "Waiting");

                if (waitingCount < settings.MaxWaiting)
                {
                    var token = new ServiceToken
                    {
                        TokenNumber = Random.Shared.Next(100000, 999999),
                        Status = "Waiting",
                        CreatedAt = DateTime.Now
                    };

                    _context.ServiceTokens.Add(token);
                    _context.SaveChanges();
                }
            });
        }

        public void ServeNext()
        {
            ExecuteWithQueueLock(() =>
            {
                var settings = _context.QueueSettings.First(); // though not using FOR UPDATE here, part of same transaction locked by FOR UPDATE queary in ExecuteWithQueueLock

                var servingCount = _context.ServiceTokens
                    .Count(t => t.Status == "Serving");

                if (servingCount < settings.MaxServing)
                {
                    var token = _context.ServiceTokens
                        .Where(t => t.Status == "Waiting")
                        .OrderBy(t => t.CreatedAt)
                        .FirstOrDefault();

                    if (token != null)
                    {
                        token.Status = "Serving";
                        _context.SaveChanges();
                    }
                }
            });
        }

        public void Complete(int id)
        {
            ExecuteWithQueueLock(() =>
            {
                var token = _context.ServiceTokens
                    .FirstOrDefault(t =>
                        t.Id == id &&
                        t.Status == "Serving");

                if (token != null)
                {
                    token.Status = "Completed";
                    _context.SaveChanges();
                }
            });
        }

        public List<ServiceToken> GetWaitingTokens() // reading operation, no need for transaction and locking
        {
            return _context.ServiceTokens
                .Where(t => t.Status == "Waiting")
                .OrderBy(t => t.CreatedAt)
                .ToList();
        }

        public List<ServiceToken> GetServingTokens() // reading operation, no need for transaction and locking
        {
            return _context.ServiceTokens
                .Where(t => t.Status == "Serving")
                .OrderBy(t => t.CreatedAt)
                .ToList();
        }

        private void ExecuteWithQueueLock(Action action)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                _context.QueueSettings
                    .FromSqlRaw(
                        "SELECT * FROM QueueSettings WHERE Id = 1 FOR UPDATE") // used for locking
                    .First();

                action();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
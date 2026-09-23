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

        public QueueOperationResponse GetToken()
        {
            var response = new QueueOperationResponse
            {
                Result = QueueOperationResult.QueueFull
            };

            ExecuteWithQueueLock(settings =>
            {
                var waitingCount = _context.ServiceTokens
                    .Count(t => t.Status == ServiceTokenStatus.Waiting);

                if (waitingCount < settings.MaxWaiting)
                {
                    var token = new ServiceToken
                    {
                        TokenNumber = GenerateUniqueTokenNumber(),
                        Status = ServiceTokenStatus.Waiting,
                        CreatedAt = DateTime.Now
                    };

                    _context.ServiceTokens.Add(token);
                    _context.SaveChanges();

                    response.Result = QueueOperationResult.Success;
                    response.TokenNumber = token.TokenNumber;
                }
            });

            return response;;
        }

        public QueueOperationResult ServeNext()
        {
            var result = QueueOperationResult.NoWaitingCustomer;

            ExecuteWithQueueLock(settings =>
            {
                var servingCount = _context.ServiceTokens
                    .Count(t => t.Status == ServiceTokenStatus.Serving);

                if (servingCount >= settings.MaxServing)
                {
                    result = QueueOperationResult.ServingCapacityFull;
                    return;
                }

                var token = _context.ServiceTokens
                    .Where(t => t.Status == ServiceTokenStatus.Waiting)
                    .OrderBy(t => t.CreatedAt)
                    .FirstOrDefault();

                if (token != null)
                {
                    token.Status = ServiceTokenStatus.Serving;
                    _context.SaveChanges();

                    result = QueueOperationResult.Success;
                }
            });

            return result;
        }

        public QueueOperationResult Complete(int id)
        {
            var result = QueueOperationResult.TokenNotFound;

            ExecuteWithQueueLock(settings =>    // settings only used for locking here
            {
                var token = _context.ServiceTokens
                    .FirstOrDefault(t =>
                        t.Id == id &&
                        t.Status == ServiceTokenStatus.Serving);

                if (token != null)
                {
                    token.Status = ServiceTokenStatus.Completed;
                    _context.SaveChanges();

                    result = QueueOperationResult.Success;
                }
            });

            return result;
        }

        public List<ServiceToken> GetWaitingTokens() // reading operation, no need for transaction and locking
        {
            return _context.ServiceTokens
                .Where(t => t.Status == ServiceTokenStatus.Waiting)
                .OrderBy(t => t.CreatedAt)
                .ToList();
        }

        public List<ServiceToken> GetServingTokens() // reading operation, no need for transaction and locking
        {
            return _context.ServiceTokens
                .Where(t => t.Status == ServiceTokenStatus.Serving)
                .OrderBy(t => t.CreatedAt)
                .ToList();
        }

        private int GenerateUniqueTokenNumber() // generally not needed as 100000 - 999999 is a large range
        {
            int tokenNumber;

            do
            {
                tokenNumber = Random.Shared.Next(100000, 999999);
            }
            while (_context.ServiceTokens
                .Any(t => t.TokenNumber == tokenNumber));

            return tokenNumber;
        }

        private void ExecuteWithQueueLock(Action<QueueSettings> action)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var settings = _context.QueueSettings
                    .FromSqlRaw(
                        "SELECT * FROM QueueSettings WHERE Id = 1 FOR UPDATE") // used for locking
                    .First();

                action(settings);

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
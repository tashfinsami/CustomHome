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
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var settings = _context.QueueSettings
                    .FromSqlRaw(
                        "SELECT * FROM QueueSettings WHERE Id = 1 FOR UPDATE")
                    .First();

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

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void ServeNext()
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var settings = _context.QueueSettings
                    .FromSqlRaw(
                        "SELECT * FROM QueueSettings WHERE Id = 1 FOR UPDATE")
                    .First();

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

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Complete(int id)
        {
            using var transaction = _context.Database.BeginTransaction(); // not absolutely necessary here, but added for consistency

            try
            {
                var settings = _context.QueueSettings
                    .FromSqlRaw(
                        "SELECT * FROM QueueSettings WHERE Id = 1 FOR UPDATE") // used only to lock the row for concurrency control
                    .First();

                var token = _context.ServiceTokens
                    .FirstOrDefault(t =>
                        t.Id == id &&
                        t.Status == "Serving");

                if (token != null)
                {
                    token.Status = "Completed";
                    _context.SaveChanges();
                }

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
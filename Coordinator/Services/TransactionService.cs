using Coordinator.Models;
using Coordinator.Models.Contexts;
using Coordinator.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Coordinator.Services
{
    public class TransactionService : ITransactionService
    {

        TwoPhaseCommitContext _context;
        IHttpClientFactory _httpClientFactory;


        public TransactionService(TwoPhaseCommitContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;

        }
        public async Task<Guid> CreateTransactionAsync()
        {
            Guid transactionId = Guid.NewGuid();

            var nodes = await _context.Nodes.ToListAsync();
            nodes.ForEach(node => node.NodeStates = new List<NodeState>()
            {
                new(transactionId)
                {
                    IsReady = Enums.ReadyType.Pending,
                    TransactionState = Enums.TransactionState.Pending
                }
            });

            _context.SaveChanges();
            return transactionId;
        }

        public async Task PrepareServicesAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates.Include(ns => ns.Node)
                  .Where(ns => ns.TransactionId == transactionId).ToListAsync();

            foreach (var node in transactionNodes)
            {
                try
                {
                    var response = await (node.Node.Name switch
                    {
                        "Order.API" => _httpClientFactory.CreateClient("Order.API").GetAsync("ready"),
                        "Stock.API" => _httpClientFactory.CreateClient("Stock.API").GetAsync("ready"),
                        "Payment.API" => _httpClientFactory.CreateClient("Payment.API").GetAsync("ready")
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    

                    node.IsReady = result ? Enums.ReadyType.Ready : Enums.ReadyType.Unready;
                }
                catch (Exception ex)
                {
                    node.IsReady = Enums.ReadyType.Unready;
                }
            }
            await _context.SaveChangesAsync();
        }
        public async Task<bool> CheckReadyServicesAsync(Guid transactionId)
        {
            return (await _context.NodeStates.Where(ns => ns.TransactionId == transactionId)
                .ToListAsync()).TrueForAll(ns => ns.IsReady == Enums.ReadyType.Ready);
        }

        public async Task CommitAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
                .Where(ns => ns.TransactionId == transactionId)
                .Include(ns => ns.Node)
                .ToListAsync();

            foreach (var node in transactionNodes)
            {
                try
                {
                    var response = await (node.Node.Name switch
                    {
                        "Order.API" => _httpClientFactory.CreateClient("Order.API").GetAsync("commit"),
                        "Stock.API" => _httpClientFactory.CreateClient("Stock.API").GetAsync("commit"),
                        "Payment.API" => _httpClientFactory.CreateClient("Payment.API").GetAsync("commit")
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    

                    node.TransactionState = result ? Enums.TransactionState.Done : Enums.TransactionState.Abort;
                }
                catch
                {
                    node.TransactionState = Enums.TransactionState.Abort;
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckTransactionStateServicesAsync(Guid transactionId)
        {
            return (await _context.NodeStates.Where(ns => ns.TransactionId == transactionId).ToListAsync()).TrueForAll(ns => ns.TransactionState == Enums.TransactionState.Done);
        }

        public async Task RollbackAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates.Where(ns => ns.TransactionId == transactionId).Include(ns => ns.Node).ToListAsync();
            foreach (var node in transactionNodes)
            {
                try
                {
                    if (node.TransactionState == Enums.TransactionState.Done)
                        _ = await (node.Node.Name switch
                        {
                            "Order.API" => _httpClientFactory.CreateClient("Order.API").GetAsync("rollback"),
                            "Stock.API" => _httpClientFactory.CreateClient("Stock.API").GetAsync("rollback"),
                            "Payment.API" => _httpClientFactory.CreateClient("Payment.API").GetAsync("rollback")
                        });
                    node.TransactionState = Enums.TransactionState.Abort;
                    
                }
                catch
                {
                    node.TransactionState = Enums.TransactionState.Abort;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

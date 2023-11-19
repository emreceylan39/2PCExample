using Coordinator.Models.Contexts;
using Coordinator.Services;
using Coordinator.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TwoPhaseCommitContext>();

builder.Services.AddHttpClient("Order.API", client => client.BaseAddress = new Uri("https://localhost:7043"));
builder.Services.AddHttpClient("Stock.API", client => client.BaseAddress = new Uri("https://localhost:7275"));
builder.Services.AddHttpClient("Payment.API", client => client.BaseAddress = new Uri("https://localhost:7001"));


builder.Services.AddTransient<ITransactionService,TransactionService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/create-order-transaction", async (ITransactionService transactionService) =>
{
    //Phase 1 Prepare
    var transactionId = await transactionService.CreateTransactionAsync();

    await transactionService.PrepareServicesAsync(transactionId);

    bool transactionState = await transactionService.CheckReadyServicesAsync(transactionId);

    if (transactionState)
    {
        await transactionService.CommitAsync(transactionId);
        transactionState = await transactionService.CheckTransactionStateServicesAsync(transactionId);
    }

    if(!transactionState)
    {
        await transactionService.RollbackAsync(transactionId);
    }
});

app.Run();

using Folha.Domain.Enums;

namespace Folha.Api.DTOs;

public record CreateTransactionRequest(string Description, decimal Amount, TransactionType Type, DateTime Date, Guid CategoryId);

public record TransactionResponse(Guid Id, string Description, decimal Amount, TransactionType Type, DateTime Date, Guid CategoryId, string CategoryName, string CategoryColor);

public record SummaryResponse(decimal TotalIncome, decimal TotalExpense, decimal Balance, int Count);

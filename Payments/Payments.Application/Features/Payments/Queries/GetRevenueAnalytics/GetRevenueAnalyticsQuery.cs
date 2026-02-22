using MediatR;
using Payments.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Payments.Application.Features.Payments.Queries.GetRevenueAnalytics;

public record RevenueAnalyticsDto(decimal CurrentMonthRevenue, List<PaymentChartDataDto> ChartData);
public record PaymentChartDataDto(string Date, decimal Amount);

public record GetRevenueAnalyticsQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<RevenueAnalyticsDto>;

public class GetRevenueAnalyticsQueryHandler : IRequestHandler<GetRevenueAnalyticsQuery, RevenueAnalyticsDto>
{
    private readonly IPaymentTransactionRepository _transactionRepository;

    public GetRevenueAnalyticsQueryHandler(IPaymentTransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<RevenueAnalyticsDto> Handle(GetRevenueAnalyticsQuery request, CancellationToken cancellationToken)
    {
        // Support filtering by exact Date or defaulting to last 30 days
        var start = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var end = request.EndDate ?? DateTime.UtcNow;

        var transactionsList = await _transactionRepository.ListAsync(t => t.CreatedAt >= start && t.CreatedAt <= end);
        var transactions = transactionsList.AsQueryable();

        // Chart Data grouped by Date
        var groupedChartData = transactions
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new PaymentChartDataDto(g.Key.ToString("MMM dd"), g.Sum(t => t.Amount)))
            .OrderBy(d => d.Date)
            .ToList();

        // Current Month Revenue (ignoring the custom filter for the overall metric card)
        var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var currentMonthTransactions = await _transactionRepository.ListAsync(t => t.CreatedAt >= currentMonthStart);
        var currentMonthRevenue = currentMonthTransactions.Sum(t => t.Amount);
            
        // Mock if empty (for demo purposes if needed depending on seeds)
        // (Removed mock fallbacks)

        return new RevenueAnalyticsDto(currentMonthRevenue, groupedChartData);
    }
}

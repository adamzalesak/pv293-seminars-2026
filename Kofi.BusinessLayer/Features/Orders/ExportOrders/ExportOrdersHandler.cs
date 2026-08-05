using System.Text;
using MediatR;
using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Features.Orders.GetOrders;

namespace Kofi.BusinessLayer.Features.Orders.ExportOrders;

public class ExportOrdersHandler(IMediator mediator) : IQueryHandler<ExportOrdersQuery, string>
{
    public async Task<string> Handle(ExportOrdersQuery query, CancellationToken ct)
    {
        var list = await mediator.Send(
            new GetOrdersQuery { From = query.From, SortBy = "date" }, ct);

        var csv = new StringBuilder();
        csv.AppendLine("Id;Customer;Status;CreatedAt;Total");
        foreach (var order in list)
        {
            csv.AppendLine($"{order.Id};{order.CustomerName};{order.Status};{order.CreatedAt};{order.Total}");
        }
        return csv.ToString();
    }
}

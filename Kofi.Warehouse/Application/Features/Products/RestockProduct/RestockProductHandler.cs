using Kofi.Common.Abstractions;
using Kofi.Warehouse.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Warehouse.Application.Features.Products.RestockProduct;

public class RestockProductHandler(IWarehouseDbContext db) : ICommandHandler<RestockProductCommand>
{
    public async Task Handle(RestockProductCommand command, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == command.Id, ct) ??
                      throw new NotFoundException("Product not found");

        product.Restock(new Quantity(command.Quantity));
    }
}

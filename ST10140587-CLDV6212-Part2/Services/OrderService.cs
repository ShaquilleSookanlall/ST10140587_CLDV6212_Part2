using Azure.Data.Tables;
using ST10140587_CLDV6212_Part2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IOrderService
{
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> GetOrdersByCustomerAsync(string customerEmail);
}

public class OrderService : IOrderService
{
    private readonly TableClient _orderTableClient;

    public OrderService(string connectionString)
    {
        _orderTableClient = new TableClient(connectionString, "Orders");
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        var orders = new List<Order>();

        await foreach (var order in _orderTableClient.QueryAsync<Order>())
        {
            orders.Add(order);
        }

        return orders;
    }

    public async Task<List<Order>> GetOrdersByCustomerAsync(string customerEmail)
    {
        var orders = new List<Order>();

        await foreach (var order in _orderTableClient.QueryAsync<Order>(o => o.CustomerName == customerEmail))
        {
            orders.Add(order);
        }

        return orders;
    }
}

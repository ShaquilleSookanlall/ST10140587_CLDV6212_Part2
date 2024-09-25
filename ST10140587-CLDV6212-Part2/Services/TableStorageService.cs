using Azure;
using Azure.Data.Tables;
using ST10140587_CLDV6212_Part2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class TableStorageService
{
    private readonly TableClient _productTableClient;
    private readonly TableClient _customerTableClient;
    private readonly TableClient _orderTableClient;

    public TableStorageService(string connectionString)
    {
        // Initialize TableClients for each entity
        _productTableClient = new TableClient(connectionString, "Products");
        _customerTableClient = new TableClient(connectionString, "Customers");
        _orderTableClient = new TableClient(connectionString, "Transactions");
    }

    // ------------------------------------------
    // PRODUCTS TABLE OPERATIONS
    // ------------------------------------------

    // Retrieve all products
    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = new List<Product>();

        await foreach (var product in _productTableClient.QueryAsync<Product>())
        {
            products.Add(product);
        }

        return products;
    }

    // Add a new product
    public async Task AddProductAsync(Product product)
    {
        if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            await _productTableClient.AddEntityAsync(product);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error adding product to Table Storage", ex);
        }
    }

    // Delete a product
    public async Task DeleteProductAsync(string partitionKey, string rowKey)
    {
        await _productTableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

    // Get a specific product
    public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _productTableClient.GetEntityAsync<Product>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    // ------------------------------------------
    // CUSTOMERS TABLE OPERATIONS
    // ------------------------------------------

    // Retrieve all customers
    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        var customers = new List<Customer>();

        await foreach (var customer in _customerTableClient.QueryAsync<Customer>())
        {
            customers.Add(customer);
        }

        return customers;
    }

    // Add a new customer (for registration)
    public async Task AddCustomerAsync(Customer customer)
    {
        if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            await _customerTableClient.AddEntityAsync(customer);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error adding customer to Table Storage", ex);
        }
    }

    // Delete a customer
    public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
    {
        await _customerTableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

    // Get a specific customer (for login)
    public async Task<Customer?> GetCustomerAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _customerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    // ------------------------------------------
    // ORDERS TABLE OPERATIONS
    // ------------------------------------------

    // Retrieve all orders
    public async Task<List<Order>> GetAllOrdersAsync()
    {
        var orders = new List<Order>();
        await foreach (var order in _orderTableClient.QueryAsync<Order>())
        {
            orders.Add(order);
        }
        return orders;
    }

    // Add a new order
    public async Task AddOrderAsync(Order order)
    {
        if (string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            await _orderTableClient.AddEntityAsync(order);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error adding order to Table Storage", ex);
        }
    }

    // Delete an order
    public async Task DeleteOrderAsync(string partitionKey, string rowKey)
    {
        await _orderTableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

    // Get a specific order
    public async Task<Order?> GetOrderAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _orderTableClient.GetEntityAsync<Order>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    // Update an existing order
    public async Task UpdateOrderAsync(Order order)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order), "Order cannot be null.");
        }

        try
        {
            await _orderTableClient.UpdateEntityAsync(order, order.ETag, TableUpdateMode.Replace);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error updating order in Table Storage", ex); 
        }
    }
}

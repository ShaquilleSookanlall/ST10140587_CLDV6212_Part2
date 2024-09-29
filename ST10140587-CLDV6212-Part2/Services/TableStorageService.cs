using Azure;
using Azure.Data.Tables;
using ST10140587_CLDV6212_Part2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class TableStorageService
{
    private readonly TableClient _productTableClient;
    private readonly TableClient _userTableClient; // Renamed to users instead of customers
    private readonly TableClient _orderTableClient;

    public TableStorageService(string connectionString)
    {
        // Initialize TableClients for each entity
        _productTableClient = new TableClient(connectionString, "Products");
        _userTableClient = new TableClient(connectionString, "Users"); // Renamed from Customers to Users
        _orderTableClient = new TableClient(connectionString, "Orders"); // Renamed from Transactions to Orders
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
    // USERS (CUSTOMERS/ADMINS) TABLE OPERATIONS
    // ------------------------------------------

    // Retrieve all users
    public async Task<List<User>> GetAllUsersAsync()
    {
        var users = new List<User>();

        await foreach (var user in _userTableClient.QueryAsync<User>())
        {
            users.Add(user);
        }

        return users;
    }

    // Add a new user (for registration)
    public async Task AddUserAsync(User user)
    {
        if (string.IsNullOrEmpty(user.PartitionKey) || string.IsNullOrEmpty(user.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            await _userTableClient.AddEntityAsync(user);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error adding user to Table Storage", ex);
        }
    }

    // Delete a user
    public async Task DeleteUserAsync(string partitionKey, string rowKey)
    {
        await _userTableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

    // Get a specific user (for login)
    public async Task<User?> GetUserAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _userTableClient.GetEntityAsync<User>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    // Retrieve a user by email and password for login
    public async Task<User?> GetUserByEmailAndPasswordAsync(string email, string password)
    {
        try
        {
            var users = _userTableClient.Query<User>(u => u.Email == email && u.Password == password);
            return users.FirstOrDefault(); // Assuming email and password match a single user
        }
        catch (RequestFailedException ex)
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

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        try
        {
            var products = _productTableClient.Query<Product>(p => p.Product_Id == productId);
            return products.FirstOrDefault();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

}

using Microsoft.AspNetCore.Http;
using ST10140587_CLDV6212_Part2.Models;
using System.Collections.Generic;
using System.Linq;

public class CartService
{
    private readonly ISession _session;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        // Ensure the session is available
        _session = httpContextAccessor.HttpContext.Session;
    }

    public List<CartItem> GetCartItems()
    {
        return _session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
    }

    public void AddToCart(CartItem item)
    {
        var cart = GetCartItems();
        cart.Add(item);
        _session.SetObjectAsJson("Cart", cart);
    }

    public void RemoveFromCart(string productId)
    {
        var cart = GetCartItems();
        var item = cart.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            cart.Remove(item);
            _session.SetObjectAsJson("Cart", cart);
        }
    }

    public void ClearCart()
    {
        _session.Remove("Cart");
    }
}

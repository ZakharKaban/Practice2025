using System;
using System.Data;
using System.Data.Objects;
using Practice2025One.AppData;

namespace Practice2025One.Class
{
    public static class EFHelper
    {
        public static void SetUserRole(Users user, Roles role)
        {
            if (user != null && role != null)
            {
                user.RolesReference.EntityKey = new EntityKey("Entities1.Roles", "RoleID", role.RoleID);
            }
        }
        
        public static void SetCartUser(Carts cart, Users user)
        {
            if (cart != null && user != null)
            {
                cart.UsersReference.EntityKey = new EntityKey("Entities1.Users", "UserID", user.UserID);
            }
        }
        
        public static void SetOrderUser(Orders order, Users user)
        {
            if (order != null && user != null)
            {
                order.UsersReference.EntityKey = new EntityKey("Entities1.Users", "UserID", user.UserID);
            }
        }
        
        public static int? GetUserRoleID(Users user)
        {
            if (user != null && user.RolesReference.EntityKey != null)
            {
                return (int?)user.RolesReference.EntityKey.EntityKeyValues[0].Value;
            }
            return null;
        }
        
        public static int? GetCartUserID(Carts cart)
        {
            if (cart != null && cart.UsersReference.EntityKey != null)
            {
                return (int?)cart.UsersReference.EntityKey.EntityKeyValues[0].Value;
            }
            return null;
        }
        
        public static void SetCartItemCart(CartItems cartItem, Carts cart)
        {
            if (cartItem != null && cart != null)
            {
                cartItem.CartsReference.EntityKey = new EntityKey("Entities1.Carts", "CartID", cart.CartID);
            }
        }
        
        public static void SetCartItemProduct(CartItems cartItem, Products product)
        {
            if (cartItem != null && product != null)
            {
                cartItem.ProductsReference.EntityKey = new EntityKey("Entities1.Products", "ProductID", product.ProductID);
            }
        }
        
        public static void SetOrderItemOrder(OrderItems orderItem, Orders order)
        {
            if (orderItem != null && order != null)
            {
                orderItem.OrdersReference.EntityKey = new EntityKey("Entities1.Orders", "OrderID", order.OrderID);
            }
        }
        
        public static void SetOrderItemProduct(OrderItems orderItem, Products product)
        {
            if (orderItem != null && product != null)
            {
                orderItem.ProductsReference.EntityKey = new EntityKey("Entities1.Products", "ProductID", product.ProductID);
            }
        }
        
        public static int? GetCartItemProductID(CartItems cartItem)
        {
            if (cartItem != null && cartItem.ProductsReference.EntityKey != null)
            {
                return (int?)cartItem.ProductsReference.EntityKey.EntityKeyValues[0].Value;
            }
            return null;
        }
    }
}


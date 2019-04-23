using System;
using System.Collections.Generic;
using System.Text;

namespace MicroShop
{
	class MicroShop
	{
		public string schemaName { get; set; }
		public Entity[] entities { get; set; }
	}

	public class Entity
	{
		public Customer Customer { get; set; }
		public Order Order { get; set; }
		public OrderLine OrderLine { get; set; }
		public Product Product { get; set; }
	}

	public class Customer
	{
		public string name { get; set; }
		public List<Order> orders { get; set; }
	}

	public class Order
	{
		public string date { get; set; }
		public decimal total { get; set; }
		public Customer customer { get; set; }
		public List<OrderLine> lines { get; set; }
	}

	public class OrderLine
	{
		public Order order { get; set; }
		public Product product { get; set; }
		public decimal count { get; set; }
		public decimal total { get; set; }
	}

	public class Product
	{
		public string name { get; set; }
		public decimal price { get; set; }
	}
}
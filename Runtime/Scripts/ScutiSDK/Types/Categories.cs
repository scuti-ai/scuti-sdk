using System.ComponentModel;

namespace Scuti {
	public enum Categories {
        [Description("Apparel")]
		Apparel= 0,
        [Description("Sporting Goods")]
		SportingGoods = 1, 
        [Description("Auto")]
		Auto = 2,
        [Description("Beauty")]
        Beauty = 3,
        [Description("Beverages")]
        Beverages = 4,
        [Description("Books")]
        Books = 5,
        [Description("CBD Products")]
        CBD = 6,
        [Description("Electronics")]
        Electronics = 7,
        [Description("Financials")]
        Finance = 8,
        [Description("Food")]
        Food = 9,
        [Description("Fitness")]
        Fitness = 10,
        [Description("Gaming")]
        Gaming = 11,
        [Description("Gifts")]
        Gifts = 12,
        [Description("Health Products")]
        Health = 13,
        [Description("Home / Office")]
        HomeOffice = 14,
        [Description("Movies & TV")]
        Movies = 15,
        [Description("Pet Supplies")]
        Pets = 16,
        [Description("Toys / Games")]
        Toys = 17,
        [Description("Warranties")]
        Warranties = 18,
        [Description("Computer Hardware")]
        ComputerHardware = 19,
	}
}
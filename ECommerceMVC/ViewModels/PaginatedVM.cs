namespace ECommerceMVC.ViewModels
{
	public class PaginatedVM<T>
	{
		public IEnumerable<T> Items { get; set; } = new List<T>();
		public int CurrentPage { get; set; }
		public int TotalPages { get; set; }
		public int PageSize { get; set; }
		public int TotalItems { get; set; }
		public int? CategoryId { get; set; }
		public double? MinPrice { get; set; }
		public double? MaxPrice { get; set; }
		public string? SortBy { get; set; }

		public bool HasPreviousPage => CurrentPage > 1;
		public bool HasNextPage => CurrentPage < TotalPages;
	}
}

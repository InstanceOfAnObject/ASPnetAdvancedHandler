using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace System.Collections.Generic
{
	[DataContract]
	public class PagingInfo
	{
		public PagingInfo() { }
		public PagingInfo(string sortFieldName, SortDirectionOptions sortDirection, int pageNumber, int pageSize)
		{
			this.SortFieldName = sortFieldName;
			this.SortDirection = sortDirection;
			this.PageNumber = pageNumber;
			this.PageSize = pageSize;
		}
		public PagingInfo(string sortFieldName, string sortDirection, int pageNumber, int pageSize)
			: this(sortFieldName, SortDirectionOptions.Ascending, pageNumber, pageSize)
		{
			sortDirection = string.IsNullOrEmpty(sortDirection) ? "ASC" : sortDirection;
			switch (sortDirection.ToUpper())
			{
				case "ASC":
					this.SortDirection = SortDirectionOptions.Ascending;
					break;
				case "DESC":
					this.SortDirection = SortDirectionOptions.Descending;
					break;
				default:
					this.SortDirection = SortDirectionOptions.Ascending;
					break;
			}
		}

		string _sortFieldName = string.Empty;
		[DataMember]
		public string SortFieldName
		{
			get
			{
				return _sortFieldName;
			}
			set
			{
				_sortFieldName = value;
			}
		}

		SortDirectionOptions _sortDirection = SortDirectionOptions.Ascending;
		[DataMember]
		public SortDirectionOptions SortDirection
		{
			get
			{
				if (_sortDirection == SortDirectionOptions.Undefined)
				{
					_sortDirection = SortDirectionOptions.Ascending;
				}
				return _sortDirection;
			}
			set
			{
				_sortDirection = value;
			}
		}

		int _pageNumber = 1;
		[DataMember]
		public int PageNumber
		{
			get
			{
				if (_pageNumber <= 0)
				{
					_pageNumber = 1;
				}
				return _pageNumber;
			}
			set
			{
				_pageNumber = value;
			}
		}

		int _pageSize = 20;
		[DataMember]
		public int PageSize
		{
			get
			{
				return _pageSize;
			}
			set
			{
				_pageSize = value;
			}
		}

		int _totalRecords;
		[DataMember]
		public int TotalRecords
		{
			get
			{
				//if (_totalRecords < 0)
				//{
				//    _totalRecords = 0;
				//}

				return _totalRecords;
			}
			set
			{
				_totalRecords = value;
			}
		}

		public static PagingInfo Default
		{
			get
			{
				return new PagingInfo()
				{
					PageNumber = 1,
					PageSize = int.MaxValue,
					SortDirection = SortDirectionOptions.Descending,
					SortFieldName = string.Empty
				};
			}
		}

		public int TotalPages()
		{
			if (TotalRecords > 0 && PageSize > 0)
				return (int)Math.Ceiling((double)TotalRecords / (double)PageSize);
			else
				return 0;
		}

		public bool? HasPreviousPage()
		{
			return (PageNumber > 1);
		}

		public bool? HasNextPage() { return (PageNumber < TotalPages()); }

		public bool? IsFirstPage() { return PageNumber == 1; }

		public bool? IsLastPage() { return PageNumber == TotalPages(); }

		public bool IsPaged() { return PageSize > 0; }


	}
}

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Linq.Expressions;

namespace System.Collections.Generic
{
	public partial class PagedList<T>
	{

		public PagedList() { }
		public PagedList(IEnumerable<T> source, PagingInfo paging)
		{
			if (source == null)
				source = new List<T>();

			Items.AddRange(source);
			_pagingInfo = paging;
		}

		List<T> _items = null;
		[DataMember]
		public List<T> Items
		{
			get
			{
				if (_items == null)
					_items = new List<T>();

				return _items;
			}
			set
			{
				_items = value;
			}
		}

		PagingInfo _pagingInfo;
		[DataMember]
		public PagingInfo PagingInfo
		{
			get
			{
				if (_pagingInfo == null)
					_pagingInfo = new PagingInfo();

				return _pagingInfo;
			}
			set
			{
				_pagingInfo = value;
			}
		}

	}

}

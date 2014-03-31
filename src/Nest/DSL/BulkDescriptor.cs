﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Elasticsearch.Net;
using Nest.Domain;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace Nest
{
	
	public partial class BulkDescriptor :
		FixedIndexTypePathDescriptor<BulkDescriptor, BulkRequestParameters>
		, IPathInfo<BulkRequestParameters>
	{
		internal IList<BaseBulkOperation> _Operations = new SynchronizedCollection<BaseBulkOperation>();

		public BulkDescriptor Create<T>(Func<BulkCreateDescriptor<T>, BulkCreateDescriptor<T>> bulkCreateSelector) where T : class
		{
			bulkCreateSelector.ThrowIfNull("bulkCreateSelector");
			var descriptor = bulkCreateSelector(new BulkCreateDescriptor<T>());
			if (descriptor == null)
				return this;
			this._Operations.Add(descriptor);
			return this;
		}
		
		/// <summary>
		/// CreateMany, convenience method to create many documents at once.
		/// </summary>
		/// <param name="objects">the objects to create</param>
		/// <param name="bulkCreateSelector">A func called on each object to describe the individual create operation</param>
		public BulkDescriptor CreateMany<T>(IEnumerable<T> @objects, Func<BulkCreateDescriptor<T>, T, BulkCreateDescriptor<T>> bulkCreateSelector = null) where T : class
		{
			bulkCreateSelector = bulkCreateSelector ?? ((d, o) => d);
			foreach (var descriptor in @objects.Select(o => bulkCreateSelector(new BulkCreateDescriptor<T>().Object(o), o)))
				this._Operations.Add(descriptor);
			return this;
		}

		public BulkDescriptor Index<T>(Func<BulkIndexDescriptor<T>, BulkIndexDescriptor<T>> bulkIndexSelector) where T : class
		{
			bulkIndexSelector.ThrowIfNull("bulkIndexSelector");
			var descriptor = bulkIndexSelector(new BulkIndexDescriptor<T>());
			if (descriptor == null)
				return this;
			this._Operations.Add(descriptor);
			return this;
		}

		/// <summary>
		/// IndexMany, convenience method to pass many objects at once.
		/// </summary>
		/// <param name="objects">the objects to index</param>
		/// <param name="bulkIndexSelector">A func called on each object to describe the individual index operation</param>
		public BulkDescriptor IndexMany<T>(IEnumerable<T> @objects, Func<BulkIndexDescriptor<T>, T, BulkIndexDescriptor<T>> bulkIndexSelector = null) where T : class
		{
			bulkIndexSelector = bulkIndexSelector ?? ((d, o) => d);
			foreach (var descriptor in @objects.Select(o => bulkIndexSelector(new BulkIndexDescriptor<T>().Object(o), o)))
				this._Operations.Add(descriptor);
			return this;
		}

		public BulkDescriptor Delete<T>(Func<BulkDeleteDescriptor<T>, BulkDeleteDescriptor<T>> bulkDeleteSelector) where T : class
		{
			bulkDeleteSelector.ThrowIfNull("bulkDeleteSelector");
			var descriptor = bulkDeleteSelector(new BulkDeleteDescriptor<T>());
			if (descriptor == null)
				return this;
			this._Operations.Add(descriptor);
			return this;
		}
		
		/// <summary>
		/// DeleteMany, convenience method to delete many objects at once.
		/// </summary>
		/// <param name="objects">the objects to delete</param>
		/// <param name="bulkDeleteSelector">A func called on each object to describe the individual delete operation</param>
		public BulkDescriptor DeleteMany<T>(IEnumerable<T> @objects, Func<BulkDeleteDescriptor<T>, T, BulkDeleteDescriptor<T>> bulkDeleteSelector = null) where T : class
		{
			bulkDeleteSelector = bulkDeleteSelector ?? ((d, o)=>d);
			foreach (var descriptor in @objects.Select(o => bulkDeleteSelector(new BulkDeleteDescriptor<T>().Object(o), o)))
				this._Operations.Add(descriptor);
			return this;
		}
		
		/// <summary>
		/// DeleteMany, convenience method to delete many objects at once.
		/// </summary>
		/// <param name="ids">Enumerable of string ids to delete</param>
		/// <param name="bulkDeleteSelector">A func called on each ids to describe the individual delete operation</param>
		public BulkDescriptor DeleteMany<T>(IEnumerable<string> ids, Func<BulkDeleteDescriptor<T>, string, BulkDeleteDescriptor<T>> bulkDeleteSelector = null) where T : class
		{
			bulkDeleteSelector = bulkDeleteSelector ?? ((d, s)=> d);
			foreach (var descriptor in ids.Select(o => bulkDeleteSelector(new BulkDeleteDescriptor<T>().Id(o), o)))
				this._Operations.Add(descriptor);
			return this;
		}
		
		/// <summary>
		/// DeleteMany, convenience method to delete many objects at once.
		/// </summary>
		/// <param name="ids">Enumerable of int ids to delete</param>
		/// <param name="bulkDeleteSelector">A func called on each ids to describe the individual delete operation</param>
		public BulkDescriptor DeleteMany<T>(IEnumerable<int> ids, Func<BulkDeleteDescriptor<T>, string, BulkDeleteDescriptor<T>> bulkDeleteSelector = null) where T : class
		{
			return this.DeleteMany(ids.Select(i=>i.ToString(CultureInfo.InvariantCulture)), bulkDeleteSelector);
		}

		public BulkDescriptor Update<T>(Func<BulkUpdateDescriptor<T, T>, BulkUpdateDescriptor<T, T>> bulkUpdateSelector) where T : class
		{
			return this.Update<T, T>(bulkUpdateSelector);
		}
		public BulkDescriptor Update<T, K>(Func<BulkUpdateDescriptor<T, K>, BulkUpdateDescriptor<T, K>> bulkUpdateSelector)
			where T : class
			where K : class
		{
			bulkUpdateSelector.ThrowIfNull("bulkUpdateSelector");
			var descriptor = bulkUpdateSelector(new BulkUpdateDescriptor<T, K>());
			if (descriptor == null)
				return this;
			this._Operations.Add(descriptor);
			return this;
		}

		ElasticsearchPathInfo<BulkRequestParameters> IPathInfo<BulkRequestParameters>.ToPathInfo(IConnectionSettingsValues settings)
		{
			var pathInfo = this.ToPathInfo<BulkRequestParameters>(settings, this._QueryString);
			pathInfo.HttpMethod = PathInfoHttpMethod.POST;
			return pathInfo;
		}
	}
}

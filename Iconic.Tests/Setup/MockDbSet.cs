using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iconic.Tests
{
    /// <summary>
    /// Holds an in-memory collection of items that will be used by db context, it is a mock implementation of DbSet.
    /// </summary>
    public class MockDbSet<T> : DbSet<T>, IQueryable, IEnumerable<T>
        where T : class
    {
        ObservableCollection<T> items;
        IQueryable query;

        public MockDbSet()
        {
            items = new ObservableCollection<T>();
            query = items.AsQueryable();
        }

        public override T Add(T item)
        {
            items.Add(item);
            return item;
        }

        public override IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            foreach(var entity in entities)
            items.Add(entity);
            return items;
        }
        
        public override T Remove(T item)
        {
            items.Remove(item);
            return item;
        }

        public override T Attach(T item)
        {
            items.Add(item);
            return item;
        }

        public override T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public override TDerivedEntity Create<TDerivedEntity>()
        {
            return Activator.CreateInstance<TDerivedEntity>();
        }

        public override ObservableCollection<T> Local
        {
            get { return new ObservableCollection<T>(items); }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        Type IQueryable.ElementType
        {
            get { return query.ElementType; }
        }

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get { return query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return query.Provider; }
        }

    }
}

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace ChronoChart
{
    public static class DependencyObjectExtensions
    {
        public static IObservable<TProperty> OnPropertyChanges<T, TProperty>(this T source, Expression<Func<T, TProperty>> property)
            where T : DependencyObject
        {
            return Observable.Create<TProperty>(o =>
            {
                var propertyName = property.GetPropertyInfo().Name;
                var dpd = DependencyPropertyDescriptor.FromName(propertyName, typeof (T), typeof (T));
                if (dpd == null)
                {
                    o.OnError(new InvalidOperationException("Can not register change handler for this dependency property."));
                }
                var propertySelector = property.Compile();

                EventHandler handler = delegate { o.OnNext(propertySelector(source)); };
                dpd.AddValueChanged(source, handler);

                return Disposable.Create(() => dpd.RemoveValueChanged(source, handler));
            });
        }
    }
}
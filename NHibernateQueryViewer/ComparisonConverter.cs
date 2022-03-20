using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace NHibernateQueryViewer
{
    public class ComparisonConverter : MarkupExtension, IValueConverter
    {
        private static ComparisonConverter? _converter;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ??= new ComparisonConverter();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Synchronizer2.Converters
{
    class InvertableBooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Включение инверсии (false - видно, true - не видно)
        /// </summary>
        public Boolean Inverted { get; set; } = false;

        /// <summary>
        /// Включение коллапсирования (true - убирается из разметки, false - остаётся в разметке)
        /// </summary>
        public Boolean Collapsed { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value ^ Inverted) ? Visibility.Visible : Collapsed ? Visibility.Collapsed : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (((Visibility)value) == Visibility.Visible) ^ Inverted;
        }
    }
}

using System.Globalization;

namespace OramaGo.Converters;

public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(value?.ToString());
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
            return Colors.Red;
        return Colors.Green;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CreditoToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal credito)
        {
            if (credito < 0)
                return Colors.Red;
            if (credito < 1000)
                return Colors.Orange;
            return Colors.Green;
        }
        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SyncStatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Models.SyncStatus status)
        {
            return status switch
            {
                Models.SyncStatus.Synced => Colors.Green,
                Models.SyncStatus.Pending => Colors.Orange,
                Models.SyncStatus.Modified => Colors.Blue,
                Models.SyncStatus.Conflict => Colors.Red,
                Models.SyncStatus.Error => Colors.DarkRed,
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SyncStatusToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Models.SyncStatus status)
        {
            return status switch
            {
                Models.SyncStatus.Synced => "Sincronizado",
                Models.SyncStatus.Pending => "Pendente",
                Models.SyncStatus.Modified => "Modificado",
                Models.SyncStatus.Conflict => "Conflito",
                Models.SyncStatus.Error => "Erro",
                _ => "Desconhecido"
            };
        }
        return "Desconhecido";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
public class EstoqueToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal estoque)
        {
            if (estoque <= 0)
                return Colors.Red;
            else if (estoque <= 10)
                return Colors.Orange;
            else
                return Colors.Green;
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusVendaToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Models.StatusVendaLocal status)
        {
            return status switch
            {
                Models.StatusVendaLocal.Orcamento => Color.FromArgb("#FF9800"), // Orange
                Models.StatusVendaLocal.Aprovado => Color.FromArgb("#2196F3"), // Blue
                Models.StatusVendaLocal.Faturado => Color.FromArgb("#4CAF50"), // Green
                Models.StatusVendaLocal.Entregue => Color.FromArgb("#8BC34A"), // Light Green
                Models.StatusVendaLocal.Cancelado => Color.FromArgb("#F44336"), // Red
                _ => Color.FromArgb("#9E9E9E") // Gray
            };
        }
        return Color.FromArgb("#9E9E9E");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return false;
    }
}

public class SyncStatusToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Models.SyncStatus status)
        {
            return status switch
            {
                Models.SyncStatus.Synced => "✓",
                Models.SyncStatus.Pending => "⏳",
                Models.SyncStatus.Modified => "📝",
                Models.SyncStatus.Conflict => "⚠️",
                Models.SyncStatus.Error => "❌",
                _ => "?"
            };
        }
        return "?";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SyncStatusToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Models.SyncStatus status)
        {
            // Mostrar apenas se não estiver sincronizado
            return status != Models.SyncStatus.Synced;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusVendaToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Models.StatusVendaLocal status)
        {
            return status switch
            {
                Models.StatusVendaLocal.Orcamento => "Orçamento",
                Models.StatusVendaLocal.Aprovado => "Aprovado",
                Models.StatusVendaLocal.Faturado => "Faturado",
                Models.StatusVendaLocal.Entregue => "Entregue",
                Models.StatusVendaLocal.Cancelado => "Cancelado",
                _ => "Desconhecido"
            };
        }
        
        if (value == null)
            return "Todos";
        
        return value.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
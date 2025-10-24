using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using radiosw.ViewModels;

namespace radiosw {
    public class ViewLocator : IDataTemplate {
        public Control? Build(Object? arg_data) {
            if (arg_data is null) { return null; }
            String name = arg_data.GetType().FullName!.Replace("ViewModel" , "View" , StringComparison.Ordinal);
            Type?  type = Type.GetType(name);
            if (type == null) { return new TextBlock { Text = "NO " + name }; }
            Control control = ( Control ) Activator.CreateInstance(type)!;
            control.DataContext = arg_data;
            return control;
        }

        public Boolean Match(Object? arg_data) => arg_data is ViewModelBase;
    }
}
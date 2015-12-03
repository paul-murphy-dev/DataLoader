using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Markup;

namespace DataLoaderWPF.Helpers
{
    public static class ViewMapper
    {
        public static void Map<TViewModel, TView>(ResourceDictionary scope) where TView : FrameworkElement
        {
            var template = CreateTemplate(typeof(TViewModel), typeof(TView));
            var key = template.DataTemplateKey;
            scope.Add(key, template);
        }

        public static void UnMap<TViewModel, TView>(ResourceDictionary scope) where TView : FrameworkElement
        {
            var template = CreateTemplate(typeof(TViewModel), typeof(TView));
            var key = template.DataTemplateKey;
            scope.Remove(key);
        }

        private static DataTemplate CreateTemplate(Type viewModelType, Type viewType)
        {
            const string xamlTemplate = "<DataTemplate DataType=\"{{x:Type vm:{0}}}\"><v:{1} /></DataTemplate>";
            var xaml = String.Format(xamlTemplate, viewModelType.Name, viewType.Name, viewModelType.Namespace, viewType.Namespace);

            var context = new ParserContext();

            context.XamlTypeMapper = new XamlTypeMapper(new string[0]);
            context.XamlTypeMapper.AddMappingProcessingInstruction("vm", viewModelType.Namespace, viewModelType.Assembly.FullName);
            context.XamlTypeMapper.AddMappingProcessingInstruction("v", viewType.Namespace, viewType.Assembly.FullName);

            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            context.XmlnsDictionary.Add("vm", "vm");
            context.XmlnsDictionary.Add("v", "v");

            var template = (DataTemplate)XamlReader.Parse(xaml, context);
            return template;
        }
    }
}

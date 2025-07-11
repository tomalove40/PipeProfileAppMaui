using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeProfileAppMaui.ViewModels
{
    public class RibbonItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ButtonTemplate { get; set; }
        public DataTemplate LabelTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                RibbonButtonItem => ButtonTemplate,
                RibbonLabelItem => LabelTemplate,
                _ => null
            };
        }
    }
}

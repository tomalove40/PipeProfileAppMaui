using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeProfileAppMaui.Controls
{
    public partial class PipeProfileView
    {
        void PositionEditor() => _editorManager.PositionEditor();
        void EndEdit() => _editorManager.EndEdit();
    }
}

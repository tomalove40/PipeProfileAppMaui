using PipeProfileAppMaui.Models;
using PipeProfileAppMaui.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeProfileAppMaui.Controls
{
public partial class PipeProfileView : ContentView
{
    //readonly ProfileRenderer _renderer;
    readonly EditorManager   _editorMgr;

        public PipeProfileView()
        {
            InitializeComponent();

            // 1) Инициализируем ГОСТ-штамп
            _stamp = new Stamp();

            // 2) Создаём ProfileRenderer и EditorManager (VM подставится в OnViewModelChanged)
            //_renderer = new ProfileRenderer();
            _editorManager = new EditorManager(
                root: RootLayout,
                canvas: CanvasView,
                stamp: _stamp,
                vm: null,
                invalidateSurface: () => CanvasView.InvalidateSurface());

            CanvasView.HandlerChanged += (s, _) => AttachNativeHandlers();

            // Pinch — масштабирование
            var pinch = new PinchGestureRecognizer();
            pinch.PinchUpdated += OnPinchUpdated;
            CanvasView.GestureRecognizers.Add(pinch);

            // Pan — панорамирование
            var pan = new PanGestureRecognizer();
            pan.PanUpdated += OnPanUpdated;
            CanvasView.GestureRecognizers.Add(pan);

            // сразу отрисуем лист
            CanvasView.InvalidateSurface();

            // подписываемся на ресайз сцены (ContentView)
            this.SizeChanged += OnViewSizeChanged;

            // или на сам CanvasView, если нужно
            CanvasView.SizeChanged += OnCanvasSizeChanged;
        }
    }
}

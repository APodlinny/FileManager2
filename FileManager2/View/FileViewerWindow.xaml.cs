namespace FileManager2.View
{
    /// <summary>
    /// Interaction logic for FileViewerWindow.xaml
    /// </summary>
    public partial class FileViewerWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileViewerWindow"/> class.
        /// </summary>
        /// <param name="fileViewerViewModel">
        /// The file Viewer View Model.
        /// </param>
        public FileViewerWindow(ViewModel.FileViewer fileViewer)
        {
            DataContext = fileViewer;

            InitializeComponent();
        }
    }
}

namespace FileManager2.View
{
    /// <summary>
    /// Interaction logic for DiskInfoWindow.xaml
    /// </summary>
    public partial class PartitionInfoWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionInfoWindow"/> class.
        /// </summary>
        /// <param name="infoViewModel">
        /// The info view model.
        /// </param>
        public PartitionInfoWindow(ViewModel.PartitionInfo info)
        {
            DataContext = info;
            InitializeComponent();
        }
    }
}

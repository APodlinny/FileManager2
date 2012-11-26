using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FileManager2;
using System.Windows.Input;
using System.Windows;

namespace FileManager2.ViewModel
{
    public class Partition : INotifyPropertyChanged
    {
        private IEnumerable<Model.IPartition> _partitions;
        private Model.IPartition _currentPartition;

        public Partition()
        {
            try
            {
                _partitions = Model.PartitionEnumerator.Enumerate();
                _currentPartition = _partitions.First(d => d.Info().Active);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<Model.IPartition> Partitions
        {
            get
            {
                try
                {
                    return _partitions;
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
                    return new List<Model.UnknownPartition> { new Model.UnknownPartition(new Model.PartitionInfo()) };
                }
            }
        }

        public PartitionInfo Info
        {
            get 
            {
                try
                {
                    return new PartitionInfo(_currentPartition.Info());
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
                    return new PartitionInfo(new Model.PartitionInfo());
                }
            }
        }

        public List<char> DiskLetters
        {
            get
            {
                try
                {
                    return _partitions.Select(d => d.Info().Letter).ToList();
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
                    return new List<char> { '?' };
                }
            }
        }

        public char CurrentDiskLetter
        {
            get
            {
                try
                {
                    return _currentPartition.Info().Letter;
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
                    return '?';
                }
            }

            set
            {
                try
                {
                    if (_currentPartition.Info().Letter != value)
                    {
                        _currentPartition = _partitions.First(d => d.Info().Letter == value);
                        OnPropertyChanged("CurrentPath");
                        OnPropertyChanged("DirectoryObjects");
                    }
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
                }
            }
        }

        public string CurrentPath
        {
            get
            {
                return _currentPartition.CurrentPath();
            }
        }

        public IEnumerable<DirectoryObject> DirectoryObjects
        {
            get
            {
                try
                {
                    var dirs = _currentPartition.CurrentDirectoryContents().
                        Where(x => x.Type == Model.DirectoryObjectType.Directory).
                        OrderBy(x => x.Name);

                    var files = _currentPartition.CurrentDirectoryContents().
                        Where(x => x.Type == Model.DirectoryObjectType.File).
                        OrderBy(x => x.Name);

                    return dirs.Concat(files).Select(x => new DirectoryObject(x, this));
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
                    return new List<DirectoryObject> { new DirectoryObject(new Model.DirectoryObject(), this) };
                }
            }
        }

        public Model.IPartition CurrentDisk
        {
            get
            {
                try
                {
                    return _currentPartition;
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
                    return new Model.UnknownPartition(new Model.PartitionInfo());
                }
            }

            set
            {
                if ((_currentPartition == null) || (_currentPartition != value))
                {
                    _currentPartition = value;
                    OnPropertyChanged("CurrentPath");
                    OnPropertyChanged("DirectoryObjects");
                }
            }
        }

        public void GoRootDirectory()
        {
            try
            {
                _currentPartition.GoRootDirectory();

                OnPropertyChanged("CurrentPath");
                OnPropertyChanged("DirectoryObjects");
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
            }
        }

        public void GoSubDirectory(string name)
        {
            try
            {
                _currentPartition.GoSubDirectory(name);

                OnPropertyChanged("CurrentPath");
                OnPropertyChanged("DirectoryObjects");
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
            }
        }

        public byte[] GetFileContent(string name)
        {
            try
            {
                return _currentPartition.ReadFile(name);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
                return new byte[1];
            }
        }

        private Extentions.Command _backCommand;
        public ICommand BackCommand
        {
            get
            {
                if (_backCommand == null)
                {
                    _backCommand = new Extentions.Command(param => GoRootDirectory());
                }
                return _backCommand;
            }
        }

        private Extentions.Command _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new Extentions.Command(param => (param as Window).Close());
                }
                return _closeCommand;
            }
        }

        private Extentions.Command _infoCommand;
        public ICommand InfoCommand
        {
            get
            {
                if (_infoCommand == null)
                {
                    _infoCommand = new Extentions.Command(param => (new View.PartitionInfoWindow(new PartitionInfo(_currentPartition.Info()))).Show());
                }
                return _infoCommand;
            }
        }

        private Extentions.Command _aboutCommand;
        public ICommand AboutCommand
        {
            get
            {
                if (_aboutCommand == null)
                {
                    _aboutCommand = new Extentions.Command(param => (new View.AboutWindow()).Show());
                }
                return _aboutCommand;
            }
        }
        
        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}

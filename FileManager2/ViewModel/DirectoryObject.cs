using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using FileManager2;

namespace FileManager2.ViewModel
{
    public enum DirectoryObjectType
    {
        File,
        Directory
    }

    public class DirectoryObject
    {
        private Partition _partition;

        public DirectoryObject(Model.DirectoryObject obj, Partition partition)
        {
            _partition = partition;
            Name = obj.Name;
            Size = obj.Size;
            Type = obj.Type == Model.DirectoryObjectType.Directory ? 
                DirectoryObjectType.Directory :
                DirectoryObjectType.File;
        }

        public string Name { get; set; }
        public DirectoryObjectType Type { get; set; }
        public int Size { get; set; }

        private Extentions.Command _goSubCommand;
        public ICommand GoSubCommand
        {
            get
            {
                if (_goSubCommand == null)
                {
                    _goSubCommand = new Extentions.Command(param => 
                    {
                        try
                        {
                            if (Type == DirectoryObjectType.Directory)
                            {
                                _partition.GoSubDirectory(Name);
                            }
                            else
                            {
                                var content = _partition.GetFileContent(Name);
                                (new View.FileViewerWindow(new FileViewer(content))).Show();
                            }
                        }
                        catch (Exception e)
                        {
                            System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
                        }
                    });
                }
                return _goSubCommand;
            }
        }
    }
}

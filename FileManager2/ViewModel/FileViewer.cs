using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FileManager2.ViewModel
{
    public class FileViewer : INotifyPropertyChanged
    {
        private byte[] _content;
        private bool _inHex;

        public event PropertyChangedEventHandler PropertyChanged;

        public FileViewer(byte[] content)
        {
            _content = content;
        }

        public string FileContent
        {
            get
            {
                try
                {
                    if (!_inHex)
                    {
                        return Encoding.Default.GetString(_content);
                    }
                    else
                    {
                        var strBytes = _content.Select(x => x.ToString("X2")).ToList();
                        var lines = new List<string>();
                        var line = String.Empty;

                        for (int i = 0; i < strBytes.Count(); i++)
                        {
                            line += strBytes[i] + " ";
                            if ((i + 1) % 20 == 0)
                            {
                                lines.Add(line);
                                line = String.Empty;
                            }
                        }

                        lines.Add(line);

                        return String.Join("\n", lines);
                    }
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
                    return String.Empty;
                }
            }

            set
            {
                try
                {
                    _content = Encoding.Default.GetBytes(value);
                    OnPropertyChanged("FileContent");
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, e.GetType().ToString());
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether InHex.
        /// </summary>
        public bool InHex
        {
            get
            {
                return _inHex;
            }

            set
            {
                _inHex = value;
                OnPropertyChanged("InHex");
                OnPropertyChanged("FileContent");
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

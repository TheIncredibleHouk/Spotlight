using System;
using System.IO;

namespace Spotlight.Models
{
    public class Rom
    {
        private string Filename;
        private byte[] _data;
        private bool[] _dataProtection;

        public Rom()
        {
        }

        public byte this[int index]
        {
            get { return _data[index]; }
            set
            {
                if (_dataProtection[index])
                {
                    throw new Exception("Data at address " + index.ToString("X") + " has already been written to!");
                }

                _data[index] = value;
                _dataProtection[index] = true;
            }
        }

        public bool Load(string filename)
        {
            if (!File.Exists(filename))
            {
                return false;
            }

            if (Path.GetExtension(filename).ToLower() != ".nes")
            {
                throw new Exception("Must be saved to an NES ROM only.");
            }

            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            _data = new byte[fileStream.Length];
            _dataProtection = new bool[fileStream.Length];

            fileStream.Read(_data, 0, (int)fileStream.Length);
            fileStream.Close();

            Filename = filename;
            AllowWrites();
            return true;
        }

        public bool Save()
        {
            FileStream fileStream = new FileStream(Filename, FileMode.Open, FileAccess.Write);
            fileStream.Write(_data, 0, _data.Length);
            fileStream.Close();
            AllowWrites();
            return true;
        }

        public void AllowWrites()
        {
            for (var i = 0; i < _dataProtection.Length; i++)
            {
                _dataProtection[i] = false;
            }
        }
    }
}
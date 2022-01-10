using System.Collections.Generic;

namespace RemoteFileManager.Models
{
    class Dir
    {
        public Dir(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        
        public ISet<Dir> FileExplorer = new HashSet<Dir>();
        public override int GetHashCode()
        => Name.GetHashCode();
        
        public override bool Equals(object obj)
        {
            if (obj is Dir myDir)
            {
                return myDir.Name == this.Name;
            }
            return false;
        }
    }
}

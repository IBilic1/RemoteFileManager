using Azure.Storage.Blobs.Models;
using RemoteFileManager.DAO;
using RemoteFileManager.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteFileManager.ViewModels
{
    class ItemsViewModel
    {
        public ItemsViewModel()
        {
            Items = new ObservableCollection<BlobItem>();
            Directories = new ObservableCollection<string>();
            Root = new Dir("root");
            Refresh();
        }

        public const string ForwardSlash = "/";

        private string direcotry;
        public string Direcotry 
        {
            get => direcotry;
            set
            {
                direcotry = value;
                Refresh(); //kad se promijeni refresh
            }
        }

        public ObservableCollection<string> Directories { get; set; }

        public Dir Root { get; set; }
        public ObservableCollection<BlobItem> Items { get; set; }

        private void Refresh()
        {
            Items.Clear();
            Directories.Clear();
            Repository.Container.GetBlobs().ToList().ForEach(item => {
                if (item.Name.Contains(ForwardSlash))
                {
                    string dir = item.Name.Substring(0, item.Name.LastIndexOf(ForwardSlash));
                    if (!Directories.Contains(dir))
                    {
                        parent = null;
                        initDirectoriyes(dir);
                        Directories.Add(dir);
                    }
                }
                //handle root
                if (string.IsNullOrEmpty(Direcotry) && !item.Name.Contains(ForwardSlash))
                {
                    Items.Add(item);
                }
                else if(!string.IsNullOrEmpty(Direcotry) && item.Name.Substring(0,item.Name.LastIndexOf(ForwardSlash)+1).Equals($"{Direcotry}{ForwardSlash}"))
                {
                    Items.Add(item);
                }

            });

        }
        Dir parent;
        private void initDirectoriyes(string dir)
        {
            // test
            //  test/ivana
            string[] details = dir.Split(ForwardSlash[0]);
            
            foreach (var item in details)
            {
                if (parent!=null)
                {
                    parent.FileExplorer.Add(new Dir(item));
                    parent= parent.FileExplorer.First(f => f.Name == item);
                }
                else if (Root.FileExplorer.Contains(new Dir(item)))
                {
                    parent = Root.FileExplorer.First(f => f.Name == item);  
                }
                else
                {
                    Root.FileExplorer.Add(new Dir(item));
                    parent= Root.FileExplorer.First(f => f.Name == item);  
                }
            }
        }

        public async Task DeleteAsync(BlobItem blobItem)
        {
            await Repository.Container.GetBlobClient(blobItem.Name).DeleteAsync();
            Refresh();
        }

        public async Task DownloadAsync(BlobItem blobItem, string fileName)
        {
            using (var fs=File.OpenWrite(fileName))
            {
                await Repository.Container.GetBlobClient(blobItem.Name).DownloadToAsync(fs);
            }
        }

        public async Task UploadAsync(string path, string dir)
        {
            string fileName = path.Substring(path.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            if (!string.IsNullOrEmpty(dir))
            {
                fileName = $"{Direcotry}{ForwardSlash}{fileName}";
            }
            using (var fs = File.OpenRead(path))
            {
                await Repository.Container.GetBlobClient(fileName).UploadAsync(fs,true);
            }
            Refresh();
        }
    }
}

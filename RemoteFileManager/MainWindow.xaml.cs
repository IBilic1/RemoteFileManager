using Azure.Storage.Blobs.Models;
using Microsoft.Win32;
using RemoteFileManager.Models;
using RemoteFileManager.ViewModels;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RemoteFileManager
{
    public partial class MainWindow : Window
    {
        private const string FILE_FILTER = "Images|*.jpeg;*.png;*.gif;*.tif;*.svg;";

        private readonly ItemsViewModel itemsViewModel;
        private Dir Root { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            itemsViewModel = new ItemsViewModel();
            Init();
        }

        private void Init()
        {
            TwFileExplorer.Items.Clear();
            Root = itemsViewModel.Root;
            foreach (var item in Root.FileExplorer)
            {
               TwFileExplorer.Items.Add(CreateDirectoryNode(item));
            }
        }

        private TreeViewItem CreateDirectoryNode(Dir dir)
        {
            var directoryNode = new TreeViewItem { Header = dir.Name, Name=dir.Name };
            foreach (var directory in dir.FileExplorer)
            {
                directoryNode.Items.Add(CreateDirectoryNode(directory));
            }
            return directoryNode;
        }

        private void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter=FILE_FILTER
            };
            
            if (openFileDialog.ShowDialog()==true)
            {
                uploadFileToSotrage(openFileDialog.FileName);
               
            }
        }

        private async void uploadFileToSotrage(string fileName)
        {
            string fileExtension = fileName.Substring(fileName.LastIndexOf(".") + 1);

            if (itemsViewModel.Direcotry.StartsWith(fileExtension.ToUpper()))
            {
                await itemsViewModel.UploadAsync(fileName, itemsViewModel.Direcotry);
                Init();
            }
            else {
                MessageBox.Show($"Picture of format {fileExtension} cant be in the file {itemsViewModel.Direcotry}");
            }
        }

        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (!(TwFileExplorer.SelectedItem is BlobItem blobItem))
            {
                return;
            }
            var saveFileDialog = new SaveFileDialog
            {
               FileName=blobItem.Name.Contains(ItemsViewModel.ForwardSlash) ? blobItem.Name.Substring(blobItem.Name.LastIndexOf(ItemsViewModel.ForwardSlash)+1) : blobItem.Name
            };
            if (saveFileDialog.ShowDialog()==true)
            {
                await itemsViewModel.DownloadAsync(blobItem,saveFileDialog.FileName);
            }            
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!(TwFileExplorer.SelectedItem is BlobItem blobItem))
            {
                return;
            }
            await itemsViewModel.DeleteAsync(blobItem);
            Init();
        }
        private void TwFileExplorer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TwFileExplorer.SelectedItem is BlobItem blobItem)
            {
                DataContext = TwFileExplorer.SelectedItem as BlobItem;
            }
            else
            {
                TreeViewItemMouseClick();
            }
        }

        private void TreeViewItemMouseClick()
        {
            TreeViewItem selectedItem = (TreeViewItem)TwFileExplorer.SelectedItem;
            selectedItem.IsExpanded = true;

            StringBuilder path = new StringBuilder();
            getAllParents(selectedItem,path);

            if (itemsViewModel.Directories.Contains(path.Append(selectedItem.Header).ToString()))
            {
                itemsViewModel.Direcotry = path.ToString();
                selectedItem.DisplayMemberPath = "Name";
               
                new List<BlobItem>(itemsViewModel.Items).ForEach(bi =>
                {
                    if (!clearAllBlobItems(selectedItem.Items,bi))
                    {
                        selectedItem.Items.Add(bi);
                    }
                });
            }
        }

        private bool clearAllBlobItems(ItemCollection items,BlobItem blobItem)
        {
            for (int i = 0; i < items.Count; i++) //iterator
            {
                if ((items[i] is BlobItem _BlobItem) && _BlobItem.Name.Equals(blobItem.Name))
                {
                    return true;
                }
            }
            return false;
        }


        private string getAllParents(TreeViewItem dependencyObject,StringBuilder path)
        {
            if (dependencyObject.Parent is TreeView)
            {
                return path.ToString();
            }
            path.Insert(0,((TreeViewItem)dependencyObject.Parent).Header+ItemsViewModel.ForwardSlash);
            return getAllParents( dependencyObject.Parent as TreeViewItem,path);
        }

        private void TbDirecotry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (TwFileExplorer.SelectedItem!=null && TwFileExplorer.SelectedItem is TreeViewItem selectedItem)
                {
                    TbDirecotry.Text= selectedItem.Header+ ItemsViewModel.ForwardSlash + TbDirecotry.Text;
                }
                itemsViewModel.Direcotry = TbDirecotry.Text;
            }
        }
    }
}

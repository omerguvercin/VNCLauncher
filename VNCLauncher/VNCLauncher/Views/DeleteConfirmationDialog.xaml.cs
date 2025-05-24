using System.Windows;
using VNCLauncher.Models;

namespace VNCLauncher.Views
{
    public partial class DeleteConfirmationDialog : Window
    {
        public VncConnection Connection { get; private set; }
        
        // Parametre olarak bağlantı nesnesi alır
        public DeleteConfirmationDialog(VncConnection connection)
        {
            InitializeComponent();
            Connection = connection;
            
            // Mesajı özelleştir
            txtMessage.Text = $"'{connection.Name}' bağlantısını silmek istediğinizden emin misiniz?";
        }
        
        // Onaylama butonu tıklandığında
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        
        // İptal butonu tıklandığında
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PizzeriaChainApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var admin = PizzeriaChainApp.Data.AppData.GetContext()
    .customer.FirstOrDefault(c => c.login == "admin");
            MessageBox.Show(admin != null &&
                PizzeriaChainApp.Helpers.PasswordHelper.Verify("Admin123!", admin.salt, admin.password_hash)
                ? "База подключена, пароль проверен" : "Что-то не так");
        }

    }
}

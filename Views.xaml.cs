using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using VMS.TPS.Common.Model.API;

namespace ProyectoLA_Structures_Vs1
{
    /// <summary>
    /// Lógica de interacción para Views.xaml
    /// </summary>
    public partial class Views : UserControl
    {
        public Views(StructureSet ss, Patient paciente, VMS.TPS.Common.Model.API.Image img)
        {
            InitializeComponent();
            PatientName.Text = paciente.Name;
            PatientID.Text = paciente.Id;
            StructSet.Text = ss.Id;
            CT.Text = img.Id;
            my_ss = ss;
            Creation = new CreationStructures(ss);
            this.DataContext = Creation;
            CB1.ItemsSource = Creation.TemplatesName;
        }

        public CreationStructures Creation;
        public StructureSet my_ss;
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            Creation = new CreationStructures(my_ss,Selected.Text);
            Creation.StartCreation();
            MessageBox.Show("Proceso Terminado. Verifique la correcta ejecución de la tarea.");
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Credits_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Proyecto de Estandarización LatinoAmericano\nMSc. Alberto Alarcón Paredes\nMagister en Física Médica\n Universidad Mayor de San Simón \n Instituto Balseiro-FUESMEN-Universidad Nacional del Cuyo");
        }

        private void CB1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Apply.IsEnabled = true;
            Selected.Text = CB1.SelectedValue.ToString();
        }
    }
}

using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using ProyectoLA_Structures_Vs1;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.17")]
[assembly: AssemblyFileVersion("1.0.0.17")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Execute(ScriptContext context, System.Windows.Window window /*, ScriptEnvironment environment*/)
    {
            Licence();
            Image img = context.Image;
            StructureSet ss = context.StructureSet;
            Patient paciente = context.Patient;
            paciente.BeginModifications();
            var MainWindow = new Views(ss, paciente, img);
            window.Content = MainWindow;
            window.Height = MainWindow.Height;
            window.Width = MainWindow.Width;
            window.Title = "ProyectoLA Creacion de Estructuras";
            
    }
        private void Licence()
        {
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            List<string> users = new List<string>{ @"SDE\alberto" , @"SDE\alejandro", @"SDE\federico", @"SDE\nicolas", @"SDE\wtrinca" };
            if (!users.Any(x => x == userName))
            {
                MessageBox.Show("No esta autorizado para Ejecutar este Plug-In. \nPongase en contacto con el autor: alarcon.alberto01@gmail.com");
                System.Windows.Application.Current.Shutdown();
            }
        }
  }
}

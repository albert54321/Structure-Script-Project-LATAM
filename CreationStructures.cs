using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ProyectoLA_Structures_Vs1
{
    public class CreationStructures:INotifyPropertyChanged
    {
        //Propiedades:
        private string template;
        public string Template
        {
            get { return template; }
            set { 
                template = value;
                NotifyPropertyChanged("Template");
            }
        }

        //public string Template { get; set; }
        public string FileName { get; set; }
        
        public List<string> TemplatesName { get; set; }

        public StructureSet My_ss { get; set; }

        //Eventos:
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //Metodos de la clase:
        public CreationStructures(StructureSet ss, string data_template="@@")
        {
            FileName = "./Data/StructuresOperation.csv";
            My_ss = ss;
            TemplatesName= FindTemplatesNames(Read_File(FileName));
            //Template = "Prostate_CHHIP_60GY/20Fx_IR-HR";
            Template = data_template;
        }

        public void StartCreation()
        {
            List<string> AllLines = Read_File(FileName);
            //Read_File(FileName, My_ss);
            foreach(var lines in AllLines) ExecuteOperation(lines, My_ss);
        }

        private void ExecuteOperation(string lines, StructureSet ss)
        {
            string[] optionline = SeparateLines(lines);
            bool margen = !(optionline[6]==""|| optionline[6]=="N/A"|| optionline[6]=="0");
            if (optionline[0] == Template)
            {
                if (optionline[1] == "Start")
                {
                    Start(optionline, ss);
                }
                else if (optionline[1] == "MarginAsymmetric")
                {
                    MarginAsymmetric(optionline, ss);
                }
                else if (optionline[1] == "Margin")
                {
                    Margin(optionline, ss);
                }
                else if (optionline[1] == "Union")
                {
                    Union(optionline, ss, margen);
                }
                else if (optionline[1] == "Intersection")
                {
                    Intersection(optionline, ss, margen);
                }
                else if (optionline[1] == "Substraction")
                {
                    Substraction(optionline, ss, margen);
                }
            }
        }

        private void Substraction(string[] optionline, StructureSet ss,bool margen=false)
        {
            Structure aux = Add_AuxStructure(ss);
            Structure st1 = NullEmpty(ss,optionline[2]);
            Structure st2 = NullEmpty(ss,optionline[5]);
            Structure st_new = Add_Structure(ss,optionline);
            if (margen)
            {
                double margen_value = Convert.ToDouble(optionline[6]);
                aux.SegmentVolume = st1.Margin(Math.Abs( margen_value ));
                if (margen_value < 0) st_new.SegmentVolume = st2.Sub(aux);
                else st_new.SegmentVolume = aux.Sub(st2);
            }
            else st_new.SegmentVolume = st1.Sub(st2);
            ss.RemoveStructure(aux);
        }

        private void Intersection(string[] optionline, StructureSet ss, bool margen=false)
        {
            Structure aux = Add_AuxStructure(ss);
            Structure st1 = NullEmpty(ss, optionline[2]);
            Structure st2 = NullEmpty(ss, optionline[5]);
            Structure st_new = Add_Structure(ss, optionline);
            if (margen)
            {
                aux.SegmentVolume = st1.Margin(Convert.ToDouble(optionline[6]));
                st_new.SegmentVolume = aux.And(st2);
            }
            else st_new.SegmentVolume = st1.And(st2);
            ss.RemoveStructure(aux);
        }

        private void Union(string[] optionline, StructureSet ss, bool margen=false)
        {
            Structure aux= Add_AuxStructure(ss);
            Structure st1 = NullEmpty(ss, optionline[2]);
            Structure st2 = NullEmpty(ss, optionline[5]);
            Structure st_new = Add_Structure(ss, optionline);
            if (margen)
            {
                aux.SegmentVolume = st1.Margin(Convert.ToDouble(optionline[6]));
                st_new.SegmentVolume = aux.Or(st2);
            }
            else st_new.SegmentVolume = st1.Or(st2);
            ss.RemoveStructure(aux);
        }

        private void Margin(string[] optionline, StructureSet ss)
        {
            Structure st1 = NullEmpty(ss, optionline[2]);
            Structure st_new = Add_Structure(ss, optionline);
            st_new.SegmentVolume = st1.Margin( Convert.ToDouble(optionline[6]));
        }

        private void MarginAsymmetric(string[] optionline, StructureSet ss)
        {
            Structure st1 = NullEmpty(ss, optionline[2]);
            Structure st_new = Add_Structure(ss, optionline);
            try
            {
                if (optionline[5] == "Inner") st_new.SegmentVolume = st1.AsymmetricMargin(new AxisAlignedMargins(StructureMarginGeometry.Inner, Math.Abs(Convert.ToDouble(optionline[6])), Math.Abs(Convert.ToDouble(optionline[7])), Math.Abs(Convert.ToDouble(optionline[8])), Math.Abs(Convert.ToDouble(optionline[9])), Math.Abs(Convert.ToDouble(optionline[10])), Math.Abs(Convert.ToDouble(optionline[11]))));
                else if (optionline[5] == "Outer") st_new.SegmentVolume = st1.AsymmetricMargin(new AxisAlignedMargins(StructureMarginGeometry.Outer, Math.Abs(Convert.ToDouble(optionline[6])), Math.Abs(Convert.ToDouble(optionline[7])), Math.Abs(Convert.ToDouble(optionline[8])), Math.Abs(Convert.ToDouble(optionline[9])), Math.Abs(Convert.ToDouble(optionline[10])), Math.Abs(Convert.ToDouble(optionline[11]))));
            }
            catch (Exception)
            {
                throw new Exception("Error de sintáxis, no coloco si el margen asimetrico es:\nInner(dentro) o Outer(hacia fuera) en la columna 5 o\n falta algun elemento de margen x1,y1,z1,x2,y2,z2.\n Corregir e intentar de nuevo.");
            }
        }

        private void Start(string[] optionline, StructureSet ss)
        {
            Structure st = ss.Structures.FirstOrDefault(x => optionline.Any(y => y == x.Id));
            if ((st == null || st.IsEmpty) && optionline[3] == "necessary") 
            { 
                throw new Exception("No se encuentra la estructura: " + optionline[2] + ", que es necesaria\nEl Script no se ejecutara");
            }
            if ((st == null || st.IsEmpty) && optionline[3] == "NOnecessary") MessageBox.Show("No se encuentra o esta vacia la estructura: " + optionline[2], "ProyectoLA", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        //Genericos
        private List<string> FindTemplatesNames(List<string> NamesNoSeparate)
        {
            List<string> templatesName = new List<string>();
            string[] linesSeparates;
            foreach (var line in NamesNoSeparate)
            {
                linesSeparates = SeparateLines(line);
                if (linesSeparates[0] == "PATOLOGIA") templatesName.Add(linesSeparates[1]);
            }
            return templatesName;
        }

        private List<string> Read_File(string filepath)
        {
            using (StreamReader reader = new StreamReader(filepath))
            {
                int row = 0;
                List<string> AllLines = new List<string>();
                string lines;
                while ((lines = reader.ReadLine()) != null)
                {
                    row++;
                    if (row == 1 || row == 2 || row==3) continue;//salta las dos primeras lineas
                    //ExecuteOperation(lines, ss);
                    AllLines.Add(lines);
                }
                return AllLines;
            }
        }

        private string[] SeparateLines(string lines)
        {
            Regex CSVParser = new Regex(";(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            //Separating columns to array
            string[] optionline = CSVParser.Split(lines);//divide la linea con los signos mostrado anteriormente
            if (optionline.Length <= 1) optionline = lines.Split(',');
            return optionline;
        }

        private Structure Add_Structure(StructureSet ss, string[] optionline)
        {
            Structure st = ss.Structures.FirstOrDefault(x => x.Id == optionline[3]);
            if (st == null) st = ss.AddStructure(optionline[4], optionline[3]);
            return st;
        }

        private Structure Add_AuxStructure(StructureSet ss)
        {
            Structure st = ss.Structures.FirstOrDefault(x => x.Id == "Auxi");
            if (st == null) st = ss.AddStructure("CONTROL", "Auxi");
            return st;
        }

        private Structure NullEmpty(StructureSet ss, string name)
        {
            Structure st = ss.Structures.FirstOrDefault(x => x.Id == name);
            if (st == null || st.IsEmpty) throw new Exception("No se puede realizar la operacion dada con la estructura:" + st.Id);
            return st;
        }
    }
}

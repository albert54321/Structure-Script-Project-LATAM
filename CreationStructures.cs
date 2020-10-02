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
        public CreationStructures(StructureSet ss, string template="@@")
        {
            FileName = "./Data/StructuresOperation.csv";
            My_ss = ss;
            TemplatesName= FindTemplatesNames(Read_File(FileName));
            //Template = "Prostate_CHHIP_60GY/20Fx_IR-HR";
            Template = template;
        }

        private List<string> FindTemplatesNames(List<string> NamesNoSeparate)
        {
            List<string> templatesName = new List<string>();
            string[] linesSeparates;
            foreach (var line in NamesNoSeparate)
            {
                linesSeparates = SeparateLines(line);
                if (linesSeparates[0]=="PATOLOGIA" ) templatesName.Add(linesSeparates[1]);
            }
            return templatesName;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void StartCreation()
        {
            List<string> AllLines = Read_File(FileName);
            //Read_File(FileName, My_ss);
            foreach(var lines in AllLines) ExecuteOperation(lines, My_ss);
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
                    if (row == 1 || row == 2) continue;//salta las dos primeras lineas
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
            return optionline;
        }
        private void ExecuteOperation(string lines, StructureSet ss)
        {
            string[] optionline = SeparateLines(lines);
            if (optionline[0] == Template)
            {
                if (optionline[1] == "Start")
                {
                    Start(optionline, ss);
                }
                if (optionline[1] == "MarginAsymmetric")
                {
                    MarginAsymmetric(optionline,ss);
                }
                if (optionline[1] == "Margin")
                {
                    Margin(optionline,ss);
                }
                if (optionline[1] == "Union")
                {
                    Union(optionline,ss);
                }
                if (optionline[1] == "Intersection")
                {
                    Intersection(optionline,ss);
                }
                if (optionline[1] == "Substraction")
                {
                    Substraction(optionline,ss);
                }
            }
        }

        private void Substraction(string[] optionline, StructureSet ss)
        {
            Structure st1 = NullEmpty(ss,optionline[2]);
            Structure st2 = NullEmpty(ss,optionline[5]);
            Structure st_new = Add_Structure(ss,optionline);
            st_new.SegmentVolume = st1.Sub(st2);
        }

        private Structure Add_Structure(StructureSet ss, string[] optionline)
        {
            Structure st=ss.Structures.FirstOrDefault(x => x.Id == optionline[3]);
            if (st == null) st = ss.AddStructure(optionline[4],optionline[3]);
            return st;
        }

        private Structure NullEmpty(StructureSet ss, string name)
        {
            Structure st = ss.Structures.FirstOrDefault(x => x.Id == name);
            if (st == null || st.IsEmpty) throw new Exception("No se puede realizar la operacion dada con la estructura:"+st.Id);
            return st;
        }

        private void Intersection(string[] optionline, StructureSet ss)
        {
            Structure st1 = NullEmpty(ss, optionline[2]);
            Structure st2 = NullEmpty(ss, optionline[5]);
            Structure st_new = Add_Structure(ss, optionline);
            st_new.SegmentVolume = st1.And(st2);
        }

        private void Union(string[] optionline, StructureSet ss)
        {
            Structure st1 = NullEmpty(ss, optionline[2]);
            Structure st2 = NullEmpty(ss, optionline[5]);
            Structure st_new = Add_Structure(ss, optionline);
            st_new.SegmentVolume = st1.Or(st2);
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
            if (optionline[5]=="Inner") st_new.SegmentVolume = st1.AsymmetricMargin(new AxisAlignedMargins(StructureMarginGeometry.Inner, Convert.ToDouble(optionline[6]), Convert.ToDouble(optionline[7]), Convert.ToDouble(optionline[8]), Convert.ToDouble(optionline[9]), Convert.ToDouble(optionline[10]), Convert.ToDouble(optionline[11])));
            else if(optionline[5] == "Outer") st_new.SegmentVolume = st1.AsymmetricMargin(new AxisAlignedMargins(StructureMarginGeometry.Outer, Convert.ToDouble(optionline[6]), Convert.ToDouble(optionline[7]), Convert.ToDouble(optionline[8]), Convert.ToDouble(optionline[9]), Convert.ToDouble(optionline[10]), Convert.ToDouble(optionline[11])));
        }

        private void Start(string[] optionline, StructureSet ss)
        {
            Structure st = ss.Structures.FirstOrDefault(x => optionline.Any(y => y == x.Id));
            if ((st == null || st.IsEmpty) && optionline[3] == "necessary") 
            { 
                throw new Exception("No se encuentra la estructura: " + optionline[2] + ", que es necesaria\nEl Script no se ejecutara");
            }
            if ((st == null || st.IsEmpty) && optionline[3] == "NOnecessary") MessageBox.Show("No se encuentra o esta vacia la estructura: " + optionline[2]);
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SystemDependencies2
{
    class VM : INotifyPropertyChanged
    {
        const string DEPEND = "DEPEND";
        const string INSTALL = "INSTALL";
        const string REMOVE = "REMOVE";
        const string LIST = "LIST";

        string FILE;
        string[] commands;
        int dependantCount = 0;
        public List<string> Installed { get; set; } = new List<string>();
        public List<string> ExplicitlyInstalled { get; set; } = new List<string>();
        public List<Components> Dependencies { get; set; } = new List<Components>();
        public BindingList<string> View { get; set;  } = new BindingList<string>();
        public BindingList<string> Result { get; set; } = new BindingList<string>();
        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; NotifyChange(); }
        }
        public void GetScript()
        {
            View.Clear();
            Result.Clear();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                FILE = openFileDialog.FileName.ToString();
                using (StreamReader r = new StreamReader(FILE))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        View.Add(line);
                    }
                }
            }
        }
        private string[] FormatInput(string [] lines)
        {
            View.Clear();
            foreach(string line in lines)
            {
                commands = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (commands[0].ToString() == DEPEND)
                {
                    if (!(line.Length > 80) && CheckWordLength(commands))
                    {
                        View.Add(line);
                    }
                    else
                    {
                        MessageBox.Show("The line has more than 80 characters or any word in the line has more than 10 characters. Hence, the line is skipped!");
                    }
                }
            }
            foreach (string line in lines)
            {
                commands = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (commands[0].ToString() != DEPEND)
                {
                    if (!(line.Length > 80) && CheckWordLength(commands))
                    {
                        View.Add(line);
                    }
                    else
                    {
                        MessageBox.Show("The line has more than 80 characters or any word in the line has more than 10 characters. Hence, the line is skipped!");
                    }
                }
            }
            return View.ToArray();
        }
        private bool CheckWordLength(string[] items)
        {
            for(int i=0; i<items.Length; i++)
            {
                if(items[i].Length > 10)
                {
                    return false;
                }
            }
            return true;
        }
        public void ReadScript()
        {            
            if (FILE != null)
            {
                string[] lines = File.ReadAllLines(FILE);
                lines = FormatInput(lines);
                foreach (string line in lines)
                {
                    Result.Add(line.ToString());
                    commands = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (commands[0].ToString() == DEPEND)
                    {
                        Depend();
                    }
                    if (commands[0].ToString() == INSTALL)
                    {
                        if (!Installed.Contains(commands[1].ToString()))
                        {
                            ExplicitlyInstalled.Add(commands[1].ToString());
                            Install(commands[1].ToString());
                        }
                        else
                            Result.Add("   " + commands[1].ToString() + " already installed");
                    }
                    if (commands[0].ToString() == REMOVE)
                    {
                        if (Installed.Contains(commands[1].ToString()))
                        {
                            dependantCount = 0;
                            Remove(commands[1].ToString());
                        }
                        else
                            Result.Add("   " + commands[1].ToString() + " not installed");
                    }
                    if (commands[0].ToString() == LIST)
                    {
                        List();
                    }
                }
            }
            else
            {
                Message = "Please select a file";
            }
        }
        public void Depend()
        {
            int count = commands.Length;
            List<Components> AddDeps = new List<Components>();
            for (int j = 2; j < count; j++)
            {
                AddDeps.Add(new Components { Name = commands[j].ToString()});
            }
            Dependencies.Add(
                new Components { Name = commands[1].ToString(), Dependants =  AddDeps });
        }
        public void Install(string name)
        {
            var temp = Dependencies.FirstOrDefault(x => x.Name == name);
            if (!Installed.Contains(name))
            {
                if (temp != null)
                 { 
                    if (temp.Dependants.Count > 0)
                    {
                        foreach (var list in temp.Dependants)
                        {
                            Install(list.Name);
                        }
                    }
                    Result.Add("   Installing " + name);
                }
                else
                {
                    Result.Add("   Installing " + name);
                }
                Installed.Add(name);
            }
        }
        public void Remove(string name)
        {
            int count = 0;
            foreach (string comp in Installed)
            {
                var temp = Dependencies.FirstOrDefault(x => x.Name == comp);
                if (temp != null)
                {
                    foreach (var components in temp.Dependants)
                        if (components.Name == name)
                            count++;
                }
            }
            if (count == 0)
            {
                dependantCount = 1;
                Result.Add("   Removing " + name);
                Installed.Remove(name);
                var temp = Dependencies.FirstOrDefault(x => x.Name == name);
                if (temp != null)
                {
                    foreach (var components in temp.Dependants)
                        if (!ExplicitlyInstalled.Contains(components.Name))
                            Remove(components.Name);
                }
            }
            else if (count > 0 && dependantCount != 1)
                Result.Add("   " + name + " is still needed");
        }
        public void List()
        {
            foreach (string comp in Installed)
                Result.Add("   " + comp);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChange([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}

﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace RubikTimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rubikfiles");
        public static string filepath = Path.Combine(path, "config.txt");
        private MainWindow main;

        public static string[] configitems =
        {
            "DoCountdown = ",
            "CountDownSecs = ",
            "DoCounting = ",
            "AutoScrambleGenerate = ",
            "ScrambleLenght = ",
            "PuzzleType = ",
            "DontSaveStats = ",
            "UserFolderPath = ",
            "CurrentFileName = "
            };

        private string[] defconfig =
        {
            "True",
            "15",
            "True",
            "True",
            "20",
            "2",
            "False",
            Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"RubikTimer")),
            "rubikstat0"
            };

        private void Start(object sender, StartupEventArgs e)
        {
            string[] config = new string[configitems.Length];
            for (int i = 0; i < config.Length; i++) config[i] = configitems[i] + defconfig[i];

            try
            {
                if (!File.Exists(filepath))
                {
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    File.WriteAllLines(filepath, config);
                }

                else
                {
                    config = File.ReadAllLines(filepath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception was thrown while operating with the config file: " + ex.Message, "Config file exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            bool docountdown = bool.Parse(defconfig[0]);
            byte countdownsecs = byte.Parse(defconfig[1]);
            bool docounting = bool.Parse(defconfig[2]);
            bool autoscramblegenerate = bool.Parse(defconfig[3]);
            byte scramblesteps = byte.Parse(defconfig[4]);
            byte puzzletype = byte.Parse(defconfig[5]);
            bool dontsave = bool.Parse(defconfig[6]);
            string userfolderpath = defconfig[7];
            string currentfilename = defconfig[8];
            
            foreach(string conf in config)
            {
                foreach(string confitem in configitems)
                {
                    if (conf.StartsWith(confitem))
                    {
                        switch (Array.IndexOf(configitems, confitem))
                        {
                            case 0: if (!bool.TryParse(conf.Substring(confitem.Length), out docountdown)) docountdown = bool.Parse(defconfig[0]); break;
                            case 1: if (!byte.TryParse(conf.Substring(confitem.Length), out countdownsecs)) countdownsecs = byte.Parse(defconfig[1]); break;
                            case 2: if (!bool.TryParse(conf.Substring(confitem.Length), out docounting)) docounting = bool.Parse(defconfig[2]); break;
                            case 3: if (!bool.TryParse(conf.Substring(confitem.Length), out autoscramblegenerate)) autoscramblegenerate = bool.Parse(defconfig[3]); break;
                            case 4: if (!byte.TryParse(conf.Substring(confitem.Length), out scramblesteps)) scramblesteps = byte.Parse(defconfig[4]); break;
                            case 5: if (!byte.TryParse(conf.Substring(confitem.Length), out puzzletype)) puzzletype = byte.Parse(defconfig[5]); break;
                            case 6: if (!bool.TryParse(conf.Substring(confitem.Length), out dontsave)) dontsave = bool.Parse(defconfig[6]); break;
                            case 7:
                                {
                                    userfolderpath = conf.Substring(confitem.Length);

                                    try { Directory.CreateDirectory(userfolderpath); }
                                    catch { userfolderpath = defconfig[7]; Directory.CreateDirectory(userfolderpath); }
                                    
                                    break;
                                }
                            case 8: currentfilename = conf.Substring(confitem.Length); break;
                        }
                    }
                }
            }

            main = new MainWindow(docountdown, countdownsecs, docounting, autoscramblegenerate, scramblesteps, puzzletype, dontsave, userfolderpath, currentfilename);
            main.Show();
        }
    }
}
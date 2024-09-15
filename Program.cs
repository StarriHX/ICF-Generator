using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICF_Generator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ICF Generator V1.00 by はくてい\n");
            if (args.Length != 0 && Directory.Exists(args[0]))
            {
                string[] _fileName = Directory.GetFiles(args[0]);
                List<string> fileName = new List<string>();
                List<string> fileTime = new List<string>();
                Console.WriteLine("AMFS Directory: " + args[0]);
                Console.WriteLine("Scaning files...");
                for (int i = 0; i < _fileName.Length; i++)
                {
                    if (_fileName[i].Split('.').Last() != "app" && _fileName[i].Split('.').Last() != "opt" && _fileName[i].Split('.').Last() != "pack")
                    {
                        Console.WriteLine("Unknown file: {0}", _fileName[i]);
                    }
                    else
                    {
                        fileName.Add(_fileName[i]);
                        fileTime.Add(_fileName[i].Split('_')[2]);
                    }
                }
                Console.WriteLine("\nGenerating JSON...");
                fileTime.Sort();
                string json = "[";
                string systemVersion = "";
                string sourceVersion = "";
                string sourceTime = "";
                for (int i = 0; i < fileTime.Count; i++)
                {
                    foreach (string _file in fileName)
                    {
                        string file = _file;
                        file = file.Split('\\').Last();
                        if (file.Contains(fileTime[i]))
                        {
                            Console.WriteLine(String.Format("[{0}/{1}] {2}", i + 1, fileName.Count, file));
                            if (file.Split('.').Last() == "pack")
                            {
                                string _version = file.Split('_')[1];
                                systemVersion = String.Format("{0}.{1}.{2}", int.Parse(_version.Split('.')[0]).ToString("D2"), int.Parse(_version.Split('.')[1]).ToString("D2"), int.Parse(_version.Split('.')[2]).ToString("D2"));
                                json += String.Format("\n  {{\n    \"type\": \"System\",\n    \"id\": \"ACA\",\n    \"version\": \"{0}\",\n    \"required_system_version\": \"{1}\",\n    \"datetime\": \"{2}\",\n    \"is_prerelease\": false\n  }},", systemVersion, systemVersion, TimeToString(file.Split('_')[2]));
                            }
                            if (file.Split('.').Last() == "app")
                            {
                                file = file.Replace(".app", "");
                                if (file.Split('_')[3] == "0")
                                {
                                    string id = file.Split('_')[0];
                                    sourceVersion = file.Split('_')[1];
                                    sourceTime = TimeToString(file.Split('_')[2]);
                                    json += String.Format("\n  {{\n    \"type\": \"App\",\n    \"id\": \"{0}\",\n    \"version\": \"{1}\",\n    \"required_system_version\": \"{2}\",\n    \"datetime\": \"{3}\",\n    \"is_prerelease\": false\n  }},", id, sourceVersion, systemVersion, TimeToString(file.Split('_')[2]));
                                }
                                else
                                {
                                    string id = file.Split('_')[3];
                                    string version = file.Split('_')[1];
                                    json += String.Format("\n  {{\n    \"type\": \"Patch\",\n    \"sequence_number\": {0},\n    \"source_version\": \"{1}\",\n    \"source_datetime\": \"{2}\",\n    \"source_required_system_version\": \"{3}\",\n    \"target_version\": \"{4}\",\n    \"target_datetime\": \"{5}\",\n    \"target_required_system_version\": \"{6}\",\n    \"is_prerelease\": false\n  }},", id, sourceVersion, sourceTime, systemVersion, version, TimeToString(file.Split('_')[2]), systemVersion);
                                }
                            }
                            if (file.Split('.').Last() == "opt")
                            {
                                string id = file.Split('_')[1];
                                json += String.Format("\n  {{\n    \"type\": \"Option\",\n    \"option_id\": \"{0}\",\n    \"datetime\": \"{1}\",\n    \"is_prerelease\": false\n  }},", id, TimeToString(file.Split('_')[2]));
                            }
                        }
                    }
                }
                json = json.Substring(0, json.Length - 1);
                json += "\n]";

                Console.WriteLine("\nGenerated JSON:\n");
                Console.WriteLine("----------BEGIN----------");
                Console.WriteLine(json);
                Console.WriteLine("----------END----------");
                Console.WriteLine("\nGenerate complete");
                File.WriteAllText("ICF.json", json);
                Console.WriteLine("Preparing file");
                Console.WriteLine("Calling Third-Party program to generate ICF file...");
                Process process = new Process();
                process.StartInfo.FileName = "icf-reader.exe";
                process.StartInfo.Arguments = "encode ICF.json ICF";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = false;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Console.WriteLine("\nICF json: {0}", Environment.CurrentDirectory + "\\ICF.json");
                Console.WriteLine("ICF file: {0}", Environment.CurrentDirectory + "\\ICF");
                Console.WriteLine("ICF generate complete.");
            }
            else
            {
                Console.WriteLine("ERROR: No directory selected");
                Console.WriteLine("Usage: ICF-Generator.exe [AMFS Path]");
            }
        }

        static string TimeToString(string Time)
        {
            return String.Format("{0}-{1}-{2}T{3}:{4}:{5}", Time.Substring(0, 4), Time.Substring(4, 2), Time.Substring(6, 2), Time.Substring(8, 2), Time.Substring(10, 2), Time.Substring(12, 2));
        }
    }
}
